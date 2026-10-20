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

        main.Navigate("week");
        var firstWeek = (WeekViewModel)main.CurrentPage!;
        var mondayPlan = main.Services.Store.Load().CurrentWeek!.GetDay(DayOfWeek.Monday);
        check(mondayPlan.ShiftId is not null, "Startanzeige ueberschreibt den Plan nicht");
        check(firstWeek.Days[0].SelectedOption!.Name == "Frühdienst", "Montag zeigt Fruehdienst: " + firstWeek.Days[0].SelectedOption!.Name);

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

        main.Services.StatusSourceFactory = () => new DemoStatusSource();
        main.Navigate("week");
        var week = (WeekViewModel)main.CurrentPage!;
        check(week.Days.Count == 7, "Woche zeigt 7 Tage");
        var monday = week.Days[0];
        var todayRow = week.Days.First(day => day.IsToday);
        check(todayRow.IsEditable, "Heute ist aenderbar");
        var pastCount = week.Days.Count(day => day.IsPast);
        check(week.Days.Take(pastCount).All(day => !day.IsEditable), "Vergangene Tage gesperrt");
        todayRow.SelectedOption = todayRow.Options.First(option => option.Name == "Frühdienst");
        todayRow.ReturnEnabled = false;
        var plan = main.Services.Data.CurrentWeek!.GetDay(todayRow.Date.DayOfWeek);
        check(plan.ShiftId == todayRow.Shift!.Id && !plan.ReturnEnabled, "Aenderung im Plan gespeichert");
        var reloaded = main.Services.Store.Load().CurrentWeek!.GetDay(todayRow.Date.DayOfWeek);
        check(reloaded.ShiftId == plan.ShiftId, "Aenderung in der Datei");
        await week.CheckNowCommand.ExecuteAsync(null);
        var refreshedToday = week.Days.First(day => day.IsToday);
        check(refreshedToday.OutboundStatus is not null && refreshedToday.OutboundStatus.Text.Contains("Anschluss gefährdet"), "Jetzt pruefen zeigt Anschluss gefaehrdet");
        await capture(window, Path.Combine(directory, "week-checked.png"));
        check(monday.DayName == "Montag", "Wochentage auf Deutsch");
    }
}
#endif
