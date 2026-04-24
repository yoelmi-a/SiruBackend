using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIRU.Core.Application.Dtos.Reports;
using SIRU.Core.Application.Interfaces.Reports;

namespace SIRU.Infrastructure.Shared.Pdf;

public class PdfReportService : IPdfReportService
{
    private const string FontFamily = "Helvetica";

    public PdfReportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateHiringTimeReport(HiringTimeReportDto data)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily(FontFamily));

                page.Header().Element(c => ComposeHeader(c, "Average Hiring Time Report"));
                page.Content().Element(c => ComposeHiringTimeContent(c, data));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GeneratePerformanceByDepartmentReport(IEnumerable<DepartmentPerformanceDto> data)
    {
        var dataList = data.ToList();
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily(FontFamily));

                page.Header().Element(c => ComposeHeader(c, "Performance by Department Report"));
                page.Content().Element(c => ComposePerformanceContent(c, dataList));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GenerateEmployeeReport(IEnumerable<EmployeeReportDto> data)
    {
        var dataList = data.ToList();
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily(FontFamily));

                page.Header().Element(c => ComposeHeader(c, "General Employee Report"));
                page.Content().Element(c => ComposeEmployeeContent(c, dataList));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, string title)
    {
        container.Column(column =>
        {
            column.Item().AlignCenter().Text(title).Bold().FontSize(18);
            column.Item().AlignCenter().PaddingTop(5).Text($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm}").FontSize(10).FontColor(Colors.Grey.Medium);
            column.Item().PaddingTop(15).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
        });
    }

    private static void ComposeHiringTimeContent(IContainer container, HiringTimeReportDto data)
    {
        container.PaddingTop(30).Column(column =>
        {
            column.Item().Text("Summary").Bold().FontSize(13);
            column.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                });

                table.Cell().Background(Colors.Grey.Lighten3).Padding(8).Text("Average Days").Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Padding(8).Text(data.AverageDays.ToString("F2"));

                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Text("Total Closed Vacancies").Bold();
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Text(data.TotalClosedVacancies.ToString());
            });
        });
    }

    private static void ComposePerformanceContent(IContainer container, List<DepartmentPerformanceDto> data)
    {
        container.PaddingTop(30).Column(column =>
        {
            column.Item().Text("Department Performance").Bold().FontSize(13);
            column.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Blue.Darken3).Padding(8).Text("Department").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken3).Padding(8).Text("Average Score").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken3).Padding(8).AlignRight().Text("Employees").Bold().FontColor(Colors.White);
                });

                for (var i = 0; i < data.Count; i++)
                {
                    var item = data[i];
                    var bgColor = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                    table.Cell().Background(bgColor).Padding(8).Text(item.DepartmentName);
                    table.Cell().Background(bgColor).Padding(8).Text(item.AverageScore.ToString("F2"));
                    table.Cell().Background(bgColor).Padding(8).AlignRight().Text(item.EmployeeCount.ToString());
                }
            });
        });
    }

    private static void ComposeEmployeeContent(IContainer container, List<EmployeeReportDto> data)
    {
        container.PaddingTop(30).Column(column =>
        {
            column.Item().Text("Employee List").Bold().FontSize(13);
            column.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Blue.Darken3).Padding(8).Text("Full Name").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken3).Padding(8).Text("Cedula").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken3).Padding(8).Text("Position").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken3).Padding(8).Text("Department").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken3).Padding(8).AlignCenter().Text("Active").Bold().FontColor(Colors.White);
                });

                for (var i = 0; i < data.Count; i++)
                {
                    var item = data[i];
                    var bgColor = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                    table.Cell().Background(bgColor).Padding(8).Text(item.FullName);
                    table.Cell().Background(bgColor).Padding(8).Text(item.Cedula);
                    table.Cell().Background(bgColor).Padding(8).Text(item.Position);
                    table.Cell().Background(bgColor).Padding(8).Text(item.Department);
                    table.Cell().Background(bgColor).Padding(8).AlignCenter().Text(item.IsActive ? "Yes" : "No");
                }
            });
        });
    }

    private static void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(text =>
        {
            text.Span("Page ");
            text.CurrentPageNumber();
            text.Span(" of ");
            text.TotalPages();
        });
    }
}