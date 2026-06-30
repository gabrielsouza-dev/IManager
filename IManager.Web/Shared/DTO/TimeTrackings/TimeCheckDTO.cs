using IManager.Web.Domain.Enums;
namespace IManager.Web.Shared.DTO.TimeTrackings;

public class TimeCheckDTO
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string TimeZoneId { get; set; } = string.Empty;

}