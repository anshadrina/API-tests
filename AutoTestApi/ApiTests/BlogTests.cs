using NUnit.Framework;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApiTests
{
    [TestFixture]
    public class BlogTests
    {
        private const string BaseUrl = "https://localhost:5001";
        public HttpClient _httpClient;
        private JsonSerializerOptions jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public async Task<(Guid id, HttpStatusCode code)> CreateBlog(Blog blog)
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "api/Blog");
            var serializedBlog = JsonSerializer.Serialize(blog);
            requestMessage.Content = new StringContent(serializedBlog, Encoding.UTF8, MediaTypeNames.Application.Json);
            using var responseMessage = await _httpClient.SendAsync(requestMessage);

            if (!responseMessage.IsSuccessStatusCode)
            {
                return (Guid.Empty, responseMessage.StatusCode);
            }
            var responseContent = await responseMessage.Content.ReadAsStringAsync();
            var id = JsonSerializer.Deserialize<Guid>(responseContent);
            return (id, responseMessage.StatusCode);
        }

        public async Task<(Blog blog, HttpStatusCode code)> GetBlog(Guid id)
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"api/Blog/{id}");
            using var responseMessage = await _httpClient.SendAsync(requestMessage);

            if (!responseMessage.IsSuccessStatusCode)
            {
                return (null, responseMessage.StatusCode);
            }
            var responseContent = await responseMessage.Content.ReadAsStringAsync();
            var blog = JsonSerializer.Deserialize<Blog>(responseContent, jsonOptions);
            return (blog, responseMessage.StatusCode);
        }

        public bool CompareBlogs(Blog blog1, Blog blog2)
        {
            return blog1.Author == blog2.Author &&
                blog1.CreatedDate == blog2.CreatedDate &&
                blog1.Text == blog2.Text &&
                blog1.UpdatedDate == blog2.UpdatedDate;
        }

        [OneTimeSetUp]
        public void Init()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        }

        [SetUp]
        public void Setup()
        {

        }

        [TestCase("author1", "text1")]
        [TestCase("12312!WDw;,q", "text1")]
        [TestCase("waroifjaorilszrj fao;sirhcmgao;sizrjgmaxozirjga;o,xrizjgvoiszrjlvalizrjvlzisrjmvlzirjvz;lixrjvmz;lirjvmzl;z", "text1")]
        public async Task Create_Ok(string author, string text)
        {
            var blog = new Blog
            {
                Author = author,
                CreatedDate = DateTime.UtcNow,
                Text = text,
                UpdatedDate = DateTime.UtcNow
            };
            var response = await CreateBlog(blog);

            Assert.AreEqual(HttpStatusCode.OK, response.code);
            Assert.AreNotEqual(Guid.Empty, response.id);
        }

        [Test]
        public async Task Create_BadRequest()
        {
            var blog = new Blog
            {
                Author = "test",
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            var response = await CreateBlog(blog);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.code);
        }

        [Test]
        public async Task Update_Ok()
        {
            var blog = new Blog
            {
                Author = "author1",
                CreatedDate = DateTime.UtcNow,
                Text = "text 1",
                UpdatedDate = DateTime.UtcNow
            };

            var response = await CreateBlog(blog);

            using var requestMessage = new HttpRequestMessage(HttpMethod.Put, "api/Blog");

            var blogUpdate = new Blog
            {
                Id = response.id,
                Author = "author1",
                CreatedDate = DateTime.UtcNow,
                Text = "text 2",
                UpdatedDate = DateTime.UtcNow
            };

            var serializedBlog = JsonSerializer.Serialize(blogUpdate);

            using var content = new StringContent(serializedBlog, Encoding.UTF8, MediaTypeNames.Application.Json);
            requestMessage.Content = content;

            using var responseMessage = await _httpClient.SendAsync(requestMessage);

            Assert.AreEqual(HttpStatusCode.OK, responseMessage.StatusCode);

            var responseUpdatedBlog = await GetBlog(response.id);
            var updatedBlog = responseUpdatedBlog.blog;

            Assert.AreEqual(blogUpdate.Text, updatedBlog.Text);
        }

        [Test]
        public async Task Update_BadRequest()
        {
            var blog = new Blog
            {
                Author = "author1",
                CreatedDate = DateTime.UtcNow,
                Text = "text 1",
                UpdatedDate = DateTime.UtcNow
            };

            var response = await CreateBlog(blog);

            using var requestMessage = new HttpRequestMessage(HttpMethod.Put, "api/Blog");

            blog.Text = "";
            blog.Id = response.id;
            var serializedBlog = JsonSerializer.Serialize(blog);
            using var requestContent = new StringContent(serializedBlog, Encoding.UTF8, MediaTypeNames.Application.Json);
            requestMessage.Content = requestContent;
            using var responseMessage = await _httpClient.SendAsync(requestMessage);

            Assert.AreEqual(HttpStatusCode.BadRequest, responseMessage.StatusCode);

            var responseUpdatedBlog = await GetBlog(response.id);
            var updatedBlog = responseUpdatedBlog.blog;

            Assert.AreEqual("text 1", updatedBlog.Text);
        }

        [Test]
        public async Task Update_NotFound()
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"api/Blog");
            var blog = new Blog
            {
                Id = Guid.Empty,
                Author = "author1",
                CreatedDate = DateTime.UtcNow,
                Text = "text 1",
                UpdatedDate = DateTime.UtcNow
            };
            var serialisedBlog = JsonSerializer.Serialize(blog);
            var requestContent = new StringContent(serialisedBlog, Encoding.UTF8, MediaTypeNames.Application.Json);
            request.Content = requestContent;
            using var response = await _httpClient.SendAsync(request);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public async Task Delete_Ok()
        {
            var blog = new Blog
            {
                Author = "author1",
                CreatedDate = DateTime.UtcNow,
                Text = "text 1",
                UpdatedDate = DateTime.UtcNow
            };

            var response = await CreateBlog(blog);

            using var requestMessage = new HttpRequestMessage(HttpMethod.Delete, $"api/Blog/{response.id}");
            using var responseMessage = await _httpClient.SendAsync(requestMessage);

            Assert.AreEqual(HttpStatusCode.OK, responseMessage.StatusCode);

            var responseUpdate = await GetBlog(response.id);

            Assert.AreEqual(null, responseUpdate.blog);
        }

        [Test]
        public async Task Delete_BadRequest()
        {
            var id = "erbe";
            using var requestMessage = new HttpRequestMessage(HttpMethod.Delete, $"api/Blog/{id}");
            using var responseMessage = await _httpClient.SendAsync(requestMessage);

            Assert.AreEqual(HttpStatusCode.BadRequest, responseMessage.StatusCode);
        }

        [Test]
        public async Task Delete_NotFound()
        {
            Guid guid = Guid.NewGuid();
            using var requestMessage = new HttpRequestMessage(HttpMethod.Delete, $"api/Blog/{guid}");
            using var responseMessage = await _httpClient.SendAsync(requestMessage);

            Assert.AreEqual(HttpStatusCode.NotFound, responseMessage.StatusCode);
        }

        [Test]
        public async Task Get_Ok()
        {
            var blog = new Blog
            {
                Author = "author1",
                CreatedDate = DateTime.UtcNow,
                Text = "text 1",
                UpdatedDate = DateTime.UtcNow
            };

            var response = await CreateBlog(blog);

            var responseResult = await GetBlog(response.id);

            Assert.AreEqual(HttpStatusCode.OK, responseResult.code);

            Assert.IsTrue(CompareBlogs(blog, responseResult.blog));
        }

        [Test]
        public async Task Get_BadRequest()
        {
            var id = "reg";
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/Blog/{id}");
            using var response = await _httpClient.SendAsync(request);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task Get_NotFound()
        {
            Guid id = Guid.NewGuid();
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/Blog/{id}");
            using var response = await _httpClient.SendAsync(request);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

    }
}