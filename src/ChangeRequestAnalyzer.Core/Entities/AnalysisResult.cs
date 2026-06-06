using System;

namespace ChangeRequestAnalyzer.Core.Entities
{
    public sealed class AnalysisResult
    {
        public int Id { get; set; }
        public int ChangeRequestId { get; set; }
        public ChangeRequest ChangeRequest { get; set; } = null!;
        public string TargetFiles { get; set; } = string.Empty;
        public string ConflictingFiles { get; set; } = string.Empty;
        public string GeneratedPrompt { get; set; } = string.Empty;
        public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    }
}
