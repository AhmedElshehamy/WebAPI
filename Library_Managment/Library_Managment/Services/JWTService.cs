using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Library_Managment.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Library_Managment.Services
{
    public class JWTService
    {
        private readonly string _key;
        private readonly int _duration;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<JWTService> _logger;

        public JWTService(IConfiguration configuration,
                        UserManager<ApplicationUser> userManager,
                        ILogger<JWTService> logger)
        {
            _configuration = configuration;
            _key = configuration["JWT:key"] ?? throw new ArgumentNullException("JWT:key is not configured");
            _duration = int.Parse(configuration["JWT:Duration"] ?? "1440"); // Default 24 hours
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<string> GenerateToken(ApplicationUser user)
        {
            try
            {
                _logger.LogInformation($"Generating token for user: {user.Email}");


                var keyBytes = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
                var signingCredentials = new SigningCredentials(keyBytes, SecurityAlgorithms.HmacSha256);


                var userRoles = await _userManager.GetRolesAsync(user);
                _logger.LogInformation($"User roles: {string.Join(", ", userRoles)}");

                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                    new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                    new Claim("firstName", user.FirstName ?? string.Empty),
                    new Claim("lastName", user.LastName ?? string.Empty),
                    new Claim("mobileNumber", user.PhoneNumber ?? string.Empty),
                    new Claim("userType", user.UserType.ToString()),
                    new Claim("accountStatus", user.AccountStatus.ToString()),
                    new Claim("createdOn", user.CreatedOn.ToString("yyyy-MM-dd HH:mm:ss")),
                };

                foreach (var role in userRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                    _logger.LogInformation($"Added role claim: {role}");
                }

                var now = DateTime.UtcNow;
                var expires = now.AddMinutes(_duration);

                var token = new JwtSecurityToken(
                    claims: claims,
                    expires: expires,
                    signingCredentials: signingCredentials
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                _logger.LogInformation($"Token generated successfully for user: {user.Email}");
                _logger.LogInformation($"Token expires at: {expires}");
                _logger.LogInformation($"Token (first 50 chars): {tokenString.Substring(0, Math.Min(50, tokenString.Length))}...");

                return tokenString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating token for user: {user.Email}");
                throw;
            }
        }
    }
}