namespace IManager.Web.Data.Seeder.SeedDatas;
public class DemoSeedData
{
    public CompanySeedData Company { get; set; } = default!;
    public IEnumerable<DepartmentSeedData> Departments { get; set; } = [];
    public IEnumerable<JobTitleSeedData> JobTitles { get; set; } = [];
    public IEnumerable<UserSeedData> Users { get; set; } = [];
    public IEnumerable<TimeEntrySeedData> Entries { get; set; } = [];
}