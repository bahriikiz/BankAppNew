using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBankAppServer.Application.Features.Auth.Commands.CreateUser;
using OnlineBankAppServer.Application.Features.Auth.Commands.DeleteUser;
using OnlineBankAppServer.Application.Features.Auth.Commands.Login;
using OnlineBankAppServer.Presentation.Abstraction;
using System.Security.Claims;

namespace OnlineBankAppServer.Presentation.Controller;

public sealed class AuthController : ApiController
{
    public AuthController(IMediator mediator) : base(mediator)
    {
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(new { Message = response });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);

        // Eğer Handler tarafında (Application katmanı) LoginResponse nesnesine 
        if (string.IsNullOrEmpty(response.Token))
        {
            return BadRequest(response);
        }

        return Ok(response);
        // Artık frontend 'res.firstName' ve 'res.lastName' olarak veriye erişebilecek.
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
}