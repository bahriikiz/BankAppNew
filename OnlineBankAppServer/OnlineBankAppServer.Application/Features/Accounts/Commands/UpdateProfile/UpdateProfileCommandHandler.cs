using MediatR;  
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineBankAppServer.Application.Features.Auth.Commands.UpdateProfile;
using OnlineBankAppServer.Persistance;
using System.Security.Claims;


namespace OnlineBankAppServer.Application.Features.Accounts.Commands.UpdateProfile;

internal sealed class UpdateProfileCommandHandler(
    AppDbContext context,
    IHttpContextAccessor httpContextAccessor) : IRequestHandler<UpdateProfileCommand, string>
{
    public async Task<string> Handle (UpdateProfileCommand request , CancellationToken cancellationToken)
    {
        var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes .NameIdentifier) 
                          ?? httpContextAccessor.HttpContext?.User.FindFirst("sub");
        if (userIdClaim is null) throw new Exception("Kimlik dogrulanamadı.");
        
        int userId = int.Parse(userIdClaim.Value);

        var user =await context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null) throw new Exception("Kullanıcı bulunamadı.");

        user.PhoneNumber = request.PhoneNumber;
        user.Adress = request.Address;

        context.Update(user);
        await context.SaveChangesAsync(cancellationToken);

        return "Profiliniz başarıyla güncellendi.";
    }
}