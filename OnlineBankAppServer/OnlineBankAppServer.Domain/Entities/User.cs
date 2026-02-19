using OnlineBankAppServer.Domain.Abstractions;

namespace OnlineBankAppServer.Domain.Entities
{
    public sealed class User : Entity
    {
        public string FirstName { get; set; } = string.Empty;// kullanıcının adı
        public string LastName { get; set; } = string.Empty;// kullanıcının soyadı
        public string Email { get; set; } = string.Empty; // kullanıcının e-posta adresi
        public string PasswordHash { get; set; } = string.Empty; // kullanıcının şifre hash'i, güvenlik için şifreler düz metin olarak saklanmaz
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // kullanıcının oluşturulma tarihi, varsayılan olarak UTC zamanında atanır
        public ICollection<Account> Accounts { get; set; } = []; // kullanıcının sahip olduğu hesapların koleksiyonu
        public string Role { get; set; } = "Customer"; // Varsayılan olarak herkes Müşteri
    }
}
