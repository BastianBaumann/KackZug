#if DEBUG
using Avalonia.Controls;
using KackZug.ViewModels;

namespace KackZug.Desktop;

/// <summary>Zusaetzliche Ablaeufe der Screenshot-Tour, wird mit jeder Ansicht erweitert</summary>
internal static class TourSteps
{
    public static async Task RunAsync(MainViewModel main, Window window, string directory, Action<bool, string> check, Func<TopLevel, string, Task> capture)
    {
        Avalonia.Application.Current!.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Light;

        main.Navigate("shifts");
        var shifts = (ShiftsViewModel)main.CurrentPage!;
        check(shifts.Shifts.Count == 2, "Schichtenliste zeigt 2 Demo-Schichten");
        shifts.EditCommand.Execute(shifts.Shifts[0]);
        check(shifts.IsEditing, "Editor geoeffnet");
        check(shifts.Editor!.Outbound.Legs.Count == 3, "Hinfahrt zeigt 3 Teilstrecken");
        await capture(window, Path.Combine(directory, "shift-editor.png"));
        shifts.Editor.CancelCommand.Execute(null);
        check(!shifts.IsEditing, "Editor geschlossen");

        shifts.AddCommand.Execute(null);
        check(!shifts.Editor!.SaveCommand.CanExecute(null), "Neue Schicht ohne Namen nicht speicherbar");
        shifts.Editor.Name = "Nachtdienst";
        check(shifts.Editor.SaveCommand.CanExecute(null), "Neue Schicht mit Namen speicherbar");
        shifts.Editor.SaveCommand.Execute(null);
        check(shifts.Shifts.Count == 3, "Neue Schicht in der Liste");
        await capture(window, Path.Combine(directory, "shifts-after-add.png"));

        shifts.EditCommand.Execute(shifts.Shifts[2]);
        var deleteTask = shifts.Editor!.DeleteCommand.ExecuteAsync(null);
        await Task.Delay(300);
        check(main.HasDialog, "Rueckfrage beim Loeschen");
        await capture(window, Path.Combine(directory, "shift-delete-confirm.png"));
        main.Dialog!.ConfirmCommand.Execute(null);
        await deleteTask;
        check(shifts.Shifts.Count == 2, "Schicht geloescht");

        main.Services.Resolver = new DemoResolver();
        await main.StartImportAsync("nur kaputt");
        check(main.Import!.HasError, "Import ohne Link zeigt Fehler");
        await capture(window, Path.Combine(directory, "import-error.png"));
        main.Import.CloseCommand.Execute(null);

        await main.StartImportAsync("https://int.bahn.de/en/buchung/start?vbid=demo");
        var import = main.Import!;
        check(import.IsReady && import.Legs.Count == 1, "Import zeigt die Verbindung");
        check(import.IsNewTarget && !import.SaveCommand.CanExecute(null), "Neue Schicht braucht einen Namen");
        await capture(window, Path.Combine(directory, "import-new.png"));

        var spaet = import.Targets.First(target => target.Title == "Spätdienst");
        import.SelectTargetCommand.Execute(spaet);
        check(import.IsReturn, "Spaetdienst ohne Rueckfahrt schlaegt Rueckfahrt vor");
        check(!import.HasReplacement, "Nichts wird ersetzt");
        import.ChooseOutboundCommand.Execute(null);
        check(import.HasReplacement, "Hinfahrt ersetzen zeigt vorher und nachher");
        await capture(window, Path.Combine(directory, "import-replace.png"));
        import.ChooseReturnCommand.Execute(null);
        import.SaveCommand.Execute(null);
        check(!main.HasImport, "Import geschlossen nach Speichern");
        var saved = main.Services.Data.Shifts.First(shift => shift.Name == "Spätdienst");
        check(saved.Return is not null && saved.Return.Legs[0].Name == "RB21", "Rueckfahrt gespeichert");
        await capture(window, Path.Combine(directory, "shifts-after-import.png"));
    }
}
#endif
