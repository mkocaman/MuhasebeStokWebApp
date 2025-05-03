using System;

namespace MuhasebeStokWebApp.Services.Exceptions
{
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException() : base("Belirtilen varlık bulunamadı.")
        {
        }

        public EntityNotFoundException(string message) : base(message)
        {
        }

        public EntityNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public EntityNotFoundException(string entityName, object entityId) 
            : base($"{entityName} tipi ve {entityId} ID değerine sahip varlık bulunamadı.")
        {
            EntityName = entityName;
            EntityId = entityId?.ToString();
        }

        public string? EntityName { get; }
        public string? EntityId { get; }
    }
} 