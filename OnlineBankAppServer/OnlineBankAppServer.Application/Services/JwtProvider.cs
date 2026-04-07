using Microsoft.IdentityModel.Tokens;
using OnlineBankAppServer.Application.Abstractions;
using OnlineBankAppServer.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace OnlineBankAppServer.Application.Services;

public sealed class JwtProvider : IJwtProvider
{
    // Bu RSA anahtarını sistem bir kez oluşturup hep aynısını kullanmalı.

    private static readonly RSA RsaKey = RSA.Create(2048);

    // Public Key'i dışarıya açıyoruz ki Program.cs doğrulama yapabilsin.
    public static RsaSecurityKey GetPublicKey() => new(RsaKey);

    public string CreateToken(User user)
    {
        var claims = new Claim[]
        {
            new (JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new (JwtRegisteredClaimNames.Email, user.Email),
            new ("fullName", $"{user.FirstName} {user.LastName}"),
            new (ClaimTypes.Role, user.Role),
            new ("SecurityStamp" , user.SecurityStamp.ToString())
        };
        // RS256 imzası için RSA anahtarı ve algoritma bilgisi ile imzalama bilgisi oluşturuyoruz.
        var credentials = new SigningCredentials(
            new RsaSecurityKey(RsaKey),
            SecurityAlgorithms.RsaSha256); // RS256 İmzası

        var tokenDescriptor = new JwtSecurityToken(
            issuer: "OnlineBankApp",
            audience: "OnlineBankAppUsers",
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}