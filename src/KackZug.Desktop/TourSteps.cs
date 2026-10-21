#if DEBUG
using Avalonia.Controls;
using Avalonia.VisualTree;
using KackZug.ViewModels;

namespace KackZug.Desktop;

/// <summary>Zusaetzliche Ablaeufe der Screenshot-Tour, wird mit jeder Ansicht erweitert</summary>
internal static class TourSteps
{
    public static async Task RunAsync(MainViewModel main, Window window, string directory, Action<bool, string> check, Func<TopLevel, string, Task> capture)
    {
        Avalonia.Application.Current!.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Light;

        check(!main.HasTutorial, "Kein Tutorial, wenn diese Version schon gesehen");
        main.Services.Data.Settings.TutorialSeenVersion = "0.0.1";
        main.ShowTutorialIfNeeded();
        check(main.HasTutorial, "Tutorial nach Update");
        await capture(window, Path.Combine(directory, "tutorial-1.png"));
        main.Tutorial!.NextCommand.Execute(null);
        await capture(window, Path.Combine(directory, "tutorial-2.png"));
        main.Tutorial.NextCommand.Execute(null);
        main.Tutorial.NextCommand.Execute(null);
        main.Tutorial.NextCommand.Execute(null);
        check(main.Tutorial.IsLast, "Letzte Tutorial-Seite erreicht");
        await capture(window, Path.Combine(directory, "tutorial-5.png"));
        main.Tutorial.FinishCommand.Execute(null);
        check(!main.HasTutorial && main.Services.Store.Load().Settings.TutorialSeenVersion == Core.AppInfo.Version, "Tutorial als gesehen gespeichert");
        main.ShowTutorialIfNeeded();
        check(!main.HasTutorial, "Tutorial kommt nicht erneut");

        main.ShowUpdate(new Core.Updates.UpdateInfo("9.9.9", "https://github.com/BastianBaumann/KackZug/releases/tag/v9.9.9"));
        check(main.HasUpdate && main.UpdateText.Contains("9.9.9"), "Update-Hinweis sichtbar");
        main.Navigate("week");
        await capture(window, Path.Combine(directory, "update-banner.png"));
        main.DismissUpdateCommand.Execute(null);
        check(!main.HasUpdate, "Update-Hinweis ausblendbar");

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
        var colorButtons = window.GetVisualDescendants().OfType<Button>().Where(button => button.CommandParameter is ColorOption).ToList();
        check(colorButtons.Count == ColorOption.ShiftColors.Length && colorButtons.All(button => button.Command is not null), "Farbknoepfe haben einen Befehl");
        colorButtons[3].Command!.Execute(colorButtons[3].CommandParameter);
        check(shifts.Editor.Colors[3].IsSelected && !shifts.Editor.Colors[0].IsSelected, "Farbknopf waehlt die Farbe");
        shifts.Editor.SaveCommand.Execute(null);
        check(main.Services.Store.Load().Shifts[0].ColorHex == ColorOption.ShiftColors[3], "Neue Schichtfarbe gespeichert");
        shifts.EditCommand.Execute(shifts.Shifts[0]);
        shifts.Editor!.CancelCommand.Execute(null);
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
        check(refreshedToday.OutboundCaption is not null && refreshedToday.OutboundCaption.Contains("Anschluss knapp") && refreshedToday.DayBadge!.Text == "+8 · Anschluss knapp", "Jetzt pruefen zeigt Anschluss gefaehrdet");
        await capture(window, Path.Combine(directory, "week-checked.png"));
        check(monday.DayName == "Montag", "Wochentage auf Deutsch");

        var weekRadios = window.GetVisualDescendants().OfType<RadioButton>().Where(radio => radio.GroupName == "week").ToList();
        check(weekRadios.Count == 2, "Wochen-Umschalter sichtbar");
        weekRadios[1].IsChecked = true;
        check(week.WeekTitle == "Nächste Woche" && week.Days[0].Date == main.Services.Data.CurrentWeek!.WeekStart.AddDays(7), "Naechste Woche angezeigt");
        check(week.Days.All(day => !day.IsPast && !day.IsToday), "Naechste Woche komplett aenderbar");
        var nextMondayRow = week.Days[0];
        nextMondayRow.SelectedOption = nextMondayRow.Options.First(option => option.Name == "Spätdienst");
        var storedNext = main.Services.Store.Load().NextWeek;
        check(storedNext is not null && storedNext.GetDay(DayOfWeek.Monday).ShiftId == nextMondayRow.Shift!.Id, "Naechste Woche gespeichert");
        await capture(window, Path.Combine(directory, "week-next.png"));
        weekRadios[0].IsChecked = true;
        check(week.WeekTitle == "Diese Woche", "Zurueck zur laufenden Woche");

        var detailDay = week.Days.First(day => day.IsToday);
        week.OpenDetailCommand.Execute(detailDay);
        check(week.HasDetail, "Detailansicht geoeffnet");
        await week.Detail!.RefreshCommand.ExecuteAsync(null);
        var firstTrip = week.Detail.Trips[0];
        check(firstTrip.NowText == "jetzt 05:50" && firstTrip.Legs[0].Badge is not null && firstTrip.Legs[0].Badge!.Text == "+8 min", "Detail zeigt +8 und neue Zeit");
        check(firstTrip.Notes.Any(note => note.Contains("Anschluss gefährdet")), "Detail zeigt gefaehrdeten Umstieg");
        check(firstTrip.NavigatorUrl is not null && firstTrip.NavigatorUrl.StartsWith("https://www.bahn.de/buchung/fahrplan/suche#")
              && firstTrip.NavigatorUrl.Contains("hd=" + detailDay.Date.ToString("yyyy-MM-dd")), "Detail hat Navigator-Link fuer den Tag");
        await capture(window, Path.Combine(directory, "trip-detail.png"));
        check(window.GetVisualDescendants().OfType<TextBlock>().Any(text => text.Text == "Im DB Navigator öffnen" && text.IsEffectivelyVisible), "Navigator-Knopf sichtbar");
        check(main.HandleBack() && !week.HasDetail, "Zurueck schliesst die Detailansicht");

        var editable = week.Days.Where(day => day.IsEditable).Take(3).ToList();
        week.StartSelectingCommand.Execute(null);
        check(week.IsSelecting && week.Days.Where(day => day.IsPast).All(day => !day.CanMark), "Auswahlmodus, vergangene Tage nicht markierbar");
        foreach (var day in editable)
        {
            day.IsMarked = true;
        }

        week.BulkOption = week.BulkOptions.First(option => option.Name == "Spätdienst");
        await capture(window, Path.Combine(directory, "week-select.png"));
        week.ApplySelectionCommand.Execute(null);
        var currentPlan = main.Services.Store.Load().CurrentWeek!;
        var spaetId = main.Services.Data.Shifts.First(shift => shift.Name == "Spätdienst").Id;
        check(!week.IsSelecting && editable.All(day => currentPlan.GetDay(day.Date.DayOfWeek).ShiftId == spaetId), "Drei Tage auf einmal gesetzt und gespeichert");

        main.Navigate("settings");
        var settings = (SettingsViewModel)main.CurrentPage!;
        check(settings.LeadTimes.Count(option => option.IsChecked) == 3, "Drei Vorlaufzeiten aktiv");
        settings.LeadTimes.First(option => option.Minutes == 120).IsChecked = true;
        check(main.Services.Data.Settings.LeadTimesMinutes.SequenceEqual(new[] { 120, 90, 45, 15 }), "Vorlaufzeit 120 gespeichert");
        settings.ApplyPresetCommand.Execute(settings.Presets[1]);
        check(main.Services.Store.Load().Settings.AccentColor == "#00788A", "Farbvorlage gespeichert");
        await capture(window, Path.Combine(directory, "settings-preset.png"));
        settings.ThemeDark = true;
        check(main.Services.Data.Settings.ThemeMode == Core.Models.ThemeMode.Dark, "Dunkel gespeichert");
        await capture(window, Path.Combine(directory, "settings-dark-preset.png"));
        settings.ApplyPresetCommand.Execute(settings.Presets[0]);
        await capture(window, Path.Combine(directory, "settings-before-radio.png"));
        var themeRadios = window.GetVisualDescendants().OfType<RadioButton>().Where(radio => radio.GroupName == "theme").ToList();
        check(themeRadios.Count == 3, "Drei Theme-Knoepfe");
        themeRadios[2].IsChecked = true;
        check(themeRadios.Count(radio => radio.IsChecked == true) == 1 && main.Services.Data.Settings.ThemeMode == Core.Models.ThemeMode.Dark, "Dunkel per Knopf, nur einer aktiv");
        themeRadios[0].IsChecked = true;
        check(themeRadios.Count(radio => radio.IsChecked == true) == 1 && main.Services.Data.Settings.ThemeMode == Core.Models.ThemeMode.System, "Zurueck auf System per Knopf, nur einer aktiv");
        var numberField = window.GetVisualDescendants().OfType<NumericUpDown>().First();
        check(Math.Abs(numberField.Bounds.Height - 48) < 1, "Zahlenfeld 48 hoch: " + numberField.Bounds.Height);
        var timeField = window.GetVisualDescendants().OfType<TextBox>().First(box => box.Classes.Contains("time"));
        check(timeField.Text == "20:00" && Math.Abs(timeField.Bounds.Height - 48) < 1, "Uhrzeit als Textfeld 20:00, 48 hoch");
        timeField.Text = "1930";
        check(main.Services.Store.Load().Settings.EveningCheckTime == new TimeOnly(19, 30) && !settings.EveningCheckTimeInvalid, "Uhrzeit 1930 gespeichert");
        settings.TidyEveningCheckTime();
        check(timeField.Text == "19:30", "Uhrzeit aufgeraeumt zu 19:30");
        timeField.Text = "25:00";
        check(settings.EveningCheckTimeInvalid && main.Services.Store.Load().Settings.EveningCheckTime == new TimeOnly(19, 30), "Falsche Uhrzeit zeigt Hinweis, alte bleibt");
        await capture(window, Path.Combine(directory, "settings-time-invalid.png"));
        settings.TidyEveningCheckTime();
        check(!settings.EveningCheckTimeInvalid && timeField.Text == "19:30", "Nach Verlassen wieder 19:30");

        main.Navigate("week");
        var weekPage = (WeekViewModel)main.CurrentPage!;
        var firstRow = weekPage.Days[0];
        var weekView = window.GetVisualDescendants().OfType<Views.WeekView>().FirstOrDefault();
        main.Navigate("settings");
        main.Navigate("week");
        check(ReferenceEquals(firstRow, weekPage.Days[0]), "Woche wird beim Tab-Wechsel nicht neu aufgebaut");
        check(weekView is not null && ReferenceEquals(weekView, window.GetVisualDescendants().OfType<Views.WeekView>().FirstOrDefault()), "Ansicht wird wiederverwendet");
        main.Navigate("settings");
        settings.ThemeSystem = true;
        settings.EveningCheckEnabled = false;
        check(main.Services.Data.Settings.EveningCheckTime is null, "Abend-Check abgeschaltet");
    }
}
#endif
