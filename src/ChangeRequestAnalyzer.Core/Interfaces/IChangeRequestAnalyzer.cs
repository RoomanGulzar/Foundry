using System.Threading;
using System.Threading.Tasks;
using ChangeRequestAnalyzer.Core.Entities;

namespace ChangeRequestAnalyzer.Core.Interfaces
{
    public interface IChangeRequestAnalyzer
    {
        Task<AnalysisResult> AnalyzeAsync(int changeRequestId, CancellationToken cancellationToken = default);
    }
}
