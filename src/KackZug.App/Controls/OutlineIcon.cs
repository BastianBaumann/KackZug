using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;

namespace KackZug.Controls;

/// <summary>Zeichnet ein Outline-Symbol aus dem Designsystem (24er Raster, 2px Strich) in der Schriftfarbe</summary>
public sealed class OutlineIcon : Control
{
    public static readonly StyledProperty<Geometry?> DataProperty =
        AvaloniaProperty.Register<OutlineIcon, Geometry?>(nameof(Data));

    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        TextElement.ForegroundProperty.AddOwner<OutlineIcon>();

    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<OutlineIcon, double>(nameof(StrokeThickness), 2);

    static OutlineIcon()
    {
        AffectsRender<OutlineIcon>(DataProperty, ForegroundProperty, StrokeThicknessProperty);
    }

    public Geometry? Data
    {
        get { return GetValue(DataProperty); }
        set { SetValue(DataProperty, value); }
    }

    public IBrush? Foreground
    {
        get { return GetValue(ForegroundProperty); }
        set { SetValue(ForegroundProperty, value); }
    }

    public double StrokeThickness
    {
        get { return GetValue(StrokeThicknessProperty); }
        set { SetValue(StrokeThicknessProperty, value); }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var width = 24.0;
        var height = 24.0;
        if (!double.IsNaN(Width))
        {
            width = Width;
        }

        if (!double.IsNaN(Height))
        {
            height = Height;
        }

        return new Size(width, height);
    }

    public override void Render(DrawingContext context)
    {
        var data = Data;
        var brush = Foreground;
        if (data is null || brush is null)
        {
            return;
        }

        var size = Math.Max(0, Math.Min(Bounds.Width, Bounds.Height));
        var scale = size / 24.0;
        var pen = new Pen(brush, StrokeThickness, null, PenLineCap.Round, PenLineJoin.Round);
        using (context.PushTransform(Matrix.CreateScale(scale, scale)))
        {
            context.DrawGeometry(null, pen, data);
        }
    }
}
