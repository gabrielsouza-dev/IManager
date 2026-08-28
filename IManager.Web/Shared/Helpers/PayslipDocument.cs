using System.Globalization;
using IManager.Web.Presentation.ViewModels.Payslips;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace IManager.Web.Shared.Helpers;

public sealed class PayslipDocument : IDocument
{
    private readonly PayslipViewModel _model;

    private static readonly CultureInfo _cultureInfo =
        CultureInfo.GetCultureInfo("pt-BR");

    public PayslipDocument(PayslipViewModel model)
    {
        _model = model;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer document)
    {
        document.Page(page =>
        {
            // Paisagem A5, orientação paisagem, margens de 18mm, fonte Arial 8pt
            page.Size(PageSizes.A5.Landscape());
            page.Margin(18);
            page.DefaultTextStyle(x => x
                .FontFamily("Arial")
                .FontSize(8)
                .FontColor(Colors.Black));

            page.Content()
                .Border(1)
                .BorderColor(Colors.Black)
                .Column(column =>
                {
                    column.Spacing(0);

                    column.Item().Element(ComposeHeader);
                    column.Item().Element(ComposeEmployee);
                    column.Item().Element(ComposeItems);
                    column.Item().Element(ComposeTotals);
                    column.Item().Element(ComposeFooter);
                });
        });
    }

    private void ComposeHeader(IContainer container)
    {
        container
            .BorderBottom(1)
            .Padding(6)
            .Row(row =>
            {
                row.RelativeItem(3)
                    .Column(column =>
                    {
                        column.Item()
                            .Text(_model.CompanyName)
                            .SemiBold()
                            .FontSize(10);

                        column.Item()
                            .PaddingTop(2)
                            .Text($"CNPJ/CPF: {_model.CompanyDocument}");

                        column.Item()
                            .PaddingTop(2)
                            .Text($"Emitido em: {_model.CreatedAt.ToLocalTime():dd/MM/yyyy}");
                    });

                row.RelativeItem(2)
                    .AlignRight()
                    .Column(column =>
                    {
                        column.Item()
                            .AlignRight()
                            .Text("RECIBO DE PAGAMENTO DE SALÁRIO")
                            .Bold()
                            .FontSize(11);

                        column.Item()
                            .PaddingTop(4)
                            .AlignRight()
                            .Text(
                                $"Referente a {GetMonthName(_model.ReferenceMonth)} / {_model.ReferenceYear}");
                    });
            });
    }

    private void ComposeEmployee(IContainer container)
    {
        container
            .BorderBottom(1)
            .PaddingVertical(5)
            .PaddingHorizontal(6)
            .Row(row =>
            {
                row.RelativeItem(1)
                    .Column(column =>
                    {
                        column.Item().Text("CÓDIGO").FontSize(6);
                        column.Item()
                            .Text(GetEmployeeCode())
                            .SemiBold();
                    });

                row.RelativeItem(4)
                    .Column(column =>
                    {
                        column.Item().Text("NOME DO FUNCIONÁRIO").FontSize(6);
                        column.Item()
                            .Text(_model.EmployeeName)
                            .SemiBold();
                    });

                row.RelativeItem(2)
                    .Column(column =>
                    {
                        column.Item().Text("CPF / DOCUMENTO").FontSize(6);
                        column.Item()
                            .Text(_model.EmployeeDocument)
                            .SemiBold();
                    });

                row.RelativeItem(2)
                    .Column(column =>
                    {
                        column.Item().Text("FUNÇÃO").FontSize(6);
                        column.Item()
                            .Text(_model.JobTitle)
                            .SemiBold();
                    });
            });
    }

