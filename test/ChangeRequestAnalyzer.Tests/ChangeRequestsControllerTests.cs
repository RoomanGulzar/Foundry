using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChangeRequestAnalyzer.Core.Entities;
using ChangeRequestAnalyzer.Core.Interfaces;
using ChangeRequestAnalyzer.Web.Controllers;
using ChangeRequestAnalyzer.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace ChangeRequestAnalyzer.Tests
{
    public class ChangeRequestsControllerTests
    {
        [Fact]
        public async Task Create_WithDocument_CallsAzureDevOpsAndAnalyzes()
        {
            var repoMock = new Mock<IChangeRequestRepository>();
            repoMock.Setup(r => r.AddAsync(It.IsAny<ChangeRequest>(), default)).Returns(Task.CompletedTask)
                .Callback<ChangeRequest, System.Threading.CancellationToken>((cr, ct) => { cr.Id = 1; });
            repoMock.Setup(r => r.SaveChangesAsync(default)).Returns(Task.CompletedTask);

            var adoMock = new Mock<IAzureDevOpsService>();
            adoMock.Setup(a => a.GetUserStoriesByIdsAsync(It.IsAny<IEnumerable<int>>(), default))
                .ReturnsAsync(new[] { new UserStory { StoryIdentifier = "123", RequirementText = "req" } });

            var analyzerMock = new Mock<IChangeRequestAnalyzer>();
            analyzerMock.Setup(a => a.AnalyzeAsync(It.IsAny<int>(), default)).ReturnsAsync(new AnalysisResult { Id = 1, ChangeRequestId = 1 });

            var controller = new ChangeRequestsController(repoMock.Object, analyzerMock.Object, adoMock.Object, new NullLogger<ChangeRequestsController>());

            var model = new ChangeRequestCreateModel { Title = "T" };
            var content = "This references work item #123";
            var bytes = Encoding.UTF8.GetBytes(content);
            var file = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "ChangeRequestDocument", "cr.txt") { Headers = new HeaderDictionary(), ContentType = "text/plain" };
            model.ChangeRequestDocument = file;

            var result = await controller.Create(model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirect.ActionName);
            repoMock.Verify(r => r.AddUserStoriesAsync(It.IsAny<IEnumerable<UserStory>>(), default), Times.Once);
            analyzerMock.Verify(a => a.AnalyzeAsync(1, default), Times.Once);
        }
    }
}
