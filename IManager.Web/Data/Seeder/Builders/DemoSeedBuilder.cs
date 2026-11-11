using IManager.Web.Data.Seeder.SeedDatas;
using IManager.Web.Domain.Consts;
using IManager.Web.Domain.Enums;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace IManager.Web.Data.Seeder.Builders;

public static class DemoSeedBuilder
{
    private static readonly HashSet<string> UsedCompanyNames = new();
    private static readonly HashSet<string> UsedEmails = new();

    public static DemoSeedData Build()
    {
        string fantasyName;
        do
        {
            fantasyName = GenerateCompanyName();
        }
        while (!UsedCompanyNames.Add(NormalizeKey(fantasyName)));

        var companyId = Guid.NewGuid();

        var company = new CompanySeedData(
            companyId,
            GenerateCnpj(),
            $"{fantasyName} LTDA",
            fantasyName,
            RandomFoundedDate()
        );

        var departments = BuildDepartments(companyId);
        var jobTitles = BuildJobTitles(departments);
        var users = BuildUsers(companyId, jobTitles, fantasyName);
        var entries = BuildTimeEntries(users);
        entries.AddRange(BuildAdjustments(users, entries));

        return new DemoSeedData
        {
            Company = company,
            Departments = departments,
            JobTitles = jobTitles,
            Users = users,
            Entries = entries,
        };
    }

    private static List<TimeEntrySeedData> BuildTimeEntries(
     List<UserSeedData> users)
    {
        if (users.Count == 0)
            return [];

        var timeZone = TimeZoneInfo.Local;
        var utilDays = GetUtilDaysFromLastYears(2);

        var entries = new List<TimeEntrySeedData>(
            users.Count * utilDays.Count
        );

        foreach (var user in users)
        {
            foreach (var day in utilDays)
            {
                var entry = new TimeEntrySeedData
                {
                    Id = Guid.NewGuid(),
                    EmployeeId = user.Id,
                    Date = day,
                    Checks = []
                };

                foreach (var time in GenerateWorkDayChecks())
                {
                    entry.Checks.Add(
                        CreateTimeCheck(
                            entry.Id,
                            day,
                            time,
                            timeZone
                        )
                    );
                }

                entries.Add(entry);
            }
        }

        return entries;
    }

    private static readonly string[] AdjustmentReasons =
    [
        "Esqueci de registrar a saída",
        "Hora extra autorizada pelo gestor",
        "Reunião com cliente após o expediente",
        "Falha no aplicativo ao bater o ponto",
        "Consulta médica no início do dia",
        "Treinamento externo"
    ];

    /// <summary>
    /// Gera correções de ponto aleatórias no último mês (o mês que fica sem processar):
    /// de 0 a 2 por funcionário, ~60% pendentes, ~25% aprovadas e ~15% reprovadas pelo admin.
    /// </summary>
    private static List<TimeEntrySeedData> BuildAdjustments(
        List<UserSeedData> users,
        List<TimeEntrySeedData> entries)
    {
        var admin = users.FirstOrDefault(u => u.Role == Role.Admin);
        if (admin is null || entries.Count == 0)
            return [];

        var lastDate = entries.Max(e => e.Date);
        var monthStart = new DateOnly(lastDate.Year, lastDate.Month, 1);
        var now = DateTime.UtcNow;

        var employeeIds = users
            .Where(u => u.Role == Role.User)
            .Select(u => u.Id)
            .ToHashSet();

        var adjustments = new List<TimeEntrySeedData>();

        var entriesByEmployee = entries
            .Where(e => employeeIds.Contains(e.EmployeeId) && e.Date >= monthStart)
            .GroupBy(e => e.EmployeeId);

        foreach (var employeeEntries in entriesByEmployee)
        {
            var count = RandomNumberGenerator.GetInt32(0, 3);

            var parents = employeeEntries
                .OrderBy(_ => RandomNumberGenerator.GetInt32(int.MaxValue))
                .Take(count);

            foreach (var parent in parents)
            {
                var roll = RandomNumberGenerator.GetInt32(100);
                var status = roll < 60 ? TimeEntryStatus.Pending
                           : roll < 85 ? TimeEntryStatus.Accepted
                           : TimeEntryStatus.Rejected;

                // solicitado no dia seguinte, auditado algumas horas depois (nunca no futuro)
                var createdAt = Min(parent.Date.ToDateTime(new TimeOnly(13, 0), DateTimeKind.Utc).AddDays(1), now.AddHours(-3));
                var auditedAt = Min(createdAt.AddHours(RandomNumberGenerator.GetInt32(1, 6)), now.AddHours(-1));

                var adjustment = new TimeEntrySeedData
                {
                    Id = Guid.NewGuid(),
                    EmployeeId = parent.EmployeeId,
                    Date = parent.Date,
                    ParentId = parent.Id,
                    Status = status,
                    IsCurrent = status == TimeEntryStatus.Accepted,
                    AdjustmentReason = AdjustmentReasons[RandomNumberGenerator.GetInt32(AdjustmentReasons.Length)],
                    RejectionReason = status == TimeEntryStatus.Rejected
                        ? "Ajuste não autorizado pelo gestor"
                        : null,
                    CreatedAt = createdAt,
                    LastModified = status == TimeEntryStatus.Pending ? null : auditedAt,
                    LastModifierId = status == TimeEntryStatus.Pending ? null : admin.Id
                };

                adjustment.Checks = BuildAdjustedChecks(parent, adjustment.Id);

                // aprovado: o ajuste passa a ser o registro vigente do dia
                if (status == TimeEntryStatus.Accepted)
                    parent.IsCurrent = false;

                adjustments.Add(adjustment);
            }
        }

        return adjustments;
    }

    /// <summary>
    /// Copia as batidas originais alterando uma delas:
    /// saída de 30 a 120 min mais tarde, ou entrada de 15 a 60 min mais cedo.
    /// </summary>
    private static List<TimeCheckSeedData> BuildAdjustedChecks(TimeEntrySeedData parent, Guid adjustmentId)
    {
        var ordered = parent.Checks.OrderBy(c => c.Timestamp).ToList();
        var lateExit = RandomNumberGenerator.GetInt32(2) == 0;

        return ordered
            .Select((check, index) =>
            {
                var timestamp = check.Timestamp;

                if (lateExit && index == ordered.Count - 1)
                    timestamp = timestamp.AddMinutes(RandomNumberGenerator.GetInt32(30, 121));
                else if (!lateExit && index == 0)
                    timestamp = timestamp.AddMinutes(-RandomNumberGenerator.GetInt32(15, 61));

                return new TimeCheckSeedData
                {
                    TimeEntryId = adjustmentId,
                    Timestamp = timestamp,
                    TimeZoneId = check.TimeZoneId
                };
            })
            .ToList();
    }

    private static DateTime Min(DateTime a, DateTime b) => a < b ? a : b;

    private static TimeCheckSeedData CreateTimeCheck(
        Guid timeEntryId,
        DateOnly date,
        TimeOnly time,
        TimeZoneInfo timeZone)
    {
        // O horário ainda representa "hora local",
        // portanto não atribuímos Local/Utc ao DateTime.
        var localDateTime = date.ToDateTime(
            time,
            DateTimeKind.Unspecified
        );

        // Agora convertemos explicitamente:
        // horário local do timezone -> UTC.
        var utcDateTime = TimeZoneInfo.ConvertTimeToUtc(
            localDateTime,
            timeZone
        );

        return new TimeCheckSeedData
        {
            TimeEntryId = timeEntryId,
            Timestamp = utcDateTime,
            TimeZoneId = timeZone.Id
        };
    }

    private static TimeOnly[] GenerateWorkDayChecks()
    {
        return
        [
            GenerateRandomTime(9),
        GenerateRandomTime(12),
        GenerateRandomTime(13),
        GenerateRandomTime(18)
        ];
    }

    private static TimeOnly GenerateRandomTime(int hour)
    {
        const int toleranceMinutes = 15;

        var randomMinutes = RandomNumberGenerator.GetInt32(
            -toleranceMinutes,
            toleranceMinutes + 1
        );

        return new TimeOnly(hour, 0)
            .AddMinutes(randomMinutes);
    }

    private static List<DateOnly> GetUtilDaysFromLastYears(int years)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var startDate = today.AddYears(-years);

        var days = new List<DateOnly>();

        for (
            var date = startDate;
            date <= today;
            date = date.AddDays(1)
        )
        {
            if (IsUtilDay(date))
                days.Add(date);
        }

        return days;
    }

    private static bool IsUtilDay(DateOnly date)
    {
        return date.DayOfWeek is
            >= DayOfWeek.Monday and
            <= DayOfWeek.Friday;
    }

    private static List<DepartmentSeedData> BuildDepartments(Guid companyId)
        => new[]
        {
            "Tecnologia","Financeiro","RH","Comercial","Operações","Marketing"
        }
        .Select(n => new DepartmentSeedData(Guid.NewGuid(), n, companyId))
        .ToList();

    private static List<JobTitleSeedData> BuildJobTitles(IEnumerable<DepartmentSeedData> departments)
    {
        var list = new List<JobTitleSeedData>();
        foreach (var d in departments)
        {
            list.Add(new JobTitleSeedData(Guid.NewGuid(), $"Analista de {d.Name}", d.Id));
            list.Add(new JobTitleSeedData(Guid.NewGuid(), $"Especialista em {d.Name}", d.Id));
        }
        return list;
    }

    private static List<UserSeedData> BuildUsers(
        Guid companyId,
        IReadOnlyList<JobTitleSeedData> jobTitles,
        string fantasyName)
    {
        var firstNames = new[]
        {
            "Gabriel","Ana","Lucas","Mariana","Carlos","Beatriz","Felipe","Juliana",
            "Rafael","Camila","Bruno","Laura","Thiago","Renata","Eduardo","Patricia",
            "Diego","Fernanda","Rodrigo","Aline"
        };

        var lastNames = new[]
        {
            "Souza","Silva","Oliveira","Pereira","Lima","Gomes","Ribeiro","Almeida",
            "Costa","Araujo","Rocha","Martins","Barbosa","Ferreira","Teixeira","Freitas"
        };

        var users = new List<UserSeedData>();

        var domain = NormalizeKey(fantasyName);
        var adminEmail = GenerateUniqueEmail($"admin@{domain}.com");

        users.Add(new UserSeedData(
            Guid.NewGuid(),
            adminEmail,
            DemoSeed.AdminPassword,
            Role.Admin,
            $"{fantasyName} Administrator",
            GenerateCpf(),
            new DateOnly(2000, 1, 1),
            companyId,
            jobTitles[0].Id,
            0m
        ));

        for (int i = 0; i < 20; i++)
        {
            var first = firstNames[RandomNumberGenerator.GetInt32(firstNames.Length)];
            var last = lastNames[RandomNumberGenerator.GetInt32(lastNames.Length)];
            var job = jobTitles[RandomNumberGenerator.GetInt32(jobTitles.Count)];

            var baseEmail = NormalizeKey($"{first}.{last}") + "@demo.com";
            var email = GenerateUniqueEmail(baseEmail);

            users.Add(new UserSeedData(
                Guid.NewGuid(),
                email,
                DemoSeed.UserPassword,
                Role.User,
                $"{first} {last}",
                GenerateCpf(),
                RandomBirthDate(),
                companyId,
                job.Id,
                GenerateBaseSalary()
            ));
        }

        return users;
    }

    private static string GenerateUniqueEmail(string baseEmail)
    {
        var email = baseEmail;
        var i = 1;

        while (!UsedEmails.Add(email))
        {
            var parts = baseEmail.Split('@');
            email = $"{parts[0]}{i}@{parts[1]}";
            i++;
        }

        return email;
    }

    private static string GenerateCompanyName()
    {
        var prefixes = new[] { "Nova", "Global", "Alpha", "Prime", "Tech", "Digital", "Smart", "Next" };
        var cores = new[] { "Solution", "Systems", "Soft", "Tecnologia", "Consultoria", "Services", "Group", "Labs" };
        return $"{prefixes[RandomNumberGenerator.GetInt32(prefixes.Length)]} {cores[RandomNumberGenerator.GetInt32(cores.Length)]}";
    }

    private static string GeneratePassword(int length = 12)
    {
        if (length < 6)
            throw new ArgumentException("Password length must be at least 6.");

        var upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        var lower = "abcdefghijkmnopqrstuvwxyz";
        var digits = "0123456789";
        var symbols = "@#!?";

        var all = upper + lower + digits + symbols;

        var chars = new List<char>
    {
        upper[RandomNumberGenerator.GetInt32(upper.Length)],
        lower[RandomNumberGenerator.GetInt32(lower.Length)],
        digits[RandomNumberGenerator.GetInt32(digits.Length)],
        symbols[RandomNumberGenerator.GetInt32(symbols.Length)]
    };

        for (int i = chars.Count; i < length; i++)
            chars.Add(all[RandomNumberGenerator.GetInt32(all.Length)]);

        // embaralha para não ficar previsível
        return new string(
            chars.OrderBy(_ => RandomNumberGenerator.GetInt32(int.MaxValue)).ToArray()
        );
    }

    private static decimal GenerateBaseSalary()
        => RandomNumberGenerator.GetInt32(2000_00, 4000_01) / 100m;

    private static DateOnly RandomBirthDate()
        => new DateOnly(1980, 1, 1).AddDays(RandomNumberGenerator.GetInt32(0, 9000));

    private static DateOnly RandomFoundedDate()
        => new DateOnly(RandomNumberGenerator.GetInt32(1990, 2021), RandomNumberGenerator.GetInt32(1, 13), 1);

    private static string GenerateCpf()
    {
        int[] digits = new int[11];

        for (int i = 0; i < 9; i++)
            digits[i] = RandomNumberGenerator.GetInt32(0, 10);

        digits[9] = CalculateCpfDigit(digits, 9);

        digits[10] = CalculateCpfDigit(digits, 10);

        return string.Concat(digits);
    }

    private static int CalculateCpfDigit(int[] digits, int length)
    {
        int sum = 0;
        int weight = length + 1;

        for (int i = 0; i < length; i++)
            sum += digits[i] * weight--;

        int remainder = (sum * 10) % 11;
        return remainder == 10 ? 0 : remainder;
    }

    private static string GenerateCnpj()
    {
        int[] digits = new int[14];

        // Gera os 12 primeiros dígitos
        for (int i = 0; i < 12; i++)
            digits[i] = RandomNumberGenerator.GetInt32(0, 10);

        // Primeiro dígito verificador
        digits[12] = CalculateCnpjDigit(digits, 12);

        // Segundo dígito verificador
        digits[13] = CalculateCnpjDigit(digits, 13);

        return string.Concat(digits);
    }

    private static int CalculateCnpjDigit(int[] digits, int length)
    {
        int[] weights = length == 12
            ? new[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 }
            : new[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        int sum = 0;
        for (int i = 0; i < length; i++)
            sum += digits[i] * weights[i];

        int remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }

    private static string NormalizeKey(string text)
            => RemoveAccents(text).Replace(" ", "").ToLowerInvariant();

    private static string RemoveAccents(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}