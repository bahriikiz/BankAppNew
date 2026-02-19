using OnlineBankAppServer.Domain.Abstractions;

namespace OnlineBankAppServer.Domain.Entities;

public sealed class Bank : Entity
{
    public string Name { get; set; } = string.Empty;
    public ICollection<Account>? Accounts { get; set; }
}