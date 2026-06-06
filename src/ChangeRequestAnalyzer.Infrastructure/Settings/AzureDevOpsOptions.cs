namespace ChangeRequestAnalyzer.Infrastructure.Settings
{
    public sealed class AzureDevOpsOptions
    {
        // e.g. https://dev.azure.com/yourOrg or https://dev.azure.com/yourOrg/project
        public string OrganizationUrl { get; set; } = string.Empty;
        // Project name to scope queries (optional)
        public string Project { get; set; } = string.Empty;
        // PAT - configure in appsettings or secret store; leave empty here
        public string PersonalAccessToken { get; set; } = string.Empty;
    }
}
