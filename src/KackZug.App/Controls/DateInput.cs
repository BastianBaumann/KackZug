using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using KackZug.Core.Planning;

namespace KackZug.Controls;

/// <summary>Datumsfeld, das die Punkte beim Tippen selbst setzt; die Zahlentastatur am Handy hat oft keinen</summary>
public static class DateInput
{
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<TextBox, bool>("IsEnabled", typeof(DateInput));

    /// <remarks>Wie beim Uhrzeitfeld: haengt an der Text-Eigenschaft, die Schreibmarke kommt danach noch einmal ans Ende.</remarks>
    static DateInput()
    {
        TextBox.TextProperty.Changed.AddClassHandler<TextBox>(OnTextChanged);
    }

    public static bool GetIsEnabled(TextBox textBox)
    {
        return textBox.GetValue(IsEnabledProperty);
    }

    public static void SetIsEnabled(TextBox textBox, bool value)
    {
        textBox.SetValue(IsEnabledProperty, value);
    }

    private static void OnTextChanged(TextBox textBox, AvaloniaPropertyChangedEventArgs e)
    {
        if (!GetIsEnabled(textBox))
        {
            return;
        }

        var formatted = DateText.FormatTyping(textBox.Text);
        if (formatted == (textBox.Text ?? ""))
        {
            return;
        }

        textBox.Text = formatted;
        textBox.CaretIndex = formatted.Length;
        Dispatcher.UIThread.Post(() => textBox.CaretIndex = (textBox.Text ?? "").Length);
    }
}
