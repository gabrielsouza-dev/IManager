using IManager.Web.Domain.Entities.TimeTrackings;

namespace IManager.Web.Shared.Helpers;

public static class TimeCheckHelper
{
    public static TimeSpan GetHoursWorked(this IEnumerable<TimeCheck> Checks)
    {
        var totalHours = TimeSpan.Zero;
        var IsConsistent = Checks.IsConsistent();
        if (Checks.Count() <= 1)
            return totalHours;

        var timestamps = Checks.Select(c => c.Timestamp.TimeOfDay).OrderBy(ts => ts);
        var checks = new Stack<TimeSpan>(timestamps);

        if (!IsConsistent)
            checks.Pop();

        for (int i = checks.Count; i > 0; i -= 2)
        {
            totalHours += checks.Pop() - checks.Pop();
        }

        return totalHours;
    }

    public static bool IsConsistent(this IEnumerable<TimeCheck> Checks)
    {
        return Checks.Count() % 2 == 0;
    }
}