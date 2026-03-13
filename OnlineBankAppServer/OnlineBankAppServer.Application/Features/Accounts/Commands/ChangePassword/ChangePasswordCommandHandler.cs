using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineBankAppServer.Persistance;
using System.Security.Claims;

namespace OnlineBankAppServer.Application.Features.Accounts.Commands.ChangePassword;

internal sealed class ChangePasswordCommandHandler(
    AppDbContext context,
    IHttpContextAccessor httpContextAccessor) : IRequestHandler<ChangePasswordCommand, string>
{
    public async Task<string> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        // Kimliği Tokendan Çöz
        var userIdClaim = (httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)
                          ?? httpContextAccessor.HttpContext?.User.FindFirst("sub")) ?? throw new Exception("Kimlik doğrulanamadı.");
        int userId = int.Parse(userIdClaim.Value);
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken) ?? throw new Exception("Kullanıcı bulunamadı.");
        bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);

        if (!isPasswordCorrect)
        {
            throw new Exception("Mevcut şifreniz hatalı!");
        }

        // YENİ ŞİFREYİ HASHLEYEREK KAYDET
        string newHashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.PasswordHash = newHashedPassword;

        context.Update(user);
        await context.SaveChangesAsync(cancellationToken);

        return "Şifreniz başarıyla değiştirildi. Güvenliğiniz için lütfen yeniden giriş yapın.";
    }
}