namespace OnlineBankAppServer.Application.Integration.Vakifbank.Dtos;

// 1. En dıştaki JSON yapısı
public class VakifbankAccountListResponseDto
{
    public VakifbankHeaderDto Header { get; set; } = new();
    public VakifbankDataDto Data { get; set; } = new();
}

// 2. Header kısmı
public class VakifbankHeaderDto
{
    public string StatusCode { get; set; } = string.Empty;
    public string StatusDescription { get; set; } = string.Empty;
    public string ObjectID { get; set; } = string.Empty;
}

// 3. Data kısmı
public class VakifbankDataDto
{
    public List<VakifbankAccountDto> Accounts { get; set; } = []; 
}

// 4. Asıl hesap bilgilerinin olduğu kısım
public class VakifbankAccountDto
{
    public string CurrencyCode { get; set; } = string.Empty;
    public DateTime? LastTransactionDate { get; set; } 
    public string AccountStatus { get; set; } = string.Empty;
    public string IBAN { get; set; } = string.Empty;
    public string RemainingBalance { get; set; } = string.Empty;
    public string Balance { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
}