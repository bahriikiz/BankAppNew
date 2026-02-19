using OnlineBankAppServer.Domain.Abstractions;

namespace OnlineBankAppServer.Domain.Entities;

public sealed class Beneficiary : Entity
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Iban { get; set; } = string.Empty;
}