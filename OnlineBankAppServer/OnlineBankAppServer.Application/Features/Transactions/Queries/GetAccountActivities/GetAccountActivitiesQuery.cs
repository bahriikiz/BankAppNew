using MediatR;
using OnlineBankAppServer.Domain.Entities;

namespace OnlineBankAppServer.Application.Features.Transactions.Queries.GetAccountActivities;

public sealed record GetAccountActivitiesQuery(
    int AccountId
) : IRequest<List<BankTransaction>>;