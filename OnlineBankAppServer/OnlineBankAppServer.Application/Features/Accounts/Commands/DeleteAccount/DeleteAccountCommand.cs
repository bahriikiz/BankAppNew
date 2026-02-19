using MediatR;
using OnlineBankAppServer.Domain.Abstractions; 

namespace OnlineBankAppServer.Application.Features.Accounts.Commands.DeleteAccount;

public sealed record DeleteAccountCommand(
    int AccountId,
    int UserId 
) : IRequest<Result<string>>; // Başarılı/Başarısız sonucunu dönmek için Result kullanıyoruz