using System.Text;
using Library_Managment.Models;
using Library_Managment.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Library_Managment
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            builder.Services.AddDbContext<DBContext>(op =>
            {
                op.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            builder.Services.AddOpenApi();

            builder.Services.AddScoped<EmailService>();
            builder.Services.AddScoped<JWTService>();

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<DBContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularDevClient",
                    builder => builder
                        .WithOrigins("http://localhost:4200")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials());
            });

            var jwtKey = builder.Configuration["JWT:key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT key is not configured in appsettings.json");
            }

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogError($"JWT Authentication failed: {context.Exception.Message}");
                        logger.LogError($"Exception details: {context.Exception}");
                        logger.LogError($"Token: {context.Request.Headers["Authorization"]}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogInformation($"JWT Token validated successfully for: {context.Principal?.Identity?.Name}");
                        logger.LogInformation($"Claims: {string.Join(", ", context.Principal?.Claims.Select(c => $"{c.Type}: {c.Value}"))}");
                        logger.LogInformation($"Token: {context.Request.Headers["Authorization"]}");
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        var authHeader = context.Request.Headers["Authorization"].ToString();
                        logger.LogInformation($"Authorization Header: {authHeader}");
                        
                        if (string.IsNullOrEmpty(authHeader))
                        {
                            logger.LogWarning("Authorization header is empty");
                            return Task.CompletedTask;
                        }

                        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            logger.LogWarning("Authorization header does not start with 'Bearer '");
                            return Task.CompletedTask;
                        }

                        var token = authHeader.Substring("Bearer ".Length).Trim();
                        if (string.IsNullOrEmpty(token))
                        {
                            logger.LogWarning("Token is empty after removing 'Bearer ' prefix");
                            return Task.CompletedTask;
                        }

                        context.Token = token;
                        logger.LogInformation($"Extracted token: {token.Substring(0, Math.Min(50, token.Length))}...");
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogWarning($"JWT Challenge: {context.Error}, {context.ErrorDescription}");
                        logger.LogWarning($"Token: {context.Request.Headers["Authorization"]}");
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddAuthorization();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwaggerUI(op => op.SwaggerEndpoint("/openapi/v1.json", "v1"));
            }

            // Add detailed logging for debugging
            app.Use(async (context, next) =>
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation($"Request Path: {context.Request.Path}");
                logger.LogInformation($"Authorization Header: {context.Request.Headers["Authorization"]}");
                await next();
            });

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("AllowAngularDevClient");

            // Authentication and Authorization middleware
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.Run();
        }
    }
}