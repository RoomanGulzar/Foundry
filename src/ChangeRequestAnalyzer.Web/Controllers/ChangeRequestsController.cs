using System.Linq;
using System.Threading.Tasks;
using ChangeRequestAnalyzer.Core.Interfaces;
using ChangeRequestAnalyzer.Core.Entities;
using ChangeRequestAnalyzer.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChangeRequestAnalyzer.Web.Controllers
{
    public class ChangeRequestsController : Controller
    {
        private readonly IChangeRequestRepository _repository;
        private readonly IChangeRequestAnalyzer _analyzer;
        private readonly ChangeRequestAnalyzer.Core.Interfaces.IAzureDevOpsService _azureDevOpsService;
        private readonly Microsoft.Extensions.Logging.ILogger<ChangeRequestsController> _logger;

        public ChangeRequestsController(IChangeRequestRepository repository, IChangeRequestAnalyzer analyzer, ChangeRequestAnalyzer.Core.Interfaces.IAzureDevOpsService azureDevOpsService, Microsoft.Extensions.Logging.ILogger<ChangeRequestsController> logger)
        {
            _repository = repository;
            _analyzer = analyzer;
            _azureDevOpsService = azureDevOpsService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var changeRequests = await _repository.GetAllAsync();
            return View(changeRequests.Select(cr => new ChangeRequestDetailsViewModel
            {
                Id = cr.Id,
                Title = cr.Title,
                Description = cr.Description,
                Status = cr.Status.ToString(),
                CreatedAt = cr.CreatedAt,
                UserStories = cr.UserStories.Select(us => new UserStoryInputModel
                {
                    StoryIdentifier = us.StoryIdentifier,
                    RequirementText = us.RequirementText
                }).ToArray()
            }).ToArray());
        }

        public IActionResult Create()
        {
            return View(new ChangeRequestCreateModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChangeRequestCreateModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var changeRequest = new ChangeRequest
            {
                Title = model.Title
            };

            // Read uploaded txt file content if provided
            if (model.ChangeRequestDocument != null && model.ChangeRequestDocument.Length > 0)
            {
                try
                {
                    using var sr = new System.IO.StreamReader(model.ChangeRequestDocument.OpenReadStream());
                    var content = await sr.ReadToEndAsync();
                    changeRequest.DocumentFileName = model.ChangeRequestDocument.FileName;
                    changeRequest.DocumentContent = content;
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, "Failed to read uploaded change request document");
                    ModelState.AddModelError(string.Empty, "Failed to read the uploaded document.");
                    return View(model);
                }
            }

            await _repository.AddAsync(changeRequest);
            await _repository.SaveChangesAsync();

            // Extract numeric work item ids from document content and fetch from Azure DevOps
            var ids = new List<int>();
            if (!string.IsNullOrWhiteSpace(changeRequest.DocumentContent))
            {
                var matches = System.Text.RegularExpressions.Regex.Matches(changeRequest.DocumentContent, "\\b#?(\\d{1,7})\\b");
                foreach (System.Text.RegularExpressions.Match m in matches)
                {
                    if (int.TryParse(m.Groups[1].Value, out var id)) ids.Add(id);
                }
            }

            if (ids.Any())
            {
                try
                {
                    var userStories = await _azureDevOpsService.GetUserStoriesByIdsAsync(ids);
                    var stories = userStories.Select(us =>
                    {
                        us.ChangeRequestId = changeRequest.Id;
                        return us;
                    }).ToArray();

                    if (stories.Any())
                    {
                        await _repository.AddUserStoriesAsync(stories);
                        await _repository.SaveChangesAsync();
                    }
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, "Failed to fetch or store user stories from Azure DevOps for change request {ChangeRequestId}", changeRequest.Id);
                    // Continue to analysis even if user stories fail; analysis can still run on the document content
                }
            }

            await _analyzer.AnalyzeAsync(changeRequest.Id);

            return RedirectToAction(nameof(Details), new { id = changeRequest.Id });
        }

        public async Task<IActionResult> Details(int id)
        {
            var changeRequest = await _repository.GetWithDetailsAsync(id);
            if (changeRequest == null)
            {
                return NotFound();
            }

            var latestResult = changeRequest.AnalysisResults.OrderByDescending(r => r.AnalyzedAt).FirstOrDefault();

            var viewModel = new ChangeRequestDetailsViewModel
            {
                Id = changeRequest.Id,
                Title = changeRequest.Title,
                Description = changeRequest.Description,
                Status = changeRequest.Status.ToString(),
                CreatedAt = changeRequest.CreatedAt,
                UserStories = changeRequest.UserStories.Select(story => new UserStoryInputModel
                {
                    StoryIdentifier = story.StoryIdentifier,
                    RequirementText = story.RequirementText
                }).ToArray(),
                TargetFiles = latestResult?.TargetFiles ?? "[]",
                ConflictingFiles = latestResult?.ConflictingFiles ?? "[]",
                GeneratedPrompt = latestResult?.GeneratedPrompt ?? string.Empty,
                AnalyzedAt = latestResult?.AnalyzedAt
            };

            return View(viewModel);
        }
    }
}
