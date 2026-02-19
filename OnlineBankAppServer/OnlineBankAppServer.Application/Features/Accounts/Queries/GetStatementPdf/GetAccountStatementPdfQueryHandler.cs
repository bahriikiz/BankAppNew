using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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
        // 1. Kullanıcıyı ve Hesabı Bul
        var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null) throw new Exception("Kullanıcı bulunamadı.");
        int userId = int.Parse(userIdClaim.Value);

        var account = await context.Accounts
            .Include(a => a.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.AccountId && x.UserId == userId, cancellationToken);

        if (account is null) throw new Exception("Hesap bulunamadı veya yetkiniz yok.");

        // 2. İşlem Geçmişini Çek (Son 50 işlem)
        var transactions = await context.BankTransactions
            .AsNoTracking()
            .Where(x => x.AccountId == account.Id || x.TargetIban == account.Iban)
            .OrderByDescending(x => x.TransactionDate)
            .Take(50)
            .ToListAsync(cancellationToken);

        string currency = account.CurrencyType =="1" ? "TRY" : "USD";

        // 3. PDF OLUŞTURMA (QuestPDF)
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, QuestPDF.Infrastructure.Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));

                // BAŞLIK KISMI
                page.Header().Element(ComposeHeader);

                // İÇERİK KISMI (Tablo)
                page.Content().Element(ComposeContent);

                // ALT BİLGİ KISMI (Sayfa numarası)
                page.Footer().AlignCenter().Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });

            // Başlık Tasarımı
            void ComposeHeader(IContainer container)
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

            // İçerik Tasarımı (İşlem Tablosu)
            void ComposeContent(IContainer container)
            {
                container.PaddingVertical(1, QuestPDF.Infrastructure.Unit.Centimetre).Column(column =>
                {
                    column.Spacing(5);

                    column.Item().Table(table =>
                    {
                        // Sütun Genişlikleri
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(100); // Tarih
                            columns.RelativeColumn();    // Açıklama
                            columns.ConstantColumn(80);  // Tutar
                            columns.ConstantColumn(70);  // Tür
                        });

                        // Tablo Başlıkları
                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Tarih");
                            header.Cell().Element(CellStyle).Text("Açıklama");
                            header.Cell().Element(CellStyle).AlignRight().Text("Tutar");
                            header.Cell().Element(CellStyle).AlignCenter().Text("Tür");

                            static IContainer CellStyle(IContainer container)
                            {
                                return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                            }
                        });

                        // Tablo İçeriği (Döngü)
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

                            static IContainer CellStyle(IContainer container)
                            {
                                return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                            }
                        }
                    });
                });
            }
        });

        return document.GeneratePdf();
    }
}