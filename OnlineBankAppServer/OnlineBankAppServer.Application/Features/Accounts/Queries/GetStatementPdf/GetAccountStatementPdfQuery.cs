using MediatR;

namespace OnlineBankAppServer.Application.Features.Accounts.Queries.GetStatementPdf;

public sealed record GetAccountStatementPdfQuery(
    int AccountId
) : IRequest<byte[]>;