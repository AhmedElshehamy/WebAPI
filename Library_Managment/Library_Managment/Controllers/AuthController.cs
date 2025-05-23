using Library_Managment.Services;
using Microsoft.AspNetCore.Mvc;

namespace Library_Managment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JWTService jwtService;
        private readonly ILogger<AuthController> logger;

        public AuthController(JWTService jwtService, ILogger<AuthController> logger)
        {
            this.jwtService = jwtService;
            this.logger = logger;
        }

        //[HttpPost("validate-token")]
        //public IActionResult ValidateToken()
        //{
        //    var authHeader = Request.Headers["Authorization"].FirstOrDefault();

        //    if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        //    {
        //        return BadRequest(new { error = "Missing or invalid Authorization header" });
        //    }

        //    var token = authHeader.Substring("Bearer ".Length).Trim();

        //    try
        //    {
        //        var handler = new JwtSecurityTokenHandler();
        //        var jsonToken = handler.ReadJwtToken(token);
        //        var isValid = jwtService.ValidateToken(token);

        //        return Ok(new
        //        {
        //            isValid = isValid,
        //            expires = jsonToken.ValidTo,
        //            issuer = jsonToken.Issuer,
        //            audience = jsonToken.Audiences.FirstOrDefault(),
        //            currentTime = DateTime.UtcNow,
        //            isExpired = jsonToken.ValidTo < DateTime.UtcNow,
        //            claims = jsonToken.Claims.Select(c => new { c.Type, c.Value })
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError($"Token validation error: {ex.Message}");
        //        return BadRequest(new { error = ex.Message });
        //    }
        //}
    }
}
