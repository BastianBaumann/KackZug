using CommunityToolkit.Mvvm.Input;

namespace KackZug.ViewModels;

/// <summary>Einfache Rueckfrage mit Abbrechen und Bestaetigen</summary>
public partial class ConfirmViewModel : ViewModelBase
{
    private readonly TaskCompletionSource<bool> _result = new TaskCompletionSource<bool>();

    public ConfirmViewModel(string title, string message, string confirmText)
    {
        Title = title;
        Message = message;
        ConfirmText = confirmText;
    }

    public string Title { get; }

    public string Message { get; }

    public string ConfirmText { get; }

    public Task<bool> Result
    {
        get { return _result.Task; }
    }

    [RelayCommand]
    private void Confirm()
    {
        _result.TrySetResult(true);
    }

    [RelayCommand]
    private void Cancel()
    {
        _result.TrySetResult(false);
    }
}
