using IManager.Web.Domain.Enums;
namespace IManager.Web.Shared.Requests;

public record TimeCheckRequest(DateTime Timestamp, string TimeZoneId);
