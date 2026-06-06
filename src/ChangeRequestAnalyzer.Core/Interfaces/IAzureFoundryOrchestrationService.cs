using System.Threading;
using System.Threading.Tasks;
using ChangeRequestAnalyzer.Core.Models;

namespace ChangeRequestAnalyzer.Core.Interfaces
{
    public interface IAzureFoundryOrchestrationService
    {
        Task<FoundryAnalysisResponse> AnalyzeChangeRequestAsync(string prompt, CancellationToken cancellationToken = default);
    }
}
