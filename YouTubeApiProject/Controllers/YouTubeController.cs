using Microsoft.AspNetCore.Mvc;
using YouTubeApiProject.Services;

namespace YouTubeApiProject.Controllers
{
    public class YouTubeController : Controller
    {
        private readonly YouTubeApiService _youtubeService;

        public YouTubeController(YouTubeApiService youtubeService)
        {
            _youtubeService = youtubeService;
        }

        // Display Search Page
        public IActionResult Index()
        {
            return View(new List<object>()); // Pass an empty list initially
        }

        // Handle the search query
        [HttpGet]
        public async Task<IActionResult> Search(string query, string sp, string token = "", string pg = "1")
        {
            try
            {
                var videos = await _youtubeService.SearchVideosAsync(query, sp, 10, token, int.Parse(pg));
                return View("Index", videos);
            }
            catch (Google.GoogleApiException ex)
            {
                if (ex.HttpStatusCode == System.Net.HttpStatusCode.Forbidden || ex.HttpStatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return Unauthorized(new { message = "Invalid API Key or insufficient permissions.", error = ex.Message });
                }
                return StatusCode(500, new { message = "YouTube API Error", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal Server Error", error = ex.Message });
            }
        }
    }
}
