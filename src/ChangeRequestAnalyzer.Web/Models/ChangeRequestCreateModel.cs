using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChangeRequestAnalyzer.Web.Models
{
    public sealed class ChangeRequestCreateModel
    {
        [Required]
        [MaxLength(250)]
        public string Title { get; set; } = string.Empty;

        // The uploaded change request document (.txt). The server will read this file
        // and use it to lookup referenced user stories in Azure DevOps.
        public Microsoft.AspNetCore.Http.IFormFile? ChangeRequestDocument { get; set; }
    }
}