    private void ComposeItems(IContainer container)
    {
        var earnings = GetEarnings();
        var deductions = GetDeductions();

        container
            .MinHeight(210)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(40);  // código
                    columns.RelativeColumn(4);   // descrição
                    columns.RelativeColumn(1.3f);// referência
                    columns.RelativeColumn(1.5f);// proventos
                    columns.RelativeColumn(1.5f);// descontos
                });

                table.Header(header =>
                {
                    HeaderCell(header.Cell(), "Cód.");
                    HeaderCell(header.Cell(), "Descrição");
                    HeaderCell(header.Cell(), "Referência");
                    HeaderCell(header.Cell(), "Proventos");
                    HeaderCell(header.Cell(), "Descontos");
                });

                foreach (var item in earnings)
                {
                    ItemCell(table.Cell(), item.Code);

                    ItemCell(
                        table.Cell(),
                        item.Description);

                    ItemCell(
                        table.Cell(),
                        item.Reference,
                        alignRight: true);

                    ItemCell(
                        table.Cell(),
                        Money(item.Value),
                        alignRight: true);

                    ItemCell(
                        table.Cell(),
                        string.Empty,
                        alignRight: true);
                }

                foreach (var item in deductions)
                {
                    ItemCell(table.Cell(), item.Code);

                    ItemCell(
                        table.Cell(),
                        item.Description);

                    ItemCell(
                        table.Cell(),
                        item.Reference,
                        alignRight: true);

                    ItemCell(
                        table.Cell(),
                        string.Empty,
                        alignRight: true);

                    ItemCell(
                        table.Cell(),
                        Money(item.Value),
                        alignRight: true);
                }
            });
    }

    private void ComposeTotals(IContainer container)
    {
        container
            .BorderTop(1)
            .Row(row =>
            {
                row.RelativeItem(5)
                    .Padding(5)
                    .Column(column =>
                    {
                        column.Item()
                            .Text("MENSAGENS")
                            .FontSize(6);

                        column.Item()
                            .PaddingTop(5)
                            .Text(
                                $"Competência: {_model.ReferenceMonth:00}/{_model.ReferenceYear}");
                    });

                row.RelativeItem(3)
                    .BorderLeft(1)
                    .Column(column =>
                    {
                        column.Item()
                            .Row(totalRow =>
                            {
                                totalRow.RelativeItem()
                                    .Padding(4)
                                    .Column(c =>
                                    {
                                        c.Item()
                                            .Text("Total de Vencimentos")
                                            .FontSize(6);

                                        c.Item()
                                            .AlignRight()
                                            .Text(Money(_model.GrossSalary))
                                            .SemiBold();
                                    });

                                totalRow.RelativeItem()
                                    .BorderLeft(1)
                                    .Padding(4)
                                    .Column(c =>
                                    {
                                        c.Item()
                                            .Text("Total de Descontos")
                                            .FontSize(6);

                                        c.Item()
                                            .AlignRight()
                                            .Text(Money(_model.TotalDeductions))
                                            .SemiBold();
                                    });
                            });

                        column.Item()
                            .BorderTop(1)
                            .Padding(5)
                            .Row(netRow =>
                            {
                                netRow.RelativeItem()
                                    .Text("Líquido a Receber")
                                    .Bold();

                                netRow.RelativeItem()
                                    .AlignRight()
                                    .Text(Money(_model.NetSalary))
                                    .Bold()
                                    .FontSize(10);
                            });
                    });
            });
    }

    private void ComposeFooter(IContainer container)
    {
        container
            .BorderTop(1)
            .PaddingVertical(4)
            .PaddingHorizontal(5)
            .Column(column =>
            {
                column.Item()
                    .Row(row =>
                    {
                        FooterValue(
                            row.RelativeItem(),
                            "Salário Base",
                            Money(_model.RegularSalary));

                        FooterValue(
                            row.RelativeItem(),
                            "Proventos Extras",
                            Money(_model.TotalExtraEarnings));

                        FooterValue(
                            row.RelativeItem(),
                            "INSS",
                            Money(_model.INSSDeduction));

                        FooterValue(
                            row.RelativeItem(),
                            "IRRF",
                            Money(_model.IRRFDeduction));

                        FooterValue(
                            row.RelativeItem(),
                            "Outros Descontos",
                            Money(_model.OtherDeductions));
                    });

                column.Item()
                    .PaddingTop(5)
                    .BorderTop(0.5f)
                    .Row(row =>
                    {
                        FooterValue(
                            row.RelativeItem(),
                            "Horas Regulares",
                            FormatHours(_model.RegularHours));

                        FooterValue(
                            row.RelativeItem(),
                            "Horas Extras",
                            FormatHours(_model.OvertimeHours));

                        FooterValue(
                            row.RelativeItem(),
                            "Horas Noturnas",
                            FormatHours(_model.NightShiftHours));

                        FooterValue(
                            row.RelativeItem(),
                            "Total Bruto",
                            Money(_model.GrossSalary));

                        FooterValue(
                            row.RelativeItem(),
                            "Líquido",
                            Money(_model.NetSalary));
                    });
            });
    }

    // =========================================================
    // Componentes visuais
    // =========================================================

    private static void HeaderCell(
        IContainer container,
        string text)
    {
        container
            .BorderBottom(1)
            .BorderRight(1)
            .PaddingVertical(3)
            .PaddingHorizontal(3)
            .AlignCenter()
            .Text(text)
            .SemiBold()
            .FontSize(7);
    }

    private static void ItemCell(
        IContainer container,
        string text,
        bool alignRight = false)
    {
        var cell = container
            .BorderRight(0.5f)
            .PaddingHorizontal(4)
            .PaddingVertical(2);

        if (alignRight)
        {
            cell
                .AlignRight()
                .Text(text);
        }
        else
        {
            cell.Text(text);
        }
    }

    private static void FooterValue(
        IContainer container,
        string title,
        string value)
    {
        container
            .AlignCenter()
            .Column(column =>
            {
                column.Item()
                    .AlignCenter()
                    .Text(title)
                    .FontSize(6);

                column.Item()
                    .AlignCenter()
                    .Text(value)
                    .SemiBold();
            });
    }

    // =========================================================
    // Dados
    // =========================================================

    private List<PayslipItem> GetEarnings()
    {
        var items = new List<PayslipItem>();

        AddIfPositive(
            items,
            "001",
            "SALÁRIO BASE",
            FormatHours(_model.RegularHours),
            _model.RegularSalary);

        AddIfPositive(
            items,
            "002",
            "HORAS EXTRAS",
            FormatHours(_model.OvertimeHours),
            _model.OvertimeAdditionals);

        AddIfPositive(
            items,
            "003",
            "ADICIONAL NOTURNO",
            FormatHours(_model.NightShiftHours),
            _model.NightShiftAdditionals);

        AddIfPositive(
            items,
            "004",
            "ADICIONAL PERICULOSIDADE",
            string.Empty,
            _model.HazardAdditionals);

        AddIfPositive(
            items,
            "005",
            "ADICIONAL INSALUBRIDADE",
            string.Empty,
            _model.UnhealthyAdditionals);

        AddIfPositive(
            items,
            "006",
            "COMISSÃO",
            string.Empty,
            _model.Commission);

        return items;
    }

    private List<PayslipItem> GetDeductions()
    {
        var items = new List<PayslipItem>();

        AddIfPositive(
            items,
            "101",
            "INSS",
            string.Empty,
            _model.INSSDeduction);

        AddIfPositive(
            items,
            "102",
            "IRRF",
            string.Empty,
            _model.IRRFDeduction);

        AddIfPositive(
            items,
            "199",
            "OUTROS DESCONTOS",
            string.Empty,
            _model.OtherDeductions);

        return items;
    }

    private static void AddIfPositive(
        ICollection<PayslipItem> items,
        string code,
        string description,
        string reference,
        decimal value)
    {
        if (value <= 0)
            return;

        items.Add(new PayslipItem(
            code,
            description,
            reference,
            value));
    }

    private static string Money(decimal value)
    {
        return value.ToString("N2", _cultureInfo);
    }

    private static string FormatHours(TimeSpan value)
    {
        var totalHours = (int)value.TotalHours;

        return $"{totalHours:00}:{value.Minutes:00}";
    }

    private string GetEmployeeCode()
    {
        return _model.EmployeeId
            .ToString("N")[..8]
            .ToUpperInvariant();
    }

    private static string GetMonthName(int month)
    {
        if (month is < 1 or > 12)
            return month.ToString();

        var name = _cultureInfo.DateTimeFormat.GetMonthName(month);

        return CultureInfo.CurrentCulture.TextInfo
            .ToTitleCase(name);
    }

    private sealed record PayslipItem(
        string Code,
        string Description,
        string Reference,
        decimal Value);
}