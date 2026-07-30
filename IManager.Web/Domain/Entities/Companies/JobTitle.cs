using IManager.Web.Domain.Entities.Users;

namespace IManager.Web.Domain.Entities.Companies;

public class JobTitle : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    public bool IsHazard { get; set; } = false;
    public bool IsUnhealthy { get; set; } = false;
    public bool IsCommissioned { get; set; } = false;
    public TimeSpan DailyHours { get; set; } = TimeSpan.FromHours(8);

    public ICollection<UserProfile> Employees { get; set; } = new List<UserProfile>();
    public bool IsTimeBank { get; set; }
}
