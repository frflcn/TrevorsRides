using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Hosting;

namespace TrevorsResume.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IWebHostEnvironment _webhostenvironment;

        public IndexModel(ILogger<IndexModel> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _webhostenvironment = webHostEnvironment;   
        }

        public void OnGet()
        {

        }

        
        public IActionResult OnGetDownloadFile(string fileName)
        {
           
            string path = _webhostenvironment.WebRootPath + fileName;
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            return File(bytes, "application/octet-stream", fileName);
            
        }

    }
}