using System.Security.Claims;
using Library_Managment.DTOs;
using Library_Managment.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Library_Managment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly DBContext _dbContext;
        private readonly ILogger<BookController> _logger;

        public BookController(DBContext dbContext, ILogger<BookController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [Authorize(Roles = "Student")]
        [HttpGet]
        public IActionResult GetBooks()
        {
            try
            {
                _logger.LogInformation("=== GetBooks Request ===");
                _logger.LogInformation($"User authenticated: {User.Identity.IsAuthenticated}");
                _logger.LogInformation($"User ID: {User.FindFirstValue(ClaimTypes.NameIdentifier)}");
                _logger.LogInformation($"User roles: {string.Join(", ", User.FindAll(ClaimTypes.Role).Select(c => c.Value))}");

                var books = _dbContext.Book
                    .Select(book => new GetBookDTO
                    {
                        Id = book.Id,
                        Title = book.Title,
                        Author = book.Author,
                        Price = book.Price,
                        ordered = book.Ordered,
                        bookCategoryId = book.BookCategoryId
                    })
                    .ToList();

                if (books.Any())
                {
                    _logger.LogInformation($"Returning {books.Count} books");
                    return Ok(books);
                }

                _logger.LogInformation("No books found");
                return Ok(new { message = "No books found", data = new List<GetBookDTO>() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting books");
                return StatusCode(500, new { message = "An error occurred while processing your request" });
            }
        }

        [Authorize]
        [HttpGet("test-auth")]
        public IActionResult TestAuth()
        {
            _logger.LogInformation("=== Test Auth Endpoint ===");

            var result = new
            {
                IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
                AuthenticationType = User.Identity?.AuthenticationType,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                Username = User.Identity?.Name,
                Email = User.FindFirstValue(ClaimTypes.Email),
                UserType = User.FindFirstValue("userType"),
                AccountStatus = User.FindFirstValue("accountStatus"),
                Roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList(),
                AllClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList(),
                AuthorizationHeader = Request.Headers
                    .Where(h => h.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                    .Select(h => new { h.Key, Value = h.Value.ToString() }).FirstOrDefault()
            };

            _logger.LogInformation("Test Auth Result: {Result}",
                System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                }));

            return Ok(result);
        }

        [HttpGet("test-no-auth")]
        public IActionResult TestNoAuth()
        {
            _logger.LogInformation("Test endpoint without authentication called");
            return Ok(new
            {
                message = "This endpoint works without authentication",
                timestamp = DateTime.Now,
                serverTime = DateTime.UtcNow
            });
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("admin-only")]
        public IActionResult AdminOnly()
        {
            return Ok(new
            {
                message = "This is admin-only content",
                user = User.Identity.Name,
                roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
            });
        }

        [Authorize(Roles = "Student,Admin")]
        [HttpGet("student-or-admin")]
        public IActionResult StudentOrAdmin()
        {
            return Ok(new
            {
                message = "This content is for students or admins",
                user = User.Identity.Name,
                roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
            });
        }


        [Authorize]
        [HttpGet("any-authenticated-user")]
        public IActionResult AnyAuthenticatedUser()
        {
            return Ok(new
            {
                message = "Any authenticated user can access this",
                user = User.Identity.Name,
                roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
            });
        }

        [HttpGet("verify-token")]
        public IActionResult VerifyToken()
        {
            var logger = _logger;
            var authHeader = Request.Headers["Authorization"].ToString();
            logger.LogInformation($"Raw Authorization Header: {authHeader}");

            if (string.IsNullOrEmpty(authHeader))
            {
                return Unauthorized(new { message = "No authorization header found" });
            }

            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized(new { message = "Authorization header must start with 'Bearer '" });
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { message = "Token is empty" });
            }

            return Ok(new
            {
                message = "Token format is valid",
                tokenLength = token.Length,
                tokenPrefix = token.Substring(0, Math.Min(20, token.Length)) + "...",
                isAuthenticated = User.Identity?.IsAuthenticated ?? false,
                authenticationType = User.Identity?.AuthenticationType,
                claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
            });
        }
    }
}