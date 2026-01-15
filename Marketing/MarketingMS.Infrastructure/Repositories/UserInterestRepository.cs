using MarketingMS.Domain.Entities;
using MarketingMS.Domain.Interfaces;
using MarketingMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MarketingMS.Infrastructure.Repositories
{
    public class UserInterestRepository : IUserInterestRepository
    {
        private readonly MarketingDbContext _context;

        public UserInterestRepository(MarketingDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<string>> GetTopCategoriesAsync(Guid userId, int count)
        {
            return await _context.UserInterests
                .Where(ui => ui.UserId == userId)
                .OrderByDescending(ui => ui.Score)
                .Take(count)
                .Select(ui => ui.Category)
                .ToListAsync();
        }

        public async Task UpdateInterestAsync(Guid userId, string category)
        {
             var interest = await _context.UserInterests
                .FirstOrDefaultAsync(ui => ui.UserId == userId && ui.Category == category);

            if (interest == null)
            {
                interest = new UserInterest(userId, category);
                _context.UserInterests.Add(interest);
            }
            else
            {
                interest.IncrementScore();
            }

            await _context.SaveChangesAsync();
        }

        public async Task SetInterestsAsync(Guid userId, List<string> categories)
        {
            var existing = await _context.UserInterests
                .Where(ui => ui.UserId == userId)
                .ToListAsync();

            foreach (var cat in categories)
            {
                var interest = existing.FirstOrDefault(e => e.Category == cat);
                if (interest == null)
                {
                    interest = new UserInterest(userId, cat);
                    interest.SetScore(100); 
                    _context.UserInterests.Add(interest);
                }
                else
                {
                    interest.SetScore(100);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
