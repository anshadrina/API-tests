using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoTestApi.Controllers
{
    public class BlogService : IBlogService
    {
        private readonly Dictionary<Guid, Blog> _database;

        public BlogService()
        {
            _database = new Dictionary<Guid, Blog>();
        }

        public Task<Guid> Create(Blog blog)
        {
            var id = Guid.NewGuid();
            blog.Id = id;
            _database.Add(id, blog);
            return Task.FromResult(id);
        }

        public Task Delete(Guid id)
        {
            if (!_database.Remove(id))
            {
                throw new NotFoundException();
            }

            return Task.CompletedTask;
        }

        public Task<Blog> Get(Guid id)
        {
            var found = _database.TryGetValue(id, out var blog);
            if (!found)
            {
                throw new NotFoundException();
            }

            return Task.FromResult(blog);
        }

        public Task Update(Blog blog)
        {
            if (_database.ContainsKey(blog.Id.Value))
            {
                _database[blog.Id.Value] = blog;
            }
            else
            {
                throw new NotFoundException();
            }

            return Task.CompletedTask;
        }
    }
}
