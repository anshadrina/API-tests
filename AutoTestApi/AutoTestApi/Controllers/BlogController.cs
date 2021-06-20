using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AutoTestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        private readonly IBlogService _blogService;

        public BlogController(IBlogService blogService)
        {
            _blogService = blogService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Blog blog)
        {
            var id = await _blogService.Create(blog);
            return Ok(id);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Blog blog)
        {
            try
            {
                await _blogService.Update(blog);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }

            return Ok();
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _blogService.Delete(id);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }

            return Ok();
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<Blog>> Get(Guid id)
        {
            try
            {
                var blog = await _blogService.Get(id);
                return Ok(blog);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }
    }
}
