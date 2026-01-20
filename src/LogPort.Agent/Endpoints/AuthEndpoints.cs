using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using LogPort.Internal.Configuration;
using LogPort.Internal.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace LogPort.Agent.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app, LogPortConfig config)
    {
        app.MapPost("/api/auth/login", async (LoginRequest request) =>
            {
                if (request is null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                    return Results.BadRequest("Username and password are required");

                if (request.Username != config.AdminLogin || request.Password != config.AdminPassword)
                    return Results.Unauthorized();

                var token = GenerateJwtToken(request.Username, config.JwtSecret, config.JwtIssuer ?? "LogPort");

                return Results.Ok(new { token });
            })
            .WithTags("Auth")
            .WithName("AdminLogin")
            .WithSummary("Login endpoint for admin users. Returns JWT token.");
    }

    private static string GenerateJwtToken(string username, string secret, string issuer)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: issuer,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public record LoginRequest(string Username, string Password);
}