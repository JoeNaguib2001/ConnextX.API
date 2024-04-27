using ConnextX.API.Data.Dtos;
using ConnextX.API.Data.Models;
using ConnextX.API.Data.Models.DbContext;
using ConnextX.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConnextX.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "User")]
    public class LanguagesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public LanguagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync ()
        {
            var languages = await _context.Languages.OrderBy(L => L.Name).ToListAsync();
            return Ok(languages);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateLanguageDto dto)
        {
            var language = new Language
            {
                Name = dto.Name
            };
            await _context.Languages.AddAsync(language);
            await _context.SaveChangesAsync();
            return Ok(language);
        }

        [HttpPut ("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, CreateLanguageDto dto)
        {
            var language = await _context.Languages.FirstOrDefaultAsync(L => L.Id == id);
            if (language == null)
            {
                return NotFound($"No language with ID: {id} was found");
            }
            language.Name = dto.Name;
            await _context.SaveChangesAsync();
            return Ok(language);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var language = await _context.Languages.FirstOrDefaultAsync(L => L.Id == id);
            if (language == null)
            {
                return NotFound($"No language with ID: {id} was found");
            }
            _context.Languages.Remove(language);
            await _context.SaveChangesAsync();
            return Ok(language);
        }

        //[HttpGet("someaction")]
        //public  IActionResult SomeAction()
        //{
        //    var currentUser = _authManager.GetCurrentUserAsync();
        //    // Use currentUser as needed
        //    return null;
        //}


    }
}
