using MediatR;
using OnlineBankAppServer.Domain.Entities;
using OnlineBankAppServer.Persistance;
namespace OnlineBankAppServer.Application.Features.Auth.Commands.CreateUser;


internal sealed class CreateUserCommandHandler(AppDbContext context) : IRequestHandler<CreateUserCommand, string>
{
    public async Task<string> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
        User user = new()
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = hashedPassword, // şifre hash leme işlemi
            CreatedAt = DateTime.UtcNow,
            Accounts = [],
        };
        await context.Users.AddAsync(user, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return "Kullanıcı başarıyla oluşturuldu.";

        }
}
