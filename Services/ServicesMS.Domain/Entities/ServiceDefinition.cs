using System;
using ServicesMS.Domain.ValueObjects;
using ServicesMS.Domain.Exceptions;

namespace ServicesMS.Domain.Entities
{
    public class ServiceDefinition
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public Money BasePrice { get; private set; }
        public bool RequiresStock { get; private set; }
        public int Stock { get; private set; }
        public Guid EventId { get; private set; } 

        public ServiceDefinition(string name, string description, decimal basePrice, bool requiresStock, int stock, Guid eventId)
        {
            Id = Guid.NewGuid();
            Name = name;
            Description = description;
            BasePrice = (Money)basePrice;
            RequiresStock = requiresStock;
            Stock = stock;
            EventId = eventId;
        }

        protected ServiceDefinition() 
        { 
            Name = null!;
            Description = null!;
            BasePrice = null!;
        }

        public bool CheckAvailability(int quantity)
        {
            if (!RequiresStock) return true;
            return Stock >= quantity;
        }

        public void ReduceStock(int quantity)
        {
            if (RequiresStock)
            {
                if (Stock < quantity) throw new InsufficientStockException(Name);
                Stock -= quantity;
            }
        }
        
        public void IncreaseStock(int quantity)
        {
            if (RequiresStock)
            {
                Stock += quantity;
            }
        }
    }
}
