namespace IManager.Web.Data.Seeder.SeedDatas;

public class TimeEntrySeedData
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public ICollection<TimeCheckSeedData> Checks { get; set; } = new List<TimeCheckSeedData>();
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
}
