using IManager.Web.Domain.Enums;

namespace IManager.Web.Data.Seeder.SeedDatas;

public class TimeEntrySeedData
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public ICollection<TimeCheckSeedData> Checks { get; set; } = new List<TimeCheckSeedData>();
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    // Campos usados pelas correções de ponto (ajustes) geradas no seed
    public TimeEntryStatus Status { get; set; } = TimeEntryStatus.Accepted;
    public bool IsCurrent { get; set; } = true;
    public Guid? ParentId { get; set; }
    public string? AdjustmentReason { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModified { get; set; }
    public Guid? LastModifierId { get; set; }
}
