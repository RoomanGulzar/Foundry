using System;
using System.Collections.Generic;

namespace ChangeRequestAnalyzer.Core.Entities
{
    public enum ChangeRequestStatus
    {
        Draft = 0,
        PendingAnalysis = 1,
        Analyzed = 2,
        Completed = 3,
        Blocked = 4
    }

    public sealed class ChangeRequest
    {
        private readonly List<UserStory> _userStories = new();
        private readonly List<AnalysisResult> _analysisResults = new();

        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ChangeRequestStatus Status { get; set; } = ChangeRequestStatus.PendingAnalysis;

        public IReadOnlyCollection<UserStory> UserStories => _userStories.AsReadOnly();
        public IReadOnlyCollection<AnalysisResult> AnalysisResults => _analysisResults.AsReadOnly();

        // Uploaded change request document (text). Stored for auditing and as a source
        // for deriving referenced work items. This is intentionally a plain string
        // so the domain remains serializable and simple.
        public string? DocumentFileName { get; set; }
        public string? DocumentContent { get; set; }
        public void AddUserStory(UserStory story)
        {
            if (story is null)
            {
                throw new ArgumentNullException(nameof(story));
            }

            story.ChangeRequest = this;
            _userStories.Add(story);
        }

        public void AddAnalysisResult(AnalysisResult result)
        {
            if (result is null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            result.ChangeRequest = this;
            _analysisResults.Add(result);
        }
    }
}
