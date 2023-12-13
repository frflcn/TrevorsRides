using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TrevorsRidesServer.Pages
{
    public class AdminModel : PageModel
    {
        private readonly ILogger<AdminModel> _logger;
        private readonly IWebHostEnvironment _webhostenvironment;
        [FromQuery(Name="code")]
        private string? Code { get; set; }
        [FromQuery(Name = "error")]
        private string? Error { get; set; }
        private int? PageIndex { get; set; }

        public AdminModel(ILogger<AdminModel> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _webhostenvironment = webHostEnvironment;
        }
        public void OnGet()
        {
            if (Code == null && Error == null) return;

            if (Error != null) _logger.LogError($"Admin model: {Error}");

            if (Code != null)
            {
                Response.Redirect("https://www.trevorsrides.com/");    
            }
        }

    }
}
