using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TodoApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IConfiguration _config;

    public AuthenticationController(IConfiguration config)
    {
        _config = config;
    }

    public record AuthenticationData(string? UserName, string? Password);
    public record UserData(int Id, string FirstName, string LastName, string UserName);

    [HttpPost("token")]
    [AllowAnonymous]
    public ActionResult<string> Authenticate([FromBody] AuthenticationData data)
    {
        UserData? user = ValidateCredentials(data);

        if (user is null)
        {
            return Unauthorized();
        }

        string token = GenerateToken(user);
        return Ok(token);
    }

    private UserData? ValidateCredentials(AuthenticationData data)
    {
        // <NOT_PRODUCTIN_CODE>
        // Replace this with a call to your authentication system
        if (CompareValues(data.UserName, "sammy") &&
            CompareValues(data.Password, "NAVA"))
        {
            return new UserData(1, "Sammy", "Nava", data.UserName!);
        }
        if (CompareValues(data.UserName, "dizzy") &&
            CompareValues(data.Password, "FLORES"))
        {
            return new UserData(2, "Dizzy", "Flores", data.UserName!);
        }
        // </NOT_PROEUCTION_CODE>

        return null;
    }

    private bool CompareValues(string? actual, string? expected)
    {
        return (actual is not null && actual.Equals(expected));
    }

    private string GenerateToken(UserData user)
    {
        // Get and validate configuration stings
        string? aSecretKey = _config.GetValue<string>("Authentication:SecretKey");
        string? aIssuer = _config.GetValue<string>("Authentication:Issuer");
        string? aAudience = _config.GetValue<string>("Authentication:Audience");

        // Return an empty token if any of the Authentication values are missing
        if (aSecretKey is null || aIssuer is null || aAudience is null) return string.Empty;

        var secretKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(aSecretKey));

        var signingCredentials = new SigningCredentials(secretKey,SecurityAlgorithms.HmacSha256);

        List<Claim> claims = new();
        claims.Add(new(JwtRegisteredClaimNames.Sub, user.Id.ToString()));
        claims.Add(new(JwtRegisteredClaimNames.UniqueName,user.UserName));
        claims.Add(new(JwtRegisteredClaimNames.GivenName, user.FirstName));
        claims.Add(new(JwtRegisteredClaimNames.FamilyName, user.LastName));

        var token = new JwtSecurityToken(
            aIssuer,
            aAudience,
            claims,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMinutes(60),
            signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}
