namespace OnlineBankAppServer.Domain.Abstractions
{
    public abstract class Entity
    {
        public int Id { get; set; } // her entity'nin bir Id'si olmalı, bu genellikle veritabanında birincil anahtar olarak kullanılır
        public DateTime CreatedDate { get; set; } // entity'nin oluşturulma tarihini tutar
    }
}
