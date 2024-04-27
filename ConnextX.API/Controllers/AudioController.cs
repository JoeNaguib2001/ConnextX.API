using ConnextX.API.Contracts;
using ConnextX.API.Data.Models.DbContext;
using ConnextX.API.Repositories;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConnextX.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileUploadController : ControllerBase
    {
        private readonly IAuthManager _authManager;
        private readonly ApplicationDbContext _dbContext;
        public FileUploadController(IAuthManager authManager, ApplicationDbContext dbContext)
        {
                _authManager = authManager;
                _dbContext = dbContext;
        }

        [HttpPost("dubingprocess")]
        public IActionResult ProcessFile(IFormFile file ,string Token )
        {
            try
            {
                var userName = _authManager.VerifyJwt(Token);
                var userId = _dbContext.Users.Where(x => x.UserName == userName).FirstOrDefault();
                if (userId == null) 
                {
                    return BadRequest("Invalid Token Please Re-Sign");
                }
                //var currentUserSubscription = _dbContext.Subscriptions.OrderByDescending(X => X.StartDate).
                //                        Where(x => x.UserId == userId.Id).FirstOrDefault();
                //if(currentUserSubscription == null)
                //{
                //    return BadRequest("Please Subscribe to a Plan");
                //}

                // Check if file is null or empty
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded.");
                }

                //if (file.Length > currentUserSubscription.Quota)
                //{
                //    return BadRequest("File size exceeds your quota. Please upgrade your subscription.");
                //}
                // Process the file
                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);
                    // Here, you can perform any processing on the file data
                    // For demonstration purposes, let's just return the file name
                    var fileName = Path.GetFileName(file.FileName);
                    return Ok($"File '{fileName}' uploaded and processed successfully.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
