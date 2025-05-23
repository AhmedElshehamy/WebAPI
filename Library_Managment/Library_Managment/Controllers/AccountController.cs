using Library_Managment.DTOs;
using Library_Managment.Models;
using Library_Managment.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Library_Managment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        public UserManager<ApplicationUser> UserManager { get; }
        public RoleManager<IdentityRole> RoleManager { get; }
        public EmailService EmailService { get; }
        public JWTService JWTService { get; }

        public AccountController(UserManager<ApplicationUser> _userManager, RoleManager<IdentityRole> roleManager, EmailService emailService, JWTService jWTService)
        {
            UserManager = _userManager;
            RoleManager = roleManager;
            EmailService = emailService;
            JWTService = jWTService;
        }

        [HttpPost("/api/Account/Register")]
        public async Task<IActionResult> Register(UserRegisterDTO userRegister)
        {

            if (ModelState.IsValid)
            {


                ApplicationUser userApplication = new ApplicationUser()
                {
                    UserName = userRegister.firstName + "" + userRegister.lastName,
                    FirstName = userRegister.firstName,
                    LastName = userRegister.lastName,
                    Email = userRegister.email,
                    PhoneNumber = userRegister.mobilePhone,
                    AccountStatus = AccountStatus.UNAPROOVED,
                    UserType = UserType.STUDENT,
                    PasswordHash = userRegister.password,
                    CreatedOn = DateTime.UtcNow
                };


                IdentityResult result = await UserManager.CreateAsync(userApplication, userRegister.password);

                if (!result.Succeeded)
                {
                    foreach (var item in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, item.Description);
                    }
                    return BadRequest(ModelState);
                }

                if (!await RoleManager.RoleExistsAsync("Student"))
                {
                    await RoleManager.CreateAsync(new IdentityRole("Student"));
                }

                var roleResult = await UserManager.AddToRoleAsync(userApplication, "Student");
                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return BadRequest(ModelState);
                }
                const string subject = "Account Created";
                var body = $"""
                <html>
                    <body>
                        <h1>Hello, {userApplication.FirstName} {userApplication.LastName}</h1>
                        <h2>
                            Your account has been created and we have sent approval request to admin. 
                            Once the request is approved by admin you will receive email, and you will be
                            able to login in to your account.
                        </h2>
                        <h3>Thanks</h3>
                    </body>
                </html>
            """;

                EmailService.sendEmail(userApplication.Email, subject, body);

            }
            return Ok(new { Message = "User registered successfully ,Thank you for registering Your account has been sent for aprooval , Once it is aprooved, you will get an email." });

        }
        [HttpPost("/api/Account/Login")]

        public async Task<IActionResult> Login(LoginUserDTO loginUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Invalid request data" });
            }
            ApplicationUser userFromDB = await UserManager.FindByEmailAsync(loginUser.Email);

            if (userFromDB == null)
            {
                return Unauthorized(new { Message = "Invalid email or password" });
            }
            if (userFromDB.AccountStatus != AccountStatus.ACTIVE)
            {
                return Unauthorized(new
                {
                    Message = "Account not active",
                    Status = userFromDB.AccountStatus.ToString()
                });
            }
            bool isPasswordValid = await UserManager.CheckPasswordAsync(userFromDB, loginUser.Password);
            if (!isPasswordValid)
            {
                return Unauthorized(new { Message = "Invalid email or password" });
            }
            var token = JWTService.GenerateToken(userFromDB);


            return Ok(new
            {
                Token = token,
                UserId = userFromDB.Id,
            });
        }
    }
}
