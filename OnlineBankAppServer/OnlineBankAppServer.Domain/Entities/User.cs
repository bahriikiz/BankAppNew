using OnlineBankAppServer.Domain.Abstractions;

namespace OnlineBankAppServer.Domain.Entities
{
    public sealed class User : Entity
    {
        public string FirstName { get; set; } = string.Empty;// kullanıcının adı
        public string LastName { get; set; } = string.Empty;// kullanıcının soyadı
        public string Email { get; set; } = string.Empty; // kullanıcının e-posta adresi
        public string PasswordHash { get; set; } = string.Empty; // kullanıcının şifre hash'i, güvenlik için şifreler düz metin olarak saklanmaz
        public string IdentityNumber { get; set; } = string.Empty; // kullanıcının kimlik numarası, benzersiz bir tanımlayıcı olarak kullanılabilir
        public string PhoneNumber { get; set; } = string.Empty; // kullanıcının telefon numarası
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // kullanıcının oluşturulma tarihi, varsayılan olarak UTC zamanında atanır
        public ICollection<Account> Accounts { get; set; } = []; // kullanıcının sahip olduğu hesapların koleksiyonu
        public string Role { get; set; } = "Customer"; // Varsayılan olarak herkes Müşteri
        public string City { get; set; } = string.Empty; // İl (Örn: İSTANBUL)
        public string District { get; set; } = string.Empty; // İlçe (Örn: ŞİŞLİ)
        public string Neighborhood { get; set; } = string.Empty; // Mahalle (Örn: FULYA MAH.)
        public string Adress { get; set; } = string.Empty; // Sadece Sokak, Bina, Kapı No detayı için
        public Guid SecurityStamp { get; set;} = Guid.NewGuid(); // Giriş çıkış ve şifre değişiminde damga güncellenecek
        public string? RefreshToken {  get; set; } = string.Empty;
        public DateTime? RefreshTokenExpires { get; set; }
    }
}
