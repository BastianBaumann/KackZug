using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using KackZug.Core.Planning;

namespace KackZug.Controls;

/// <summary>Uhrzeitfeld, das den Doppelpunkt beim Tippen selbst setzt; die Zahlentastatur am Handy hat oft keinen</summary>
public static class TimeInput
{
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<TextBox, bool>("IsEnabled", typeof(TimeInput));

    /// <remarks>Haengt an der Text-Eigenschaft statt am TextChanged-Ereignis, das erst spaeter kommt.
    /// Die Schreibmarke setzt das Feld nach einer Eingabe selbst, deshalb kommt sie danach noch einmal ans Ende.</remarks>
    static TimeInput()
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

        var formatted = ClockText.FormatTyping(textBox.Text);
        if (formatted == (textBox.Text ?? ""))
        {
            return;
        }

        textBox.Text = formatted;
        textBox.CaretIndex = formatted.Length;
        Dispatcher.UIThread.Post(() => textBox.CaretIndex = (textBox.Text ?? "").Length);
    }
}
