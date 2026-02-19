using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineBankAppServer.Domain.Abstractions;
using OnlineBankAppServer.Persistance;

namespace OnlineBankAppServer.Application.Features.Auth.Commands.DeleteUser;

internal sealed class DeleteUserCommandHandler(
    AppDbContext context) : IRequestHandler<DeleteUserCommand, Result<string>>
{
    public async Task<Result<string>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .Include(u => u.Accounts) // Hesaplarını da kontrol etmek için yüklüyoruz
            .FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);

        if (user is null)
        {
            return Result<string>.Failure("Kullanıcı bulunamadı.");
        }

        // Bakiye Kontrolü: Herhangi bir hesabında para var mı?
        bool hasMoney = user.Accounts != null && user.Accounts.Any(acc => acc.Balance > 0);

        if (hasMoney)
        {
            return Result<string>.Failure("Hesaplarınızda bakiye varken kullanıcı kaydınızı silemezsiniz. Lütfen bakiyelerinizi sıfırlayın.");
        }

        // Eğer para yoksa, kullanıcının hesaplarını ve kendisini siliyoruz (Cascade Delete mantığı)
        if (user.Accounts != null && user.Accounts.Any())
        {
            context.Accounts.RemoveRange(user.Accounts);
        }

        context.Users.Remove(user);
        await context.SaveChangesAsync(cancellationToken);

        return Result<string>.Succeed("Kullanıcı kaydınız ve tüm hesaplarınız başarıyla silindi.");
    }
}