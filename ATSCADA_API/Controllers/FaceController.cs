using ATSCADA_Library.Services.ApiService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace ATSCADA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FaceController : ControllerBase
    {
        private readonly FaceRecognitionService _faceService;


        public FaceController(FaceRecognitionService faceService)
        {
            _faceService = faceService;
        }

        [HttpPost("Search")]
        public async Task<IActionResult> Search([FromForm] IFormFile image)
        {
            if (image == null || image.Length == 0)
                return BadRequest(new { error = "No image provided" });

            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".jpg");
            try
            {
                using (var stream = new FileStream(tempPath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                var result = await _faceService.SearchFaceAsync(tempPath);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Server error: {ex.Message}" });
            }
            finally
            {
                if (System.IO.File.Exists(tempPath)) System.IO.File.Delete(tempPath);
            }
        }

        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromForm] IFormFile image, [FromForm] string name, [FromForm] string info)
        {
            if (image == null || image.Length == 0)
                return BadRequest(new { error = "No image provided" });
            if (string.IsNullOrEmpty(name))
                return BadRequest(new { error = "Name is required" });

            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".jpg");
            try
            {
                using (var stream = new FileStream(tempPath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                var result = await _faceService.AddFaceAsync(tempPath, name, info);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Server error: {ex.Message}" });
            }
            finally
            {
                if (System.IO.File.Exists(tempPath)) System.IO.File.Delete(tempPath);
            }
        }
    }
}
