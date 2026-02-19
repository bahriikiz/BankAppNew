using MediatR;
using OnlineBankAppServer.Domain.Entities;

namespace OnlineBankAppServer.Application.Features.Banks.Queries.GetAllBanks;

public sealed record GetAllBanksQuery() : IRequest<List<Bank>>;