using System;
using System.Threading.Tasks;

namespace AutoTestApi.Controllers
{
    public interface IBlogService
    {
        Task<Blog> Get(Guid id);
        Task<Guid> Create(Blog blog);
        Task Update(Blog blog);
        Task Delete(Guid id);
    }
}
