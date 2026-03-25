using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineBankAppServer.Domain.Entities;
using OnlineBankAppServer.Persistance;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Security.Claims;

namespace OnlineBankAppServer.Application.Features.Accounts.Queries.GetStatementPdf;

internal sealed class GetAccountStatementPdfQueryHandler(
    AppDbContext context,
    IHttpContextAccessor httpContextAccessor) : IRequestHandler<GetAccountStatementPdfQuery, byte[]>
{
    public async Task<byte[]> Handle(GetAccountStatementPdfQuery request, CancellationToken cancellationToken)
    {
        var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null)
            throw new UnauthorizedAccessException("Kullanıcı bulunamadı. Lütfen giriş yapınız.");

        int userId = int.Parse(userIdClaim.Value);

        var account = await context.Accounts
            .Include(a => a.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.AccountId && x.UserId == userId, cancellationToken)
            ?? throw new KeyNotFoundException("Hesap bulunamadı veya bu işlem için yetkiniz yok.");

        var transactions = await context.BankTransactions
            .AsNoTracking()
            .Where(x => x.AccountId == account.Id || x.TargetIban == account.Iban)
            .OrderByDescending(x => x.TransactionDate)
            .Take(50)
            .ToListAsync(cancellationToken);

        string currency = account.CurrencyType == "1" ? "TRY" : "USD";

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, QuestPDF.Infrastructure.Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));

                page.Header().Element(c => ComposeHeader(c, account, currency));
                page.Content().Element(c => ComposeContent(c, account, transactions));

                page.Footer().AlignCenter().Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, Account account, string currency)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("İKİZ BANK A.Ş.").FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
                column.Item().Text("Resmi Hesap Ekstresi").FontSize(14).FontColor(Colors.Grey.Medium);
                column.Item().PaddingTop(10).Text($"Sayın {account.User!.FirstName} {account.User.LastName}").SemiBold();
                column.Item().Text($"IBAN: {account.Iban}");
                column.Item().Text($"Güncel Bakiye: {account.Balance:N2} {currency}").SemiBold();
            });

            row.ConstantItem(100).AlignRight().Text($"{DateTime.Now:dd.MM.yyyy}");
        });
    }

    private static void ComposeContent(IContainer container, Account account, List<BankTransaction> transactions)
    {
        container.PaddingVertical(1, QuestPDF.Infrastructure.Unit.Centimetre).Column(column =>
        {
            column.Spacing(5);
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(100);
                    columns.RelativeColumn();
                    columns.ConstantColumn(80);
                    columns.ConstantColumn(70);
                });

                table.Header(ComposeTableHeader);

                foreach (var tx in transactions)
                {
                    bool isOutgoing = tx.AccountId == account.Id;
                    string txType = isOutgoing ? "Giden" : "Gelen";
                    var color = isOutgoing ? Colors.Red.Medium : Colors.Green.Medium;
                    string sign = isOutgoing ? "-" : "+";

                    table.Cell().Element(CellStyle).Text(tx.TransactionDate.AddHours(3).ToString("dd.MM.yyyy HH:mm"));
                    table.Cell().Element(CellStyle).Text(tx.Description ?? "Transfer");
                    table.Cell().Element(CellStyle).AlignRight().Text($"{sign}{tx.Amount:N2}").FontColor(color);
                    table.Cell().Element(CellStyle).AlignCenter().Text(txType).FontColor(color);
                }
            });
        });
    }

    private static void ComposeTableHeader(TableCellDescriptor header)
    {
        header.Cell().Element(HeaderCellStyle).Text("Tarih");
        header.Cell().Element(HeaderCellStyle).Text("Açıklama");
        header.Cell().Element(HeaderCellStyle).AlignRight().Text("Tutar");
        header.Cell().Element(HeaderCellStyle).AlignCenter().Text("Tür");
    }

    private static IContainer HeaderCellStyle(IContainer container)
    {
        return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
    }

    private static IContainer CellStyle(IContainer container)
    {
        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
    }
}