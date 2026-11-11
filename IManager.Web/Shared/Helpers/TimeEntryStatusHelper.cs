using IManager.Web.Domain.Enums;

namespace IManager.Web.Shared.Helpers;

public static class TimeEntryStatusHelper
{
    public static string ToDisplayName(this TimeEntryStatus status) => status switch
    {
        TimeEntryStatus.Accepted => "Aprovado",
        TimeEntryStatus.Rejected => "Reprovado",
        TimeEntryStatus.Pending => "Pendente",
        _ => status.ToString()
    };
}
