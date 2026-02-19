using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineBankAppServer.Domain.Entities;
using OnlineBankAppServer.Persistance;

namespace OnlineBankAppServer.Application.Features.Transactions.Queries.GetAccountActivities;

internal sealed class GetAccountActivitiesQueryHandler(
    AppDbContext context) : IRequestHandler<GetAccountActivitiesQuery, List<BankTransaction>>
{
    public async Task<List<BankTransaction>> Handle(GetAccountActivitiesQuery request, CancellationToken cancellationToken)
    {
        // İlgili hesaba ait işlemleri bul ve Tarihe göre (Yeniden eskiye) sırala
        var transactions = await context.BankTransactions
            .Where(p => p.AccountId == request.AccountId)
            .OrderByDescending(p => p.TransactionDate)
            .ToListAsync(cancellationToken);

        return transactions;
    }
}