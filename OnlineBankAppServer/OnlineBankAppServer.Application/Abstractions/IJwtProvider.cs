using OnlineBankAppServer.Domain.Entities;
namespace OnlineBankAppServer.Application.Abstractions
{
    public interface IJwtProvider
    {
        string CreateToken(User user);
    }
}
