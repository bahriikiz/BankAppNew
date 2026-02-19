namespace OnlineBankAppServer.Application.DTOs;

public sealed record AccountDetailDto(
    int Id,
    string Iban,
    decimal Balance,
    string Currency,
    List<AccountTransactionDto> Transactions
);

public sealed record AccountTransactionDto(
    decimal Amount,
    string Description,
    DateTime Date,
    string DateString,
    string Type,
    string Counterparty
);