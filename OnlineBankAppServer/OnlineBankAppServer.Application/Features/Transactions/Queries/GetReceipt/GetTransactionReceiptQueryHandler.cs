using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineBankAppServer.Application.Integration.Vakifbank;
using OnlineBankAppServer.Persistance;

namespace OnlineBankAppServer.Application.Features.Transactions.Queries.GetReceipt;

internal sealed class GetTransactionReceiptQueryHandler(
    AppDbContext context,
    IVakifbankService vakifbankService) : IRequestHandler<GetTransactionReceiptQuery, string>
{
    public async Task<string> Handle(GetTransactionReceiptQuery request, CancellationToken cancellationToken)
    {
        var account = await context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.AccountId, cancellationToken);

        if (account is null) throw new Exception("Hesap bulunamadı!");
        if (string.IsNullOrEmpty(account.RizaNo)) throw new Exception("Bu işlem sadece Açık Bankacılık (VakıfBank) hesapları için geçerlidir.");

        string cleanIban = account.Iban.Replace(" ", "");
        string accountNumber = cleanIban.Length >= 17 ? cleanIban.Substring(cleanIban.Length - 17) : cleanIban;

        var receiptResponse = await vakifbankService.GetReceiptAsync(
            account.RizaNo,
            accountNumber,
            request.TransactionId,
            request.Format,
            cancellationToken);

        if (receiptResponse?.Documents == null)
            throw new Exception("Dekont belgesi bulunamadı.");

        // Eğer 1 (PDF) seçildiyse Base64 PDF döner, 2 (TXT) seçildiyse normal metin döner
        return request.Format == "1"
            ? receiptResponse.Documents.PdfReceipt ?? "PDF İçeriği Boş"
            : receiptResponse.Documents.TextReceipt ?? "TXT İçeriği Boş";
    }
}