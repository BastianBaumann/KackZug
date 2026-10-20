using Avalonia.Controls;
using Avalonia.Controls.Templates;
using KackZug.ViewModels;

namespace KackZug;

/// <summary>Ordnet jedem ViewModel fest seine View zu</summary>
public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        switch (param)
        {
            default:
                return new TextBlock { Text = "Keine Ansicht fuer " + param?.GetType().Name };
        }
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
