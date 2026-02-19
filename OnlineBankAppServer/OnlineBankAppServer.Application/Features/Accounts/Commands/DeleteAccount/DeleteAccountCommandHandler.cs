using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineBankAppServer.Domain.Abstractions;
using OnlineBankAppServer.Persistance;

namespace OnlineBankAppServer.Application.Features.Accounts.Commands.DeleteAccount;

internal sealed class DeleteAccountCommandHandler(
    AppDbContext context) : IRequestHandler<DeleteAccountCommand, Result<string>>
{
    public async Task<Result<string>> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        // 1. Hesabı Bul (UserId kontrolü ile)
        var account = await context.Accounts
            .FirstOrDefaultAsync(x => x.Id == request.AccountId && x.UserId == request.UserId, cancellationToken);

        if (account is null)
        {
            return Result<string>.Failure("Hesap bulunamadı veya bu işlem için yetkiniz yok.");
        }

        // 2. Bakiye Kontrolü (Kritik!)
        if (account.Balance > 0)
        {
            return Result<string>.Failure("Bakiyesi bulunan bir hesap kapatılamaz. Lütfen önce parayı transfer edin.");
        }

        // 3. Silme İşlemi (Hard Delete)

        context.Accounts.Remove(account);
        await context.SaveChangesAsync(cancellationToken);

        return Result<string>.Succeed("Hesap başarıyla kapatıldı.");
    }
}