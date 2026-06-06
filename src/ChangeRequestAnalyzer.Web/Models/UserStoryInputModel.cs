namespace ChangeRequestAnalyzer.Web.Models
{
    public sealed class UserStoryInputModel
    {
        public string StoryIdentifier { get; set; } = string.Empty;
        public string RequirementText { get; set; } = string.Empty;
    }
}
