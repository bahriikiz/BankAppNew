using OnlineBankAppServer.Domain.Abstractions;

namespace OnlineBankAppServer.Domain.Entities;

public sealed class Account : Entity
{
    public string Iban { get; set; } = string.Empty;
    public decimal Balance { get; set; } = 0;
    public string CurrencyType { get; set; } = "TRY";
    public int UserId { get; set; }
    public User? User { get; set; } 

    public int BankId { get; set; }
    public Bank? Bank { get; set; } 
    public required ICollection<BankTransaction> Transactions { get; set; }
}