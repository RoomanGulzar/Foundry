using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ChangeRequestAnalyzer.Core.Entities;
using ChangeRequestAnalyzer.Core.Interfaces;
using ChangeRequestAnalyzer.Infrastructure.Repositories;

namespace ChangeRequestAnalyzer.Infrastructure.Services
{
    public sealed class ChangeRequestAnalysisService : IChangeRequestAnalyzer
    {
        private readonly IAzureFoundryOrchestrationService _foundryService;
        private readonly IChangeRequestRepository _repository;

        public ChangeRequestAnalysisService(
            IAzureFoundryOrchestrationService foundryService,
            IChangeRequestRepository repository)
        {
            _foundryService = foundryService;
            _repository = repository;
        }

        public async Task<AnalysisResult> AnalyzeAsync(int changeRequestId, CancellationToken cancellationToken = default)
        {
            var changeRequest = await _repository.GetWithDetailsAsync(changeRequestId, cancellationToken)
                ?? throw new InvalidOperationException($"Change request {changeRequestId} could not be found.");

            var analysisPrompt = BuildAnalysisPrompt(changeRequest);
            var foundryResponse = await _foundryService.AnalyzeChangeRequestAsync(analysisPrompt, cancellationToken);

            var result = new AnalysisResult
            {
                ChangeRequestId = changeRequest.Id,
                TargetFiles = JsonSerializer.Serialize(foundryResponse.TargetFiles),
                ConflictingFiles = JsonSerializer.Serialize(foundryResponse.ConflictingFiles),
                GeneratedPrompt = foundryResponse.GeneratedPrompt,
                AnalyzedAt = DateTime.UtcNow
            };

            changeRequest.Status = ChangeRequestStatus.Analyzed;
            await _repository.AddAnalysisResultAsync(result, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return result;
        }

        private static string BuildAnalysisPrompt(ChangeRequest changeRequest)
        {
            var userStories = string.Join(Environment.NewLine, changeRequest.UserStories.Select(story =>
                $"- {story.StoryIdentifier}: {story.RequirementText}"));

            return $"Analyze the following change request and determine target files, possible conflicting files from prior stories, and a prompt for code-base modification." +
                   $"\n\nTitle: {changeRequest.Title}" +
                   $"\nDescription: {changeRequest.Description}" +
                   $"\n\nUser Stories:\n{userStories}";
        }
    }
}
