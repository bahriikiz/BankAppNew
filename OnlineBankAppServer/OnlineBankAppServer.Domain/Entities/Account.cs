using OnlineBankAppServer.Domain.Abstractions;

namespace OnlineBankAppServer.Domain.Entities;

public sealed class Account : Entity
{
    // --- BANKA HESABI ---
    public string Iban { get; set; } = string.Empty;
    public decimal Balance { get; set; } = 0;
    public string CurrencyType { get; set; } = "TRY";
    public int UserId { get; set; }
    public User? User { get; set; } 

    public int BankId { get; set; }
    public Bank? Bank { get; set; }
    public ICollection<BankTransaction> Transactions { get; set; } = new List<BankTransaction>();
    // --- AÇIK BANKACILIK ---
    public string AccountName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string ProviderBank { get; set; } = "BankAppNew";
    public decimal? AvailableBalance { get; set; }
    public string AccountType { get; set; } = "Vadesiz";
    public bool IsActive { get; set; } = true; 
    public DateTime? LastTransactionDate { get; set; }
}