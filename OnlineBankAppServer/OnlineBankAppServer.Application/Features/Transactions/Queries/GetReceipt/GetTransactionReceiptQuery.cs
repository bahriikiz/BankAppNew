using MediatR;

namespace OnlineBankAppServer.Application.Features.Transactions.Queries.GetReceipt;

public sealed record GetTransactionReceiptQuery(
    int AccountId,
    string TransactionId,
    string Format // "1" -> pdf, "2" -> txt
) : IRequest<string>;