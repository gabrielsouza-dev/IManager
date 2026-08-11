using System.ComponentModel.DataAnnotations.Schema;
using TimeZoneConverter;

namespace IManager.Web.Data.Seeder.SeedDatas;

public class TimeCheckSeedData
{
    public Guid TimeEntryId { get; set; } = Guid.NewGuid();
    public string TimeZoneId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [NotMapped]
    public DateTime TimestampLocal
    {
        get
        {
            var id = TimeZoneId;

            // tenta direto (IANA no Linux/Android/iOS ou Windows válido)
            if (TimeZoneInfo.TryFindSystemTimeZoneById(id, out var tz))
                return TimeZoneInfo.ConvertTimeFromUtc(Timestamp, tz);

            // tenta converter Windows → IANA
            if (TZConvert.TryWindowsToIana(id, out var iana) &&
                TimeZoneInfo.TryFindSystemTimeZoneById(iana, out tz))
            {
                return TimeZoneInfo.ConvertTimeFromUtc(Timestamp, tz);
            }

            return Timestamp; // fallback seguro
        }
    }
}