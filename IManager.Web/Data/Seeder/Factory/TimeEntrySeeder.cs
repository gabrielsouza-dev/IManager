using AutoMapper;
using IManager.Web.Data.Seeder.Interfaces;
using IManager.Web.Data.Seeder.SeedDatas;
using IManager.Web.Domain.Entities.TimeTrackings;
using IManager.Web.Domain.Interfaces.Repositories;

namespace IManager.Web.Data.Seeder.Factory;

public class TimeEntrySeeder : IEntitySeeder<TimeEntrySeedData>
{
    private readonly IRepository<TimeEntry> _repo;
    private readonly IMapper _mapper;

    public TimeEntrySeeder(IRepository<TimeEntry> repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task SeedAsync(IEnumerable<TimeEntrySeedData> data)
    {
        var items = data
            .OrderBy(x => x.EmployeeId)
            .ThenBy(x => x.Date)
            .ToList();

        var employeeTotalCount = items
            .DistinctBy(x => x.EmployeeId)
            .Count();

        var entryTotalCount = items.Count;

        const int batchSize = 1000;

        var processed = 0;

        foreach (var batch in items.Chunk(batchSize))
        {
            var entities = batch
                .Select(x => _mapper.Map<TimeEntry>(x))
                .ToList();

            await _repo.AddRangeAsync(entities);

            processed += entities.Count;

            var employeeCount = items
                .Take(processed)
                .Select(x => x.EmployeeId)
                .Distinct()
                .Count();

            Console.WriteLine(
                $"Entry: {processed}/{entryTotalCount} - " +
                $"Employee: {employeeCount}/{employeeTotalCount}"
            );
        }
    }
}