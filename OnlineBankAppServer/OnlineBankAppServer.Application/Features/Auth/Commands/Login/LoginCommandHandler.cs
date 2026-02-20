using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineBankAppServer.Application.Abstractions;
using OnlineBankAppServer.Persistance;

namespace OnlineBankAppServer.Application.Features.Auth.Commands.Login

{
    internal sealed class LoginCommandHandler(
    AppDbContext context,
    IJwtProvider jwtProvider) : IRequestHandler<LoginCommand, LoginCommandResponse>
    {
        public async Task<LoginCommandResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await context.Users
                .FirstOrDefaultAsync(p => p.Email == request.Email, cancellationToken);

            if (user is null)
            {
                return new LoginCommandResponse(string.Empty, string.Empty, string.Empty, "Kullanıcı bulunamadı.");
            }

            bool isPasswordMatch = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!isPasswordMatch)
            {
                return new LoginCommandResponse(string.Empty, string.Empty, string.Empty, "Şifre hatalı.");
            }

            // Gerçek bir uygulamada, JWT oluştururken daha fazla bilgi ekleyebilir ve güvenlik önlemleri alabilirsiniz.
            string token = jwtProvider.CreateToken(user);

            return new LoginCommandResponse(
    Token: token,
    FirstName: user.FirstName, 
    LastName: user.LastName,  
    Message: "Giriş başarılı."
);
        }
    }
}
