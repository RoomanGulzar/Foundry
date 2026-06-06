using System.Threading;
using System.Threading.Tasks;
using ChangeRequestAnalyzer.Core.Entities;

namespace ChangeRequestAnalyzer.Core.Interfaces
{
    public interface IChangeRequestRepository
    {
        Task<IReadOnlyCollection<ChangeRequest>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<ChangeRequest?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default);
        Task AddAsync(ChangeRequest changeRequest, CancellationToken cancellationToken = default);
        Task AddUserStoriesAsync(IEnumerable<UserStory> userStories, CancellationToken cancellationToken = default);
        Task AddAnalysisResultAsync(AnalysisResult analysisResult, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
