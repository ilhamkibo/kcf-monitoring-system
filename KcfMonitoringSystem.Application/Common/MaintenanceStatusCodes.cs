namespace KcfMonitoringSystem.Application.Common;

public static class MaintenanceStatusCodes
{
    public const int UnderMaintenance = 5;
    public const int PreventiveMaintenance = 6;

    public static bool IsValid(int code) =>
        code is UnderMaintenance or PreventiveMaintenance;
}
