using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ChangeRequestAnalyzer.Core.Interfaces;
using ChangeRequestAnalyzer.Core.Models;
using ChangeRequestAnalyzer.Infrastructure.Settings;

namespace ChangeRequestAnalyzer.Infrastructure.Services
{
    public sealed class AzureFoundryOrchestrationService : IAzureFoundryOrchestrationService
    {
        private readonly HttpClient _httpClient;
        private readonly AzureFoundryOptions _options;

        public AzureFoundryOrchestrationService(HttpClient httpClient, AzureFoundryOptions options)
        {
            _httpClient = httpClient;
            _options = options;
        }

        public async Task<FoundryAnalysisResponse> AnalyzeChangeRequestAsync(string prompt, CancellationToken cancellationToken = default)
        {
            var request = CreateHttpRequest(prompt);
            // TODO: Replace with Azure AI Foundry SDK or direct HTTP call.
            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var rawPayload = await response.Content.ReadAsStringAsync(cancellationToken);
            return ParseFoundryPayload(rawPayload, prompt);
        }

        private HttpRequestMessage CreateHttpRequest(string prompt)
        {
            var payload = JsonSerializer.Serialize(new
            {
                deployment = _options.DeploymentName,
                prompt,
                parameters = new
                {
                    max_tokens = 800,
                    temperature = 0.2
                }
            });

            var request = new HttpRequestMessage(HttpMethod.Post, _options.Endpoint)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return request;
        }

        private static FoundryAnalysisResponse ParseFoundryPayload(string rawPayload, string prompt)
        {
            // This is a stubbed parsing hook. Replace the parsing rules with the Azure AI Foundry schema.
            var targetFiles = ExtractJsonArray(rawPayload, "targetFiles");
            var conflictingFiles = ExtractJsonArray(rawPayload, "conflictingFiles");
            var generatedPrompt = ExtractGeneratedPrompt(rawPayload) ?? prompt;

            return new FoundryAnalysisResponse
            {
                TargetFiles = targetFiles,
                ConflictingFiles = conflictingFiles,
                GeneratedPrompt = generatedPrompt
            };
        }

        private static IReadOnlyCollection<string> ExtractJsonArray(string payload, string key)
        {
            try
            {
                using var document = JsonDocument.Parse(payload);
                if (document.RootElement.TryGetProperty(key, out var property)
                    && property.ValueKind == JsonValueKind.Array)
                {
                    var items = new List<string>();
                    foreach (var element in property.EnumerateArray())
                    {
                        if (element.ValueKind == JsonValueKind.String)
                        {
                            items.Add(element.GetString()!);
                        }
                    }

                    return items;
                }
            }
            catch (JsonException)
            {
                // fall back to a safe default for stubbed payloads
            }

            return Array.Empty<string>();
        }

        private static string? ExtractGeneratedPrompt(string payload)
        {
            try
            {
                using var document = JsonDocument.Parse(payload);
                if (document.RootElement.TryGetProperty("generatedPrompt", out var property)
                    && property.ValueKind == JsonValueKind.String)
                {
                    return property.GetString();
                }
            }
            catch (JsonException)
            {
            }

            return null;
        }
    }
}
