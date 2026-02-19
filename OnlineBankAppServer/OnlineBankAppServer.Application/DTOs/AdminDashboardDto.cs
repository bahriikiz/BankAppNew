namespace OnlineBankAppServer.Application.DTOs;

public sealed record AdminDashboardDto(
    int TotalUsers,
    int TotalAccounts,
    List<CurrencyTotalDto> TotalBalances,
    List<AdminTransactionDto> RecentTransactions
);

public sealed record CurrencyTotalDto(
    string Currency,
    decimal Total
);

public sealed record AdminTransactionDto(
    string SenderName,
    string ReceiverIban,
    decimal Amount,
    string DateString,
    string Description
);