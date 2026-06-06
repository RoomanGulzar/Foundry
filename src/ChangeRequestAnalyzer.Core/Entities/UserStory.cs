namespace ChangeRequestAnalyzer.Core.Entities
{
    public sealed class UserStory
    {
        public int Id { get; set; }
        public int ChangeRequestId { get; set; }
        public ChangeRequest ChangeRequest { get; set; } = null!;
        public string StoryIdentifier { get; set; } = string.Empty;
        public string RequirementText { get; set; } = string.Empty;
    }
}
