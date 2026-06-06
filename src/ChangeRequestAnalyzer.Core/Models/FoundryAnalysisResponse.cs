using System;
using System.Collections.Generic;

namespace ChangeRequestAnalyzer.Core.Models
{
    public sealed class FoundryAnalysisResponse
    {
        public IReadOnlyCollection<string> TargetFiles { get; init; } = Array.Empty<string>();
        public IReadOnlyCollection<string> ConflictingFiles { get; init; } = Array.Empty<string>();
        public string GeneratedPrompt { get; init; } = string.Empty;
    }
}
