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

    void OpenUrl(string url);

    IReadOnlyList<PermissionInfo> GetPermissions();

    void OpenPermissionSettings(string key);
}
