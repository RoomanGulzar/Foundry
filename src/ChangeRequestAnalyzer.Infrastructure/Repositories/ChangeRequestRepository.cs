using System.Threading;
using System.Threading.Tasks;
using ChangeRequestAnalyzer.Core.Entities;
using ChangeRequestAnalyzer.Core.Interfaces;
using ChangeRequestAnalyzer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChangeRequestAnalyzer.Infrastructure.Repositories
{
    public sealed class ChangeRequestRepository : IChangeRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public ChangeRequestRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyCollection<ChangeRequest>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.ChangeRequests
                .AsNoTracking()
                .OrderByDescending(cr => cr.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public Task<ChangeRequest?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default)
        {
            return _context.ChangeRequests
                .Include(cr => cr.UserStories)
                .Include(cr => cr.AnalysisResults)
                .FirstOrDefaultAsync(cr => cr.Id == id, cancellationToken);
        }

        public Task AddAsync(ChangeRequest changeRequest, CancellationToken cancellationToken = default)
        {
            _context.ChangeRequests.Add(changeRequest);
            return Task.CompletedTask;
        }

        public Task AddUserStoriesAsync(IEnumerable<UserStory> userStories, CancellationToken cancellationToken = default)
        {
            _context.UserStories.AddRange(userStories);
            return Task.CompletedTask;
        }

        public Task AddAnalysisResultAsync(AnalysisResult analysisResult, CancellationToken cancellationToken = default)
        {
            _context.AnalysisResults.Add(analysisResult);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }
    }
}
