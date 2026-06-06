using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ChangeRequestAnalyzer.Core.Entities;
using ChangeRequestAnalyzer.Core.Interfaces;
using ChangeRequestAnalyzer.Infrastructure.Settings;
using Microsoft.Extensions.Logging;

namespace ChangeRequestAnalyzer.Infrastructure.Services
{
    public sealed class AzureDevOpsService : IAzureDevOpsService
    {
        private readonly HttpClient _httpClient;
        private readonly AzureDevOpsOptions _options;
        private readonly Microsoft.Extensions.Logging.ILogger<AzureDevOpsService> _logger;

        public AzureDevOpsService(HttpClient httpClient, AzureDevOpsOptions options, Microsoft.Extensions.Logging.ILogger<AzureDevOpsService> logger)
        {
            _httpClient = httpClient;
            _options = options;
            _logger = logger;
            if (!string.IsNullOrEmpty(_options.PersonalAccessToken))
            {
                var token = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{_options.PersonalAccessToken}"));
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
            }
        }

        public async Task<IEnumerable<UserStory>> GetUserStoriesByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
        {
            var idList = ids?.Distinct().Where(i => i > 0).ToArray() ?? Array.Empty<int>();
            if (!idList.Any())
                return Array.Empty<UserStory>();

            // Build the ADO workitems API URL
            // e.g. https://dev.azure.com/{org}/_apis/wit/workitems?ids=1,2,3&api-version=7.0
            var baseUrl = _options.OrganizationUrl?.TrimEnd('/') ?? string.Empty;
            var url = $"{baseUrl}/_apis/wit/workitems?ids={string.Join(',', idList)}&api-version=7.0";

            try
            {
                _logger.LogInformation("Requesting work items {Ids} from Azure DevOps at {Url}", string.Join(',', idList), url);
                var response = await _httpClient.GetAsync(url, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Azure DevOps responded with non-success status code {StatusCode} for ids {Ids}", response.StatusCode, string.Join(',', idList));
                    return Array.Empty<UserStory>();
                }

                var payload = await response.Content.ReadAsStringAsync(cancellationToken);

                using var doc = JsonDocument.Parse(payload);
                if (doc.RootElement.TryGetProperty("value", out var items) && items.ValueKind == JsonValueKind.Array)
                {
                    var results = new List<UserStory>();
                    foreach (var item in items.EnumerateArray())
                    {
                        try
                        {
                            var id = item.GetProperty("id").GetInt32();
                            var fields = item.GetProperty("fields");
                            var title = fields.TryGetProperty("System.Title", out var t) ? t.GetString() ?? string.Empty : string.Empty;
                            var description = fields.TryGetProperty("System.Description", out var d) ? d.GetString() ?? string.Empty : string.Empty;

                            results.Add(new UserStory
                            {
                                StoryIdentifier = id.ToString(),
                                RequirementText = (!string.IsNullOrWhiteSpace(description)) ? description : title
                            });
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to parse work item element from Azure DevOps response");
                        }
                    }

                    return results;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request to Azure DevOps failed for url {Url}", url);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse Azure DevOps response JSON for url {Url}", url);
            }

            return Array.Empty<UserStory>();
        }
    }
}
