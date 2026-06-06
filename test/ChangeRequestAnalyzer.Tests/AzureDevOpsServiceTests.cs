using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ChangeRequestAnalyzer.Infrastructure.Services;
using ChangeRequestAnalyzer.Infrastructure.Settings;
using ChangeRequestAnalyzer.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ChangeRequestAnalyzer.Tests
{
    public class AzureDevOpsServiceTests
    {
        [Fact]
        public async Task GetUserStoriesByIdsAsync_Parses_Response()
        {
            var json = @"{ 'value': [ { 'id': 123, 'fields': { 'System.Title': 'Title 1', 'System.Description': 'Desc 1' } }, { 'id': 456, 'fields': { 'System.Title': 'Title 2' } } ] }".Replace('\'', '"');

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var handler = new FakeHttpMessageHandler(response);
            var client = new HttpClient(handler);
            var options = new AzureDevOpsOptions { OrganizationUrl = "https://dev.azure.com/fakeOrg" };
            var service = new AzureDevOpsService(client, options, new NullLogger<AzureDevOpsService>());

            var results = await service.GetUserStoriesByIdsAsync(new[] { 123, 456 });

            Assert.NotNull(results);
            var arr = results as System.Collections.Generic.List<ChangeRequestAnalyzer.Core.Entities.UserStory> ?? new System.Collections.Generic.List<ChangeRequestAnalyzer.Core.Entities.UserStory>(results);
            Assert.Equal(2, arr.Count);
            Assert.Equal("123", arr[0].StoryIdentifier);
            Assert.Contains("Desc", arr[0].RequirementText);
            Assert.Equal("456", arr[1].StoryIdentifier);
        }
    }
}
