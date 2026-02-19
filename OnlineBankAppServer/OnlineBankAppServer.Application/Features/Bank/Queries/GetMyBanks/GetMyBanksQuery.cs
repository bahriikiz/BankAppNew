using MediatR;
using OnlineBankAppServer.Domain.Entities;

namespace OnlineBankAppServer.Application.Features.Banks.Queries.GetMyBanks;
// token ile giriş yapan kullanıcının sahip olduğu bankaları getirir
public sealed record GetMyBanksQuery() : IRequest<List<Bank>>;