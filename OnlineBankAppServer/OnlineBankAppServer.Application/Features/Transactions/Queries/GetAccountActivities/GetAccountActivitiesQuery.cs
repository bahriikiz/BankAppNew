using MediatR;
using OnlineBankAppServer.Domain.Entities;

namespace OnlineBankAppServer.Application.Features.Transactions.Queries.GetAccountActivities;

public sealed record GetAccountActivitiesQuery(
    int AccountId,
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<List<BankTransaction>>;