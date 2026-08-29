using AutoMapper;
using IManager.Web.Domain.Consts;
using IManager.Web.Domain.Entities.TimeTrackings;
using IManager.Web.Domain.Enums;
using IManager.Web.Domain.Interfaces.Persistence;
using IManager.Web.Domain.Interfaces.Repositories;
using IManager.Web.Shared.DTO.TimeTrackings;
using IManager.Web.Shared.Requests;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Security.Claims;
namespace IManager.Web.Presentation.Controllers;

//TODO: Implementar Service para TimeCheck e TimeEntry.
[ApiController]
[Route("api/timecheck")]
[Authorize(Roles = Role.User)]
public class TimeCheckApiController : ControllerBase
{
    private readonly IRepository<TimeEntry> _timeEntryRepository;
    private readonly IRepository<TimeCheck> _timeCheckRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<TimeCheckApiController> _logger;

    public TimeCheckApiController(IRepository<TimeEntry> timeEntryRepository, IRepository<TimeCheck> timeCheckRepository, 
        IUnitOfWork unitOfWork, IMapper mapper, ILogger<TimeCheckApiController> logger)
    {
        _timeEntryRepository = timeEntryRepository;
        _timeCheckRepository = timeCheckRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost]
    public async Task<IActionResult> Include(TimeCheckRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("TimeCheckRequest invalido.");
        }

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var timeEntry = (await _timeEntryRepository.GetAllAsync(q => q.Where(te =>
                                                                    te.EmployeeId == userId
                                                                    && te.Date == DateOnly.FromDateTime(request.Timestamp))))
                                                                    .FirstOrDefault();

            if (timeEntry == null)
            {
                timeEntry = new TimeEntry
                {
                    EmployeeId = userId,
                    Date = DateOnly.FromDateTime(request.Timestamp)
                };

                await _timeEntryRepository.AddAsync(timeEntry);
            }

            var timeCheck = new TimeCheck
            {
                TimeEntryId = timeEntry.Id,
                Timestamp = request.Timestamp,
                TimeZoneId = request.TimeZoneId
            };

            await _timeCheckRepository.AddAsync(timeCheck);

            await _unitOfWork.CommitAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Erro ao incluir TimeCheck");

            throw;
        }
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var timeEntries = await _timeEntryRepository.GetAllAsync(q => q.Where(te =>
                                                        (te.EmployeeId == userId)
                                                        && (te.IsCurrent || te.Status == TimeEntryStatus.Pending))
                                                        .Include(te => te.Checks));

        var currents = timeEntries.Where(te => te.IsCurrent).ToList();
        var pendings = timeEntries.Where(te => te.Status == TimeEntryStatus.Pending).ToList();

        var result = currents.Select(current =>
        {
            var pending = pendings.FirstOrDefault(p => p.ParentId == current.Id);
            return pending ?? current;
        })
        .ToList();

        var dto = _mapper.Map<IEnumerable<TimeEntryDTO>>(result);
        return Ok(dto);
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("all-entries-no-includes")]
    public async Task<IActionResult> GetAllNoIncludes()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var timeEntries = await _timeEntryRepository.GetAllAsync(q => q.Where(te =>
                                                        (te.EmployeeId == userId)
                                                        && (te.IsCurrent || te.Status == TimeEntryStatus.Pending)));

        var currents = timeEntries.Where(te => te.IsCurrent).ToList();
        var pendings = timeEntries.Where(te => te.Status == TimeEntryStatus.Pending).ToList();

        var result = currents.Select(current =>
        {
            var pending = pendings.FirstOrDefault(p => p.ParentId == current.Id);
            return pending ?? current;
        })
        .ToList();

        var dto = _mapper.Map<IEnumerable<TimeEntryDTO>>(result);
        return Ok(dto);
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        if (id == Guid.Empty)
            return NotFound();

        var timeEntry = await _timeEntryRepository.GetByIdAsync(id, q => q.Include(te => te.Checks));
        if(timeEntry is null)
            return NotFound();

        var dto = _mapper.Map<TimeEntryDTO>(timeEntry);
        return Ok(dto);
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpDelete("/{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var exists = await _timeEntryRepository.ExistsAsync(te => te.Id == id);
        if (id == Guid.Empty || !exists)
            return NotFound();

        await _timeEntryRepository.SoftDeleteAsync(id);

        return NoContent();
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPut]
    public async Task<IActionResult> Update(TimeCheckDTO request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("TimeCheckRequest invalido.");
        }

        var entity = _mapper.Map<TimeCheck>(request);
        entity.Timestamp = DateTime.SpecifyKind(entity.Timestamp, DateTimeKind.Utc);

        await _timeCheckRepository.UpdateAsync(entity);
        return NoContent();
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPut("timeentry/{id}")]
    public async Task<IActionResult> UpdateTimeEntry(TimeEntryDTO request, Guid id)
    {
        var original = await _timeEntryRepository.GetByIdAsync(id);
        if (original is null)
            return NotFound("TimeEntry não encontrado.");

        if (!ModelState.IsValid && original == null && original!.Id != request.Id)
        {
            return BadRequest("TimeCheckRequest invalido.");
        }

        var HasPending = await _timeEntryRepository.ExistsAsync(te =>
                                                    (te.Id == id && te.Status == TimeEntryStatus.Pending) 
                                                    || (te.ParentId == id && te.Status == TimeEntryStatus.Pending));

        if(HasPending) return BadRequest("Já existe uma TimeEntry pendente para esta data.");

        var entity = _mapper.Map<TimeEntry>(request);
        entity.Id = Guid.NewGuid();
        entity.EmployeeId = original!.EmployeeId;
        entity.Status = TimeEntryStatus.Pending;
        entity.IsCurrent = false;
        entity.ParentId = original.Id;

        foreach (var check in entity.Checks)
        {
            check.Id = Guid.NewGuid();
            check.Timestamp = DateTime.SpecifyKind(check.Timestamp, DateTimeKind.Utc);
        }
        entity.Checks.OrderBy(c => c.Timestamp);

        await _timeEntryRepository.AddAsync(entity);
        return NoContent();
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("get-by-day/{date}")]
    public async Task<IActionResult> GetTodayTimeChecks(DateOnly date)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var timeEntry = (await _timeEntryRepository.GetAllAsync(q => q.Where(te =>
                                                                te.EmployeeId == userId
                                                                && te.Date == date)
                                                                .Include(te => te.Checks))).FirstOrDefault();


        var dto = timeEntry != null? _mapper.Map<TimeEntryDTO>(timeEntry) : new TimeEntryDTO();
        return Ok(dto);
    }
}