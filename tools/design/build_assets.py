"""Builds app assets from the design system files in assets/.

- assets/logo/kackzug-app-icon.svg  -> src/KackZug.App/Assets/Logo.axaml (LogoImage)
- assets/logo/kackzug-mark.svg      -> Android adaptive icon foreground (+ orange background colour)
- assets/logo/kackzug-notification.svg -> Android notification small icon
- assets/icons/*.svg                -> src/KackZug.App/Themes/Icons.axaml (outline geometries)

Supports the small SVG subset the design system uses: g with transform (translate, scale),
path (incl. fill-rule), rect (with rx), circle and line; fill and stroke attributes.

Usage: python tools/design/build_assets.py
"""

import pathlib
import re
import xml.etree.ElementTree as ElementTree

ROOT = pathlib.Path(__file__).resolve().parents[2]
LOGO = ROOT / "assets" / "logo"
ICONS = ROOT / "assets" / "icons"
ANDROID_RESOURCES = ROOT / "src" / "KackZug.Android" / "Resources"
APP = ROOT / "src" / "KackZug.App"

SVG = "{http://www.w3.org/2000/svg}"
ICON_BACKGROUND = "#F26B1D"
ICON_KEYS = {
    "check-circle": "IconOk",
    "clock": "IconLate",
    "platform": "IconPlatform",
    "transfer": "IconTransfer",
    "x-circle": "IconCancelled",
    "alert-triangle": "IconWarning",
    "help-circle": "IconUnknown",
    "train": "IconTrain",
    "calendar": "IconWeek",
    "briefcase": "IconShifts",
    "settings": "IconSettings",
    "bell": "IconBell",
    "share": "IconShare",
    "moon": "IconFree",
    "arrow-right": "IconArrow",
    "refresh": "IconRefresh",
    "plus": "IconPlus",
    "trash": "IconDelete",
}
INHERITED = ("fill", "stroke", "stroke-width", "stroke-linecap", "stroke-linejoin", "fill-rule")


def number(text):
    return float(text)


def fmt(value):
    text = ("%.3f" % value).rstrip("0").rstrip(".")
    if text in ("-0", ""):
        return "0"
    return text


def multiply(first, second):
    a1, b1, c1, d1, e1, f1 = first
    a2, b2, c2, d2, e2, f2 = second
    return (
        a1 * a2 + c1 * b2,
        b1 * a2 + d1 * b2,
        a1 * c2 + c1 * d2,
        b1 * c2 + d1 * d2,
        a1 * e2 + c1 * f2 + e1,
        b1 * e2 + d1 * f2 + f1,
    )


def parse_transform(text):
    matrix = (1.0, 0.0, 0.0, 1.0, 0.0, 0.0)
    for name, arguments in re.findall(r"(\w+)\(([^)]*)\)", text):
        values = [number(value) for value in re.split(r"[ ,]+", arguments.strip())]
        if name == "translate":
            dy = 0.0
            if len(values) > 1:
                dy = values[1]
            step = (1.0, 0.0, 0.0, 1.0, values[0], dy)
        elif name == "scale":
            sy = values[0]
            if len(values) > 1:
                sy = values[1]
            step = (values[0], 0.0, 0.0, sy, 0.0, 0.0)
        else:
            raise ValueError("unsupported transform " + name)
        matrix = multiply(matrix, step)
    return matrix


def rect_path(element):
    x = number(element.get("x", "0"))
    y = number(element.get("y", "0"))
    width = number(element.get("width"))
    height = number(element.get("height"))
    radius = min(number(element.get("rx", "0")), width / 2, height / 2)
    if radius <= 0:
        return "M%s,%s h%s v%s h%s Z" % (fmt(x), fmt(y), fmt(width), fmt(height), fmt(-width))
    r = fmt(radius)
    return " ".join([
        "M%s,%s" % (fmt(x + radius), fmt(y)),
        "h%s" % fmt(width - 2 * radius),
        "a%s,%s 0 0 1 %s,%s" % (r, r, r, r),
        "v%s" % fmt(height - 2 * radius),
        "a%s,%s 0 0 1 -%s,%s" % (r, r, r, r),
        "h%s" % fmt(-(width - 2 * radius)),
        "a%s,%s 0 0 1 -%s,-%s" % (r, r, r, r),
        "v%s" % fmt(-(height - 2 * radius)),
        "a%s,%s 0 0 1 %s,-%s" % (r, r, r, r),
        "Z",
    ])


def circle_path(element):
    cx = number(element.get("cx"))
    cy = number(element.get("cy"))
    radius = number(element.get("r"))
    r = fmt(radius)
    return "M%s,%s a%s,%s 0 1 0 %s,0 a%s,%s 0 1 0 %s,0 Z" % (
        fmt(cx - radius), fmt(cy), r, r, fmt(2 * radius), r, r, fmt(-2 * radius))


def element_path(element, tag):
    if tag == "path":
        return element.get("d")
    if tag == "rect":
        return rect_path(element)
    if tag == "circle":
        return circle_path(element)
    if tag == "line":
        return "M%s,%s L%s,%s" % tuple(element.get(name) for name in ("x1", "y1", "x2", "y2"))
    return None


def collect(element, style, matrix, shapes):
    tag = element.tag.replace(SVG, "")
    if tag in ("title", "defs"):
        return
    own = dict(style)
    for name in INHERITED:
        if element.get(name) is not None:
            own[name] = element.get(name)
    own_matrix = matrix
    if element.get("transform") is not None:
        own_matrix = multiply(matrix, parse_transform(element.get("transform")))
    path = element_path(element, tag)
    if path is not None:
        shapes.append((path, own, own_matrix))
        return
    for child in element:
        collect(child, own, own_matrix, shapes)


def read_shapes(svg_path):
    root = ElementTree.parse(svg_path).getroot()
    shapes = []
    collect(root, {}, (1.0, 0.0, 0.0, 1.0, 0.0, 0.0), shapes)
    view_box = [number(value) for value in root.get("viewBox").split()]
    return shapes, view_box


def android_color(value):
    return "#FF" + value.lstrip("#").upper()


def android_vector(shapes, view_box, size_dp, content_scale, offset_x, offset_y, force_color=None):
    viewport = size_dp
    lines = [
        '<?xml version="1.0" encoding="utf-8"?>',
        "<!-- Generated by tools/design/build_assets.py from the design system assets -->",
        '<vector xmlns:android="http://schemas.android.com/apk/res/android"',
        '    android:width="%sdp" android:height="%sdp"' % (fmt(size_dp), fmt(size_dp)),
        '    android:viewportWidth="%s" android:viewportHeight="%s">' % (fmt(viewport), fmt(viewport)),
        '  <group android:translateX="%s" android:translateY="%s" android:scaleX="%s" android:scaleY="%s">'
        % (fmt(offset_x - view_box[0] * content_scale), fmt(offset_y - view_box[1] * content_scale), fmt(content_scale), fmt(content_scale)),
    ]
    for path, style, matrix in shapes:
        a, b, c, d, e, f = matrix
        if abs(b) > 1e-9 or abs(c) > 1e-9:
            raise ValueError("rotation not supported")
        lines.append('    <group android:translateX="%s" android:translateY="%s" android:scaleX="%s" android:scaleY="%s">'
                     % (fmt(e), fmt(f), fmt(a), fmt(d)))
        attributes = ['android:pathData="%s"' % path]
        fill = style.get("fill", "#000000")
        if force_color is not None and fill != "none":
            fill = force_color
        if fill != "none":
            attributes.append('android:fillColor="%s"' % android_color(fill))
        if style.get("fill-rule") == "evenodd":
            attributes.append('android:fillType="evenOdd"')
        stroke = style.get("stroke")
        if stroke is not None and stroke != "none":
            if force_color is not None:
                stroke = force_color
            attributes.append('android:strokeColor="%s"' % android_color(stroke))
            attributes.append('android:strokeWidth="%s"' % style.get("stroke-width", "1"))
            attributes.append('android:strokeLineCap="round"')
            attributes.append('android:strokeLineJoin="round"')
        lines.append("      <path " + " ".join(attributes) + " />")
        lines.append("    </group>")
    lines.append("  </group>")
    lines.append("</vector>")
    return "\n".join(lines) + "\n"


def build_android():
    shapes, view_box = read_shapes(LOGO / "kackzug-mark.svg")
    safe = 60.0
    scale = safe / max(view_box[2], view_box[3])
    offset_x = (108 - view_box[2] * scale) / 2
    offset_y = (108 - view_box[3] * scale) / 2
    drawable = ANDROID_RESOURCES / "drawable"
    (drawable / "icon_foreground.xml").write_text(android_vector(shapes, view_box, 108, scale, offset_x, offset_y), encoding="utf-8")

    note_shapes, note_box = read_shapes(LOGO / "kackzug-notification.svg")
    (drawable / "ic_notification.xml").write_text(android_vector(note_shapes, note_box, 24, 1.0, 0, 0, "#FFFFFF"), encoding="utf-8")

    adaptive = "\n".join([
        '<?xml version="1.0" encoding="utf-8"?>',
        '<adaptive-icon xmlns:android="http://schemas.android.com/apk/res/android">',
        '  <background android:drawable="@color/icon_background" />',
        '  <foreground android:drawable="@drawable/icon_foreground" />',
        '  <monochrome android:drawable="@drawable/ic_notification" />',
        "</adaptive-icon>",
    ])
    (ANDROID_RESOURCES / "mipmap-anydpi-v26" / "icon.xml").write_text(adaptive + "\n", encoding="utf-8")
    colors = '<?xml version="1.0" encoding="utf-8"?>\n<resources>\n  <color name="icon_background">%s</color>\n</resources>\n' % ICON_BACKGROUND
    (ANDROID_RESOURCES / "values" / "icon_colors.xml").write_text(colors, encoding="utf-8")


def avalonia_drawing(shapes, key, view_box):
    lines = ['    <DrawingImage x:Key="%s">' % key, "        <DrawingGroup>"]
    lines.append('            <GeometryDrawing Brush="Transparent" Geometry="M%s,%s h%s v%s h%s Z" />'
                 % (fmt(view_box[0]), fmt(view_box[1]), fmt(view_box[2]), fmt(view_box[3]), fmt(-view_box[2])))
    for path, style, matrix in shapes:
        lines.append("            <DrawingGroup>")
        lines.append("                <DrawingGroup.Transform>")
        lines.append('                    <MatrixTransform Matrix="%s" />' % ",".join(fmt(value) for value in matrix))
        lines.append("                </DrawingGroup.Transform>")
        geometry = path
        if style.get("fill-rule") == "evenodd":
            geometry = "F0 " + path
        drawing = '                <GeometryDrawing Geometry="%s"' % geometry
        fill = style.get("fill", "#000000")
        if fill != "none":
            drawing += ' Brush="%s"' % fill
        stroke = style.get("stroke")
        if stroke is not None and stroke != "none":
            lines.append(drawing + ">")
            lines.append("                    <GeometryDrawing.Pen>")
            lines.append('                        <Pen Brush="%s" Thickness="%s" LineCap="Round" LineJoin="Round" />'
                         % (stroke, style.get("stroke-width", "1")))
            lines.append("                    </GeometryDrawing.Pen>")
            lines.append("                </GeometryDrawing>")
        else:
            lines.append(drawing + " />")
        lines.append("            </DrawingGroup>")
    lines.append("        </DrawingGroup>")
    lines.append("    </DrawingImage>")
    return lines


def build_avalonia_logo():
    lines = [
        '<ResourceDictionary xmlns="https://github.com/avaloniaui"',
        '                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">',
        "    <!-- Generated by tools/design/build_assets.py from assets/logo -->",
    ]
    shapes, view_box = read_shapes(LOGO / "kackzug-app-icon.svg")
    lines.extend(avalonia_drawing(shapes, "LogoImage", view_box))
    mark_shapes, mark_box = read_shapes(LOGO / "kackzug-mark.svg")
    lines.extend(avalonia_drawing(mark_shapes, "LogoMarkImage", mark_box))
    lines.append("</ResourceDictionary>")
    (APP / "Assets" / "Logo.axaml").write_text("\n".join(lines) + "\n", encoding="utf-8")


def build_icons():
    lines = [
        '<ResourceDictionary xmlns="https://github.com/avaloniaui"',
        '                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">',
        "    <!-- Generated by tools/design/build_assets.py from assets/icons (design system outline icons, 24x24, stroke 2) -->",
    ]
    for svg_path in sorted(ICONS.glob("*.svg")):
        key = ICON_KEYS[svg_path.stem]
        shapes, _ = read_shapes(svg_path)
        parts = []
        for path, _, matrix in shapes:
            if matrix != (1.0, 0.0, 0.0, 1.0, 0.0, 0.0):
                raise ValueError("icons must not use transforms: " + svg_path.name)
            parts.append(path)
        lines.append('    <StreamGeometry x:Key="%s">%s</StreamGeometry>' % (key, " ".join(parts)))
    lines.append("</ResourceDictionary>")
    (APP / "Themes" / "Icons.axaml").write_text("\n".join(lines) + "\n", encoding="utf-8")


def main():
    build_android()
    build_avalonia_logo()
    build_icons()
    print("assets written")


if __name__ == "__main__":
    main()
