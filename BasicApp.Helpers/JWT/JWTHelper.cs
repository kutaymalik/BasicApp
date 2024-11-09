using BasicApp.Data.Domain;
using BasicApp.Data.UOW;
using BasicApp.Helpers.DtoModels;
using BasicApp.Helpers.Encryption;
using BasicApp.Helpers.SharedModels;
using BasicApp.Helpers.Token;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BasicApp.Helpers.JWT;

public class JwtHelper
{
    private readonly JwtConfig jwtConfig;

    public JwtHelper(IOptionsMonitor<JwtConfig> jwtConfig)
    {
        this.jwtConfig = jwtConfig.CurrentValue;
    }

    ////public async Task<Response<TokenResponse>> RefreshTokenAsync(string RefreshToken, CancellationToken cancellationToken)
    ////{
    ////    var user = unitOfWork.UserRepository.FirstOrDefault(x => x.RefreshToken == RefreshToken && x.RefreshTokenExpireDate > DateTime.Now);

    ////    if (user == null)
    ////    {
    ////        return new Response<TokenResponse>
    ////        {
    ////            isSuccessful = false,
    ////            StatusCode = System.Net.HttpStatusCode.Unauthorized,
    ////            Message = "Invalid user informations"
    ////        };
    ////    }
    ////    var token = Token(user);

    ////    TokenResponse tokenResponse = new()
    ////    {
    ////        Token = token,
    ////        ExpireDate = DateTime.Now.AddMinutes(jwtConfig.AccessTokenExpiration),
    ////        Email = user.Email,
    ////        RefreshToken = CreateRefreshToken(),
    ////        UserName = user.UserName,
    ////        Id = user.Id,
    ////        Role = user.Role,
    ////    };

    ////    user.RefreshToken = tokenResponse.RefreshToken;
    ////    user.RefreshTokenExpireDate = tokenResponse.ExpireDate;
    ////    unitOfWork.UserRepository.Update(user);
    ////    await unitOfWork.CompleteAsync(cancellationToken);

    ////    return new Response<TokenResponse>
    ////    {
    ////        Data = tokenResponse,
    ////        isSuccessful = true,
    ////        StatusCode = System.Net.HttpStatusCode.OK
    ////    };
    ////}

    public string Token(User user)
    {
        Claim[] claims = GetClaims(user);
        var secret = Encoding.ASCII.GetBytes(jwtConfig.Secret);

        // JWT token oluşturulurken, `iat` claim'inin doğru formatta eklenmesi gerekebilir
        var jwtToken = new JwtSecurityToken(
            jwtConfig.Issuer,
            jwtConfig.Audience,
            claims.Concat(new[] {
        new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            }).ToArray(),
            expires: DateTime.Now.AddMinutes(jwtConfig.AccessTokenExpiration),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256Signature)
        );

        string accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);

        return accessToken;
    }

    private Claim[] GetClaims(User user)
    {
        var claims = new[]
        {
            new Claim("Id", user.Id.ToString()),
            new Claim("Role", user.Role),
            new Claim("Email", user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("UserName", user.UserName),
        };

        return claims;
    }

    public string CreateRefreshToken()
    {
        return Guid.NewGuid().ToString();
    }

}
