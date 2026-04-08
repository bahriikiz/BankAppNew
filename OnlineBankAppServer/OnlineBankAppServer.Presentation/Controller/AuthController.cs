using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineBankAppServer.Application.Features.Accounts.Commands.ChangePassword;
using OnlineBankAppServer.Application.Features.Accounts.Commands.UpdateProfile;
using OnlineBankAppServer.Application.Features.Auth.Commands.CreateUser;
using OnlineBankAppServer.Application.Features.Auth.Commands.DeleteUser;
using OnlineBankAppServer.Application.Features.Auth.Commands.Login;
using OnlineBankAppServer.Application.Features.Auth.Queries.GetMyProfile;
using OnlineBankAppServer.Presentation.Abstraction;
using System.Security.Claims;

namespace OnlineBankAppServer.Presentation.Controller;

public sealed class AuthController(IMediator mediator) : ApiController(mediator)
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(new { Message = response });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);


        // güvenli cookie ayarlarıyla token'ı cookie'ye yazıyoruz
        Response.Cookies.Append("AccessToken", response.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddHours(1)
        });

        return Ok(response); // Artık response.Token Angular'da localStorage'a yazılmayacak!
    }

    [Authorize]
    [HttpGet("logout")]
    public async Task<IActionResult> Logout([FromServices] OnlineBankAppServer.Persistance.AppDbContext context)
    {
        // token içinden User ID'yi almaya çalışıyoruz (birden fazla claim türünü kontrol ediyoruz)
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
                          ?? User.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
                          ?? User.Claims.FirstOrDefault(c => c.Type == "sub")
                          ?? User.Claims.FirstOrDefault(c => c.Type == "UserId");

        // eğer token'dan ID'yi alamadıysak çıkış işlemini yarım bırakıyoruz (DB güncellenmiyor)
        if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
        {
            return BadRequest(new { Message = "Token içinden kullanıcı kimliği (ID) okunamadı! Çıkış işlemi yarım kaldı, DB güncellenmedi." });
        }

        //  ID'yi başarıyla bulduysa veritabanında damgayı değiştiriyoruz
        if (int.TryParse(userIdClaim.Value, out int userId))
        {
            var user = await context.Users.FindAsync(userId);
            if (user != null)
            {
                // Güvenlik damgasını değiştirerek tüm aktif token'ların geçersiz olmasını sağlıyoruz
                user.SecurityStamp = Guid.NewGuid();
                user.RefreshToken = null;
                await context.SaveChangesAsync();
            }
            else
            {
                return BadRequest(new { Message = "Kullanıcı veritabanında bulunamadı!" });
            }
        }

        // Çerezleri temizliyoruz
        Response.Cookies.Delete("AccessToken", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None
        });

        return Ok(new { Message = "Güvenli çıkış yapıldı ve damga başarıyla güncellendi." });
    }

    [Authorize]
    [HttpDelete("delete-my-profile")]
    public async Task<IActionResult> DeleteMyProfile(CancellationToken cancellationToken)
    {
        // 1. Token'dan User ID'yi alıyoruz
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")
                          ?? User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
                          ?? User.Claims.FirstOrDefault(c => c.Type == "sub");

        if (userIdClaim == null)
        {
            return StatusCode(401, new { Message = "Kimlik doğrulanamadı. Lütfen tekrar giriş yapın." });
        }

        if (!int.TryParse(userIdClaim.Value, out int userId))
        {
            return BadRequest(new { Message = "Geçersiz kullanıcı kimliği." });
        }

        // 2. Silme komutunu gönderiyoruz
        var response = await _mediator.Send(new DeleteUserCommand(userId), cancellationToken);

        // 3. Başarı durumuna göre cevap dönüyoruz
        if (!response.IsSuccess)
        {
            return BadRequest(new { Message = response.ErrorMessage });
        }

        return Ok(new { Message = "Profiliniz başarıyla silindi." });
    }

    [Authorize]
    [HttpGet("my-profile")]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetMyProfileQuery(), cancellationToken);
        return Ok(response);
    }

    [Authorize]
    [HttpPut("update-profile")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(new { Message = response });
    }

    [Authorize]
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _mediator.Send(request, cancellationToken);
            return Ok(new { Message = response });
        }
        catch (Exception ex)
        {
            return BadRequest(new { ex.Message });
        }
    }
}