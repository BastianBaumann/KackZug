using AppUpdater;
using KackZug.Core.Checking;
using KackZug.Core.Planning;

namespace KackZug.Services;

/// <summary>Was die App von der jeweiligen Plattform braucht: Datenordner, Alarme, Benachrichtigungen</summary>
public interface IPlatform
{
    string DataDirectory { get; }

    bool IsMobile { get; }

    void ScheduleChecks(IReadOnlyList<PlannedCheck> checks);

    void ShowAlert(TripAlert alert);

    void ScheduleTestAlarm(TimeSpan delay);

    void OpenUrl(string url);

    /// <remarks>Ohne Link startet nur der DB Navigator selbst. Gibt false zurueck, wenn der Navigator fehlt und stattdessen der Browser oeffnet.</remarks>
    bool OpenInNavigator(string? connectionUrl);

    /// <remarks>Nur dort noetig, wo die App die Hauptansicht nicht selbst austauschen kann (Android-Aktivitaet).</remarks>
    void RecreateMainView();

    IReadOnlyList<PermissionInfo> GetPermissions();

    void OpenPermissionSettings(string key);

    /// <remarks>Null, wo die App sich nicht selbst installieren kann; dann bleibt nur die Download-Seite.</remarks>
    IUpdateInstaller? CreateUpdateInstaller();

    /// <summary>Oeffnet die Systemseite, auf der der Nutzer der App das Installieren von Apps erlaubt</summary>
    void OpenInstallPermissionSettings();
}
