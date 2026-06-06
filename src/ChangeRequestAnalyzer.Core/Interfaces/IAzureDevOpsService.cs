using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChangeRequestAnalyzer.Core.Entities;

namespace ChangeRequestAnalyzer.Core.Interfaces
{
    public interface IAzureDevOpsService
    {
        /// <summary>
        /// Given a set of work item ids (Azure DevOps numeric ids), return lightweight
        /// user story entities populated with identifier and requirement text.
        /// </summary>
        Task<IEnumerable<UserStory>> GetUserStoriesByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
    }
}
