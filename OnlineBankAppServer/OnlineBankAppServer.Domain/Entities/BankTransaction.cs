using OnlineBankAppServer.Domain.Abstractions;
namespace OnlineBankAppServer.Domain.Entities
{
    public sealed class BankTransaction : Entity
    {
        public decimal Amount { get; set; } // işlemin tutarı
        public string Description { get; set; } = string.Empty; // işlemin açıklaması
        public string TargetIban { get; set; } = string.Empty; // işlemin hedef IBAN'ı, para transferi işlemleri için kullanılır
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow; // işlemin gerçekleştiği tarih, varsayılan olarak UTC zamanında atanır
        public int AccountId { get; set; } // işlemin ait olduğu hesabın Id'si, bu bir yabancı anahtar olarak kullanılır

    }
}
