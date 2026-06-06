using System;
using System.Collections.Generic;

namespace ChangeRequestAnalyzer.Web.Models
{
    public sealed class ChangeRequestDetailsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public IReadOnlyCollection<UserStoryInputModel> UserStories { get; init; } = Array.Empty<UserStoryInputModel>();
        public string TargetFiles { get; set; } = string.Empty;
        public string ConflictingFiles { get; set; } = string.Empty;
        public string GeneratedPrompt { get; set; } = string.Empty;
        public DateTime? AnalyzedAt { get; set; }
    }
}
