#if DEBUG
using System.Security.Cryptography;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using KackZug.Core.Localization;
using KackZug.Core.Models;
using KackZug.Core.Planning;
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
        check(refreshedToday.OutboundCaption is not null && refreshedToday.OutboundCaption.Contains("Anschluss knapp") && refreshedToday.OutboundStatus!.Text == "+8 · Anschluss knapp", "Jetzt pruefen zeigt Anschluss gefaehrdet");
        var bothTrips = week.Days.FirstOrDefault(day => day.OutboundStatus is not null && day.ReturnStatus is not null);
        check(bothTrips is not null && refreshedToday.OutboundStatus.Text != "—", "Hin- und Rueckfahrt haben je ein eigenes Kennzeichen");
        await capture(window, Path.Combine(directory, "week-checked.png"));
        var todayCard = window.GetVisualDescendants().OfType<Border>().First(border => border.Classes.Contains("day") && border.Classes.Contains("today"));
        todayCard.BringIntoView();
        await SettleAsync();
        var cardHeight = todayCard.Bounds.Height;
        refreshedToday.OutboundEnabled = false;
        await SettleAsync();
        check(Math.Abs(todayCard.Bounds.Height - cardHeight) < 0.5 && refreshedToday.OutboundOff, "Fahrt abschalten verschiebt den Tag nicht");
        refreshedToday.OutboundEnabled = true;
        await SettleAsync();
        check(refreshedToday.OutboundChecked.StartsWith("geprüft "), "Fahrt zeigt, wann sie geprueft wurde: " + refreshedToday.OutboundChecked);
        await capture(window, Path.Combine(directory, "week-today.png"));
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
        var opened = new List<string>();
        ((Services.DefaultPlatform)main.Services.Platform).UrlOpener = url => opened.Add(url);
        var shareLinks = new DemoShareLinks();
        main.Services.ShareLinks = shareLinks;
        await firstTrip.OpenInNavigatorCommand.ExecuteAsync(null);
        check(opened.Count == 1 && opened[0] == "https://www.bahn.de" && main.HasToast, "Navigator ohne gefundene Verbindung oeffnet nur die App, mit Hinweis");
        shareLinks.Link = "https://www.bahn.de/buchung/start?vbid=demo";
        await firstTrip.OpenInNavigatorCommand.ExecuteAsync(null);
        check(opened.Count == 2 && opened[1] == shareLinks.Link && shareLinks.RequestedDays.All(day => day == detailDay.Date), "Navigator bekommt den Teilen-Link fuer diesen Tag");
        await firstTrip.OpenInNavigatorCommand.ExecuteAsync(null);
        check(opened.Count == 3 && shareLinks.RequestedDays.Count == 2, "Teilen-Link wird nur einmal erzeugt");
        var platform = (Services.DefaultPlatform)main.Services.Platform;
        platform.HasNavigator = false;
        main.Toast = null;
        await firstTrip.OpenInNavigatorCommand.ExecuteAsync(null);
        check(opened.Count == 4 && opened[3] == shareLinks.Link && main.Toast!.Contains("nicht installiert"), "Ohne Navigator oeffnet der Browser, mit Hinweis");
        platform.HasNavigator = true;
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
        check(!window.GetVisualDescendants().OfType<NumericUpDown>().Any(), "Keine Auf-ab-Zahlenfelder mehr");
        var numberFields = window.GetVisualDescendants().OfType<TextBox>().Where(box => box.Classes.Contains("number")).ToList();
        check(numberFields.Count == 2 && numberFields.All(box => Math.Abs(box.Bounds.Height - 48) < 1), "Zwei einfache Zahlenfelder, 48 hoch");
        numberFields[0].Text = "8";
        check(main.Services.Store.Load().Settings.DelayThresholdMinutes == 8, "Verspaetung 8 gespeichert");
        numberFields[0].Text = "0";
        check(settings.DelayThresholdInvalid && main.Services.Store.Load().Settings.DelayThresholdMinutes == 8, "Verspaetung 0 zeigt Hinweis, 8 bleibt");
        settings.TidyTextFields();
        check(numberFields[0].Text == "8" && !settings.DelayThresholdInvalid, "Nach Verlassen wieder 8");
        numberFields[1].Text = "6";
        check(main.Services.Store.Load().Settings.MinTransferMinutes == 6, "Umstiegszeit 6 gespeichert");
        await capture(window, Path.Combine(directory, "settings-numbers.png"));
        numberFields[0].Text = "5";
        numberFields[1].Text = "4";
        var timeField = window.GetVisualDescendants().OfType<TextBox>().First(box => box.Classes.Contains("time"));
        check(timeField.Text == "20:00" && Math.Abs(timeField.Bounds.Height - 48) < 1, "Uhrzeit als Textfeld 20:00, 48 hoch");
        timeField.Text = "1930";
        check(timeField.Text == "19:30", "Doppelpunkt kommt beim Tippen von selbst");
        check(settings.ShowSaved, "Aenderung zeigt kurz Gespeichert");
        check(settings.AlarmSound && settings.AlarmFullScreen, "Ton und Vollbild sind am Anfang an");
        settings.AlarmSound = false;
        settings.AlarmFullScreen = false;
        var storedAlarm = main.Services.Store.Load().Settings;
        check(!storedAlarm.AlarmSound && !storedAlarm.AlarmFullScreen, "Ton und Vollbild lassen sich abschalten");
        settings.AlarmSound = true;
        settings.AlarmFullScreen = true;
        var soundLabel = window.GetVisualDescendants().OfType<TextBlock>().First(text => text.Text == "Vollbild auf dem Sperrbildschirm");
        soundLabel.BringIntoView();
        await SettleAsync();
        await capture(window, Path.Combine(directory, "settings-alarm.png"));
        check(main.Services.Store.Load().Settings.EveningCheckTime == new TimeOnly(19, 30) && !settings.EveningCheckTimeInvalid, "Uhrzeit 1930 gespeichert");
        settings.TidyTextFields();
        check(timeField.Text == "19:30", "Uhrzeit aufgeraeumt zu 19:30");
        timeField.Text = "25:00";
        check(settings.EveningCheckTimeInvalid && main.Services.Store.Load().Settings.EveningCheckTime == new TimeOnly(19, 30), "Falsche Uhrzeit zeigt Hinweis, alte bleibt");
        await capture(window, Path.Combine(directory, "settings-time-invalid.png"));
        settings.TidyTextFields();
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

        settings.BreakThroughDoNotDisturb = true;
        check(main.Services.Store.Load().Settings.BreakThroughDoNotDisturb, "Bitte-nicht-stoeren-Schalter gespeichert");
        check(window.GetVisualDescendants().OfType<TextBlock>().Any(text => text.Text == "Auch bei „Bitte nicht stören“"), "Bitte-nicht-stoeren-Schalter sichtbar");
        settings.BreakThroughDoNotDisturb = false;
        check(!main.Services.Store.Load().Settings.BreakThroughDoNotDisturb, "Bitte-nicht-stoeren-Schalter wieder aus");

        await RunManualEntryAsync(main, window, directory, check, capture);
        await RunTripsAsync(main, window, directory, check, capture);
        await RunUpdateCheckAsync(main, window, directory, check, capture);
        await RunSecondViewAsync(main, window, directory, check, capture);
        await RunLanguageAsync(main, window, directory, check, capture);
    }

    /// <remarks>Der Sprachwechsel baut Hauptansicht und ViewModel neu, deshalb werden alle Bezuege danach neu geholt.</remarks>
    private static async Task RunLanguageAsync(MainViewModel main, Window window, string directory, Action<bool, string> check, Func<TopLevel, string, Task> capture)
    {
        main.Services.UpdateInstaller = null;
        main.Services.UpdateClientFactory = () => new HttpClient(new DemoUpdateHandler("v9.9.9"));
        await main.Updates.CheckAsync();
        main.Navigate("settings");
        await SettleAsync();
        check(main.Updates.HasUpdate && HasText(window, "Version 9.9.9 ist da."), "Update-Hinweis vor dem Sprachwechsel");
        var languageRadios = LanguageRadios(window);
        check(languageRadios.Count == 3, "Drei Sprach-Knoepfe");
        languageRadios[2].IsChecked = true;
        await SettleAsync();
        var english = App.Main!;
        check(!ReferenceEquals(english, main) && !Language.IsGerman && main.Services.Store.Load().Settings.Language == Core.Models.AppLanguage.English, "Englisch gewaehlt und gespeichert");
        check(english.NavItems.Select(item => item.Title).SequenceEqual(new[] { "Week", "Trips", "Shifts", "Settings" }), "Tabs auf Englisch");
        check(english.CurrentPage is SettingsViewModel && HasText(window, "Language") && HasText(window, "Check for updates"), "Neu aufgebaute Einstellungen auf Englisch");
        check(HasText(window, "Offer pre-releases") && HasText(window, "Test builds from dev, may have bugs."), "Schalter Vorabversionen auf Englisch");
        check(english.Updates.HasUpdate && HasText(window, "Version 9.9.9 is here.") && HasText(window, "Open download page"), "Update-Hinweis bleibt nach dem Sprachwechsel, auf Englisch");
        await capture(window, Path.Combine(directory, "settings-en.png"));

        english.Navigate("week");
        await SettleAsync();
        var week = (WeekViewModel)english.CurrentPage!;
        check(week.WeekTitle == "This week" && week.Days[0].DayName == "Monday" && HasText(window, "Set several days"), "Woche auf Englisch");
        var germanBadges = new[] { "Teilausfall", "Fällt aus", "Planmäßig", "Laut Fahrplan", "Keine Daten", "Nicht gefunden" };
        var storedStates = english.Services.Data.CheckStates.Where(state => !string.IsNullOrEmpty(state.LastBadgeText)).ToList();
        check(storedStates.Count > 0 && week.Days.All(day => day.OutboundStatus is null || !germanBadges.Contains(day.OutboundStatus.Text)) && week.Days.All(day => day.ReturnStatus is null || !germanBadges.Contains(day.ReturnStatus.Text)), "Deutsch gespeicherte Statustexte erscheinen auf Englisch");
        var today = week.Days.First(day => day.IsToday);
        check(today.OutboundCaption is not null && today.OutboundCaption.StartsWith("Out "), "Fahrt-Hinweis auf Englisch: " + today.OutboundCaption);
        week.StartSelectingCommand.Execute(null);
        week.ApplySelectionCommand.Execute(null);
        check(english.Toast == "Mark a few days first.", "Hinweis auf Englisch");
        week.CancelSelectingCommand.Execute(null);
        english.Toast = null;
        await capture(window, Path.Combine(directory, "week-en.png"));

        english.Navigate("trips");
        await SettleAsync();
        var trips = (TripsViewModel)english.CurrentPage!;
        var tripCard = trips.Trips.FirstOrDefault();
        check(HasText(window, "One-off trips") && tripCard is not null && tripCard.Kicker == "TOMORROW"
              && tripCard.DateText.StartsWith(tripCard.Trip.TravelDate.DayOfWeek + ", ") && HasText(window, "Open in DB Navigator"), "Fahrten auf Englisch");
        check(HasText(window, "Check now (all trips)"), "Knopf Jetzt pruefen auf Englisch");
        await capture(window, Path.Combine(directory, "trips-en.png"));

        english.Navigate("settings");
        await SettleAsync();
        LanguageRadios(window)[1].IsChecked = true;
        await SettleAsync();
        var german = App.Main!;
        check(!ReferenceEquals(german, english) && Language.IsGerman && german.Services.Store.Load().Settings.Language == Core.Models.AppLanguage.German, "Zurueck auf Deutsch");
        check(german.NavItems.Select(item => item.Title).SequenceEqual(new[] { "Woche", "Fahrten", "Schichten", "Einstellungen" }) && HasText(window, "Sprache") && HasText(window, "Nach Updates suchen"), "Oberflaeche wieder auf Deutsch");
        check(german.Updates.HasUpdate && HasText(window, "Version 9.9.9 ist da.") && HasText(window, "Download-Seite öffnen"), "Update-Hinweis wieder auf Deutsch");
        await capture(window, Path.Combine(directory, "settings-de.png"));
        german.DismissUpdateCommand.Execute(null);
        main.Services.UpdateClientFactory = null;
        german.Navigate("week");
        check(((WeekViewModel)german.CurrentPage!).Days[0].DayName == "Montag", "Wochentage wieder auf Deutsch");
    }

    private static async Task SettleAsync()
    {
        await Task.Delay(300);
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
    }

    private static List<RadioButton> LanguageRadios(Window window)
    {
        return window.GetVisualDescendants().OfType<RadioButton>().Where(radio => radio.GroupName == "language").ToList();
    }

    private static bool HasText(Window window, string text)
    {
        return window.GetVisualDescendants().OfType<TextBlock>().Any(block => block.Text == text && block.IsEffectivelyVisible);
    }

    private static async Task RunSecondViewAsync(MainViewModel main, Window window, string directory, Action<bool, string> check, Func<TopLevel, string, Task> capture)
    {
        main.Navigate("week");
        var second = new Window { Width = 400, Height = 820, Content = new Views.MainView { DataContext = main } };
        second.Show();
        await capture(second, Path.Combine(directory, "second-view.png"));
        check(second.GetVisualDescendants().OfType<Views.WeekView>().Any() && window.GetVisualDescendants().OfType<Views.WeekView>().Any(), "Zweite Hauptansicht zeigt die Woche, die erste behaelt sie");
        main.Navigate("shifts");
        await capture(second, Path.Combine(directory, "second-view-shifts.png"));
        check(second.GetVisualDescendants().OfType<Views.ShiftsView>().Any(), "Zweite Hauptansicht wechselt die Seite");
        second.Close();
        main.Navigate("week");
        check(window.GetVisualDescendants().OfType<Views.WeekView>().Any(), "Erste Hauptansicht nach dem Schliessen weiter nutzbar");
    }

    private static async Task RunUpdateCheckAsync(MainViewModel main, Window window, string directory, Action<bool, string> check, Func<TopLevel, string, Task> capture)
    {
        var plannedOnly = new StatusBadge(Core.Checking.LegStatus.PlannedOnly, "Laut Fahrplan", "");
        var onTime = new StatusBadge(Core.Checking.LegStatus.OnTime, "Planmäßig", "");
        var notChecked = new StatusBadge(null, "—", "");
        check(ReferenceEquals(plannedOnly.Background, onTime.Background) && !ReferenceEquals(plannedOnly.Background, notChecked.Background), "Laut Fahrplan gruen, nicht geprueft grau");

        main.Navigate("settings");
        var settings = (SettingsViewModel)main.CurrentPage!;
        var updates = main.Updates;
        var platform = (Services.DefaultPlatform)main.Services.Platform;
        var opened = new List<string>();
        platform.UrlOpener = url => opened.Add(url);
        main.DismissUpdateCommand.Execute(null);

        var handler = new DemoUpdateHandler("v" + Core.AppInfo.Version);
        main.Services.UpdateClientFactory = () => new HttpClient(handler);
        await settings.CheckUpdatesNowCommand.ExecuteAsync(null);
        check(settings.UpdateStatusText is not null && settings.UpdateStatusText.StartsWith("Du hast die neueste Version") && !updates.HasUpdate, "Updates: aktuell");
        check(handler.RequestedUrls.Contains(DemoUpdateHandler.ReleasesUrl), "Updates: fragt die Release-Liste auf GitHub ab");

        handler = new DemoUpdateHandler(null);
        await settings.CheckUpdatesNowCommand.ExecuteAsync(null);
        check(settings.UpdateStatusText is not null && settings.UpdateStatusText.StartsWith("Konnte bei GitHub nicht nachsehen"), "Updates: 404 wird als nicht pruefbar gemeldet");

        handler = new DemoUpdateHandler("v" + Core.AppInfo.Version, "v9.9.9-dev.1");
        await settings.CheckUpdatesNowCommand.ExecuteAsync(null);
        check(!settings.IncludePrereleases && !updates.HasUpdate && settings.UpdateStatusText!.StartsWith("Du hast die neueste Version"), "Updates: Vorabversion ohne Schalter nicht angeboten");
        check(HasText(window, "Vorabversionen anbieten"), "Schalter Vorabversionen sichtbar");
        settings.IncludePrereleases = true;
        check(main.Services.Store.Load().Settings.IncludePrereleases, "Schalter Vorabversionen gespeichert");
        await settings.CheckUpdatesNowCommand.ExecuteAsync(null);
        check(updates.HasUpdate && updates.Release!.IsPrerelease && updates.BannerText == "Version 9.9.9-dev.1 ist da.", "Updates: Vorabversion mit Schalter angeboten");
        settings.IncludePrereleases = false;

        handler = new DemoUpdateHandler("v9.9.9");
        await settings.CheckUpdatesNowCommand.ExecuteAsync(null);
        await SettleAsync();
        check(updates.HasUpdate && settings.UpdateStatusText is not null && settings.UpdateStatusText.Contains("9.9.9"), "Updates: neue Version zeigt Banner");
        check(updates.BannerText == "Version 9.9.9 ist da." && HasText(window, "Version 9.9.9 ist da."), "Update-Hinweis sichtbar");
        check(updates.NotesText == "feat: Updates direkt in der App laden und installieren\nfix: Alarm klingelt auch nach Neustart", "Update-Hinweis zeigt die gekuerzten Notizen");
        check(HasText(window, "Download-Seite öffnen") && settings.UpdateStatusText!.Contains("„Download-Seite öffnen“"), "Ohne Installer: Knopf Download-Seite öffnen");
        check(HasText(window, "Nach Updates suchen"), "Knopf Nach Updates suchen sichtbar");
        await capture(window, Path.Combine(directory, "settings-update-check.png"));
        await main.UpdateNowCommand.ExecuteAsync(null);
        check(opened.Count == 1 && opened[0] == "https://github.com/BastianBaumann/KackZug/releases/tag/v9.9.9", "Ohne Installer oeffnet der Knopf die Release-Seite");
        main.DismissUpdateCommand.Execute(null);
        check(!updates.HasUpdate, "Update-Hinweis ausblendbar");

        var installer = new DemoUpdateInstaller { PermissionRequestsLeft = 1 };
        main.Services.UpdateInstaller = installer;
        var updateDirectory = Path.Combine(main.Services.Platform.DataDirectory, "updates");
        handler = new DemoUpdateHandler("v9.9.9") { CorruptAsset = true };
        await updates.CheckAsync();
        main.Navigate("week");
        await SettleAsync();
        check(updates.HasUpdate && HasText(window, "Jetzt aktualisieren"), "Mit Installer: Knopf Jetzt aktualisieren");
        await capture(window, Path.Combine(directory, "update-banner.png"));
        await main.UpdateNowCommand.ExecuteAsync(null);
        check(main.ToastIsError && main.Toast is not null && main.Toast.Contains("fehlerhaft") && installer.InstalledFiles.Count == 0
              && !Directory.EnumerateFiles(updateDirectory).Any(), "Falsche Pruefsumme: Datei verworfen, nichts installiert");
        main.Toast = null;

        var pausing = new DemoPausingStream();
        handler = new DemoUpdateHandler("v9.9.9") { PausingStream = pausing };
        await updates.CheckAsync();
        var percents = new List<int>();
        updates.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(updates.ProgressPercent) && updates.ProgressPercent is int percent)
            {
                percents.Add(percent);
            }
        };
        var permissionScreens = 0;
        platform.InstallPermissionOpener = () => permissionScreens++;
        var run = main.UpdateNowCommand.ExecuteAsync(null);
        await pausing.Paused;
        await SettleAsync();
        check(updates.IsDownloading && updates.BannerText == "Lädt … 50 %" && HasText(window, "Lädt … 50 %"), "Download: Fortschritt im Hinweis");
        await capture(window, Path.Combine(directory, "update-downloading.png"));
        pausing.Resume();
        await WaitForAsync(() => main.HasDialog);
        check(percents.Count >= 3 && percents.Contains(50) && percents[percents.Count - 1] == 100, "Download: Fortschritt erreicht 100 %");
        var downloaded = updates.DownloadedFilePath;
        check(updates.IsDownloaded && Path.GetFileName(downloaded) == "KackZug-9.9.9.apk"
              && Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(downloaded!))).ToLowerInvariant() == DemoUpdateHandler.AssetSha256, "Download: Pruefsumme stimmt, Datei liegt bereit");
        check(main.HasDialog && main.Dialog!.Message.Contains("Apps zu installieren"), "Erlaubnis: Erklaerung vor den Einstellungen");
        await capture(window, Path.Combine(directory, "update-permission.png"));
        main.Dialog!.ConfirmCommand.Execute(null);
        await run;
        check(permissionScreens == 1 && updates.IsWaitingForPermission && installer.InstalledFiles.Count == 0, "Erlaubnis: Einstellungen geoeffnet, Installation wartet");

        App.ContinueUpdateAfterResume();
        await WaitForAsync(() => installer.InstalledFiles.Count > 0);
        check(installer.InstalledFiles.Count == 1 && installer.InstalledFiles[0] == downloaded && installer.LastFileSha256 == DemoUpdateHandler.AssetSha256,
              "Nach der Rueckkehr: Installer bekommt die geladene Datei");
        check(!updates.IsWaitingForPermission && updates.IsInstalling && updates.BannerText == "Version 9.9.9 wird installiert …", "Hinweis zeigt die laufende Installation");

        App.ReportUpdateInstallFinished(false, true, "aborted");
        await SettleAsync();
        check(!updates.IsInstalling && updates.HasUpdate && main.Toast is not null && main.Toast.StartsWith("Update abgebrochen"), "Abgebrochene Installation meldet sich, Hinweis bleibt");
        var assetRequests = handler.AssetRequests;
        await main.UpdateNowCommand.ExecuteAsync(null);
        check(installer.InstalledFiles.Count == 2 && handler.AssetRequests == assetRequests, "Neuer Versuch installiert ohne neuen Download");
        App.ReportUpdateInstallFinished(true, false, null);
        await SettleAsync();
        check(!updates.HasUpdate && !updates.IsInstalling, "Erfolgreiche Installation blendet den Hinweis aus");

        main.Toast = null;
        main.Services.UpdateInstaller = null;
        platform.InstallPermissionOpener = null;
        platform.UrlOpener = null;
    }

    private static Button CheckAllButton(Window window, TripsViewModel trips)
    {
        return window.GetVisualDescendants().OfType<Button>().First(button => button.Command == trips.CheckNowCommand);
    }

    private static async Task WaitForAsync(Func<bool> condition)
    {
        for (var i = 0; i < 100 && !condition(); i++)
        {
            await Task.Delay(50);
        }
    }

    private static async Task RunTripsAsync(MainViewModel main, Window window, string directory, Action<bool, string> check, Func<TopLevel, string, Task> capture)
    {
        var services = main.Services;
        var today = DateOnly.FromDateTime(BerlinTime.ToWallClock(services.Clock.Now, BerlinTime.Zone));
        var tomorrow = today.AddDays(1);
        var previousVariant = Avalonia.Application.Current!.RequestedThemeVariant;
        Avalonia.Application.Current.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Light;
        main.Toast = null;
        main.Navigate("trips");
        await SettleAsync();
        var trips = (TripsViewModel)main.CurrentPage!;
        check(main.NavItems.Select(item => item.Key).SequenceEqual(new[] { "week", "trips", "shifts", "settings" }), "Tab Fahrten zwischen Woche und Schichten");
        check(trips.IsEmpty && HasText(window, "Noch keine Einzelfahrt"), "Fahrten ohne Einzelfahrt zeigt den Hinweis");
        check(!CheckAllButton(window, trips).IsEffectivelyVisible, "Ohne Einzelfahrt kein Knopf Jetzt pruefen");
        await capture(window, Path.Combine(directory, "trips-empty.png"));

        trips.EnterManuallyCommand.Execute(null);
        check(main.Import is not null && main.Import.IsManualStep && main.Import.PrefersOneOffTrip, "Eingabe von Hand aus Fahrten waehlt die Einzelfahrt vor");
        main.Import!.CloseCommand.Execute(null);

        await main.StartImportAsync("https://int.bahn.de/en/buchung/start?vbid=demo");
        var import = main.Import!;
        var oneOff = import.Targets.FirstOrDefault(target => target.IsOneOffTrip);
        check(oneOff is not null && oneOff.Title == "Einzelfahrt" && import.Targets.Last() == oneOff, "Import bietet Einzelfahrt an");
        import.SelectTargetCommand.Execute(oneOff);
        await SettleAsync();
        check(import.IsOneOffTarget && import.TripDateText == DateText.Format(today) && import.TripNamePlaceholder == "Beispielhausen → Musterstadt Hbf",
              "Einzelfahrt: Datum vom geteilten Tag, Name aus Start und Ziel");
        var directionRadios = window.GetVisualDescendants().OfType<RadioButton>().Where(radio => radio.GroupName == "direction").ToList();
        check(directionRadios.Count == 2 && directionRadios.All(radio => !radio.IsEffectivelyVisible), "Einzelfahrt ohne Hin und Rueck");
        var dateBox = window.GetVisualDescendants().OfType<TextBox>().First(box => box.Classes.Contains("date"));
        check(dateBox.IsEffectivelyVisible && Math.Abs(dateBox.Bounds.Height - 48) < 1, "Datumsfeld sichtbar, 48 hoch");
        var yesterday = today.AddDays(-1);
        dateBox.Text = today.AddDays(1).ToString("ddMM") + "20";
        check(!import.HasTripDateError, "Halb getipptes Jahr 20.. zeigt noch keinen Hinweis: " + dateBox.Text);
        dateBox.Text = yesterday.ToString("ddMMyyyy");
        check(dateBox.Text == DateText.Format(yesterday), "Punkte im Datum kommen beim Tippen von selbst: " + dateBox.Text);
        check(import.HasTripDateError && import.TripDateError!.Contains("vorbei") && !import.SaveCommand.CanExecute(null), "Datum in der Vergangenheit wird abgelehnt");
        await capture(window, Path.Combine(directory, "import-trip-past.png"));
        dateBox.Text = "31.02.2027";
        check(import.HasTripDateError && import.TripDateError!.Contains("TT.MM.JJJJ") && !import.SaveCommand.CanExecute(null), "Falsches Datum zeigt Hinweis");
        dateBox.Text = "";
        dateBox.Text = tomorrow.ToString("ddMMyyyy");
        var tomorrowName = Language.Culture.DateTimeFormat.GetDayName(tomorrow.DayOfWeek);
        check(!import.HasTripDateError && import.TripWeekday == tomorrowName && import.SaveCommand.CanExecute(null), "Morgen ist speicherbar, Wochentag daneben: " + import.TripWeekday);
        import.TripName = "Konzert im Stadtpark mit der ganzen Familie und Freunden";
        await SettleAsync();
        await capture(window, Path.Combine(directory, "import-trip.png"));
        import.SaveCommand.Execute(null);
        check(!main.HasImport && main.CurrentPage is TripsViewModel && main.Toast is not null && main.Toast.StartsWith("Einzelfahrt am "), "Einzelfahrt gespeichert, Tab Fahrten offen");
        var stored = services.Store.Load().Trips;
        check(stored.Count == 1 && stored[0].TravelDate == tomorrow && stored[0].Name.StartsWith("Konzert") && stored[0].Connection.Legs[0].Name == "RB21", "Einzelfahrt mit Datum in der Datei");
        var tripId = stored[0].Id;
        var planned = CheckPlanner.Plan(services.Data, services.Clock.Now, BerlinTime.Zone);
        check(planned.Any(item => item.ShiftId == tripId && item.TravelDate == tomorrow), "Pruefungen fuer die Einzelfahrt geplant");

        trips = (TripsViewModel)main.CurrentPage!;
        var card = trips.Trips.Single();
        check(card.Kicker == "MORGEN" && card.DateText == tomorrowName + ", " + DateText.Format(tomorrow) && card.TimesText == "22:10 → 22:41"
              && card.RouteText == "Beispielhausen → Musterstadt Hbf" && card.Live.Badge is not null && card.Live.Badge.Text == "—", "Karte zeigt Name, Datum, Zeiten und Kennzeichen");
        main.Toast = null;
        await SettleAsync();
        var nameBlock = window.GetVisualDescendants().OfType<TextBlock>().First(text => text.Text == card.Name);
        var deleteButton = window.GetVisualDescendants().OfType<Button>().First(button => button.CommandParameter == card && button.Classes.Contains("icon"));
        var nameRight = Avalonia.VisualExtensions.TranslatePoint(nameBlock, new Avalonia.Point(nameBlock.Bounds.Width, 0), window)!.Value.X;
        var deleteLeft = Avalonia.VisualExtensions.TranslatePoint(deleteButton, new Avalonia.Point(0, 0), window)!.Value.X;
        check(nameRight <= deleteLeft + 0.5 && nameBlock.TextLayout.TextLines.Any(line => line.HasCollapsed), "Langer Name wird gekuerzt, Loeschen bleibt sichtbar");
        check(deleteLeft + deleteButton.Bounds.Width <= window.ClientSize.Width, "Karte passt in 400 Pixel");
        await capture(window, Path.Combine(directory, "trips-light.png"));
        Avalonia.Application.Current!.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Dark;
        await capture(window, Path.Combine(directory, "trips-dark.png"));
        Avalonia.Application.Current.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Light;

        var checkAll = CheckAllButton(window, trips);
        check(checkAll.IsEffectivelyVisible && HasText(window, "Jetzt prüfen (alle Fahrten)"), "Mit Einzelfahrt gibt es den Knopf Jetzt pruefen");
        var lockedWhileChecking = false;
        System.ComponentModel.PropertyChangedEventHandler watchChecking = (_, args) =>
        {
            if (args.PropertyName == nameof(TripsViewModel.IsChecking) && trips.IsChecking)
            {
                lockedWhileChecking = !trips.CheckNowCommand.CanExecute(null);
            }
        };
        trips.PropertyChanged += watchChecking;
        await trips.CheckNowCommand.ExecuteAsync(null);
        trips.PropertyChanged -= watchChecking;
        check(lockedWhileChecking, "Waehrend der Pruefung ist der Knopf gesperrt");
        await SettleAsync();
        card = trips.Trips.Single();
        check(!trips.IsChecking && card.Live.Badge is not null && card.Live.Badge.Text == "Planmäßig" && card.CheckedText.StartsWith("geprüft "),
              "Jetzt pruefen aktualisiert Kennzeichen und Pruefzeit der Karte: " + card.CheckedText);
        check(services.Store.Load().CheckStates.Any(state => state.ShiftId == tripId && state.LastCheckedAt != default), "Pruefung gespeichert");
        await capture(window, Path.Combine(directory, "trips-check.png"));

        trips.OpenDetailCommand.Execute(card);
        check(trips.HasDetail && trips.Detail!.BackText == "‹ Zurück zu den Fahrten" && trips.Detail.Trips.Count == 1, "Details der Einzelfahrt geoeffnet");
        await trips.Detail.RefreshCommand.ExecuteAsync(null);
        var live = trips.Detail.Trips[0];
        check(live.Title == "Einzelfahrt" && live.Badge is not null && live.Badge.Text == "Planmäßig" && live.CheckedText == "Gerade geprüft", "Details zeigen den Live-Stand");
        await SettleAsync();
        await capture(window, Path.Combine(directory, "trip-oneoff-detail.png"));
        check(main.HandleBack() && !trips.HasDetail, "Zurueck schliesst die Details der Einzelfahrt");
        card = trips.Trips.Single();
        check(card.Live.Badge!.Text == "Planmäßig" && card.CheckedText.StartsWith("geprüft "), "Karte zeigt den Stand nach der Pruefung: " + card.CheckedText);

        var opened = new List<string>();
        var platform = (Services.DefaultPlatform)services.Platform;
        platform.UrlOpener = url => opened.Add(url);
        var shareLinks = new DemoShareLinks { Link = "https://www.bahn.de/buchung/start?vbid=einzel" };
        services.ShareLinks = shareLinks;
        await card.Live.OpenInNavigatorCommand.ExecuteAsync(null);
        check(opened.Count == 1 && opened[0] == shareLinks.Link && shareLinks.RequestedDays.SequenceEqual(new[] { tomorrow }), "Navigator bekommt den Teilen-Link fuer den Tag der Einzelfahrt");
        platform.UrlOpener = null;

        var deleteTask = trips.DeleteCommand.ExecuteAsync(card);
        await Task.Delay(300);
        check(main.HasDialog && main.Dialog!.Message.Contains("Konzert"), "Rueckfrage beim Loeschen der Einzelfahrt");
        await capture(window, Path.Combine(directory, "trips-delete-confirm.png"));
        main.Dialog!.CancelCommand.Execute(null);
        await deleteTask;
        check(trips.Trips.Count == 1, "Abbrechen behaelt die Einzelfahrt");
        deleteTask = trips.DeleteCommand.ExecuteAsync(trips.Trips[0]);
        await Task.Delay(300);
        main.Dialog!.ConfirmCommand.Execute(null);
        await deleteTask;
        check(trips.IsEmpty && services.Store.Load().Trips.Count == 0 && !services.Data.CheckStates.Any(state => state.ShiftId == tripId), "Einzelfahrt geloescht, mit ihrem Stand");
        main.Toast = null;

        var resolved = await new DemoResolver().ResolveAsync("demo", CancellationToken.None);
        services.Data.Trips.Add(new OneOffTrip { Name = "Gestern", TravelDate = yesterday, Connection = resolved.Connection });
        resolved = await new DemoResolver().ResolveAsync("demo", CancellationToken.None);
        services.Data.Trips.Add(new OneOffTrip { Name = "Messe", TravelDate = tomorrow, Connection = resolved.Connection });
        services.Store.Save(services.Data);
        main.Navigate("week");
        main.Navigate("trips");
        await SettleAsync();
        check(trips.Trips.Count == 1 && trips.Trips[0].Name == "Messe" && services.Store.Load().Trips.All(trip => trip.Name != "Gestern"), "Vorbei gefahrene Einzelfahrt verschwindet");
        Avalonia.Application.Current.RequestedThemeVariant = previousVariant;
    }

    private static async Task RunManualEntryAsync(MainViewModel main, Window window, string directory, Action<bool, string> check, Func<TopLevel, string, Task> capture)
    {
        main.Services.StationSearch = new DemoStationSearch();
        main.Navigate("shifts");
        var shifts = (ShiftsViewModel)main.CurrentPage!;
        shifts.EnterManuallyCommand.Execute(null);
        var import = main.Import!;
        check(import.IsManualStep && !import.IsReady && import.Manual!.Legs.Count == 1, "Eingabe von Hand geoeffnet");
        var manual = import.Manual;
        manual.ContinueCommand.Execute(null);
        check(manual.ErrorMessage is not null && manual.ErrorMessage.Contains("Start"), "Leere Eingabe nennt den fehlenden Start");
        await capture(window, Path.Combine(directory, "manual-empty.png"));
        check(window.GetVisualDescendants().OfType<TextBlock>().Any(text => text.Text == "Verbindung von Hand" && text.IsEffectivelyVisible), "Formular sichtbar");

        var first = manual.Legs[0];
        first.From.Query = "musterstadt";
        await first.From.SearchCommand.ExecuteAsync(null);
        check(first.From.Results.Count == 2, "Suche findet zwei Bahnhoefe");
        first.From.Results[0].ChooseCommand.Execute(null);
        check(first.From.HasSelection && first.From.Selected!.EvaNumber == "8000001", "Start gewaehlt");
        first.To.Query = "Beispielhausen";
        await first.To.SearchCommand.ExecuteAsync(null);
        first.To.Results.First(result => result.Name == "Beispielhausen").ChooseCommand.Execute(null);
        first.DepartureText = "2140";
        first.ArrivalText = "22:05";
        first.LineText = "RE 7";

        manual.AddLegCommand.Execute(null);
        var second = manual.Legs[1];
        check(second.From.Selected is not null && second.From.Selected.Name == "Beispielhausen" && second.Title == "Teilstrecke 2", "Zweite Teilstrecke startet am letzten Ziel");
        second.From.ChangeCommand.Execute(null);
        second.From.Query = "Markt";
        await second.From.SearchCommand.ExecuteAsync(null);
        second.From.Results[0].ChooseCommand.Execute(null);
        second.To.Query = "Werkstor";
        await second.To.SearchCommand.ExecuteAsync(null);
        second.To.Results[0].ChooseCommand.Execute(null);
        second.DepartureText = "22.15";
        second.ArrivalText = "2230";
        second.LineText = "Bus 11";
        await capture(window, Path.Combine(directory, "manual-filled.png"));

        manual.ContinueCommand.Execute(null);
        check(import.IsReady && !import.IsManualStep && import.Legs.Count == 3 && import.Legs[1].IsWalk, "Weiter zeigt die Verbindung mit Fussweg");
        check(import.Hint is not null && import.CameFromManual, "Hinweis fuer die Eingabe von Hand");
        await capture(window, Path.Combine(directory, "manual-assign.png"));

        import.EditManualCommand.Execute(null);
        check(import.IsManualStep && manual.Legs.Count == 2 && manual.Legs[1].LineText == "Bus 11", "Eingabe aendern behaelt die Werte");
        manual.ContinueCommand.Execute(null);

        var newTarget = import.Targets.First(target => target.IsNew);
        import.SelectTargetCommand.Execute(newTarget);
        import.NewShiftName = "Nachtdienst";
        import.SaveCommand.Execute(null);
        var night = main.Services.Store.Load().Shifts.FirstOrDefault(shift => shift.Name == "Nachtdienst");
        check(night is not null && night.Outbound is not null && night.Outbound.Legs.Count == 3 && night.Outbound.Legs[0].Name == "RE 7"
              && night.Outbound.Legs[0].PlannedDeparture == new TimeOnly(21, 40), "Verbindung von Hand gespeichert");
        await capture(window, Path.Combine(directory, "shifts-after-manual.png"));

        await main.StartImportAsync("nur kaputt");
        main.Import!.StartManualCommand.Execute(null);
        check(main.Import.IsManualStep && !main.Import.HasError, "Fehler beim Teilen fuehrt zur Eingabe von Hand");
        main.Import.CloseCommand.Execute(null);
    }
}
#endif
