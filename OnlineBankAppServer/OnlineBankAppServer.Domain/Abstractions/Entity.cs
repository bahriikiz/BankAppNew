namespace OnlineBankAppServer.Domain.Abstractions
{
    public abstract class Entity
    {
        public int Id { get; set; } // Primary key for the entity
        public DateTime CreatedDate { get; set; } 
    }
}
