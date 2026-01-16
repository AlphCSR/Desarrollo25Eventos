using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketingMS.Domain.Entities;

namespace MarketingMS.Domain.Interfaces
{
    public interface IUserInterestRepository
    {
        Task<IEnumerable<string>> GetTopCategoriesAsync(Guid userId, int count);
        Task UpdateInterestAsync(Guid userId, string category);
        Task SetInterestsAsync(Guid userId, List<string> categories);
    }
}
