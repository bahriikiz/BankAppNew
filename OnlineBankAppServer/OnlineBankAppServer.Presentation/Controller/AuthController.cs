using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // [Authorize] için gerekli
using OnlineBankAppServer.Application.Features.Auth.Commands.CreateUser;
using OnlineBankAppServer.Application.Features.Auth.Commands.Login;
using OnlineBankAppServer.Application.Features.Auth.Commands.DeleteUser; // EKSİK OLAN BUYDU
using OnlineBankAppServer.Presentation.Abstraction;
using System.Security.Claims; // ClaimTypes için

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
        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        if (string.IsNullOrEmpty(response.Token))
        {
            return BadRequest(response);
        }
        return Ok(response);
    }

    [Authorize] // Sadece giriş yapmış kullanıcılar çağırabilir
    [HttpDelete("delete-my-profile")]
    public async Task<IActionResult> DeleteMyProfile(CancellationToken cancellationToken)
    {
        // 1. Token'dan User ID'yi güvenli şekilde alıyoruz
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")
                          ?? User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
                          ?? User.Claims.FirstOrDefault(c => c.Type == "sub");

        if (userIdClaim == null)
        {
            return StatusCode(401, new { Message = "Kimlik doğrulanamadı. Lütfen tekrar giriş yapın." });
        }

        int userId = int.Parse(userIdClaim.Value);

        // 2. Silme komutunu gönderiyoruz
        var response = await _mediator.Send(new DeleteUserCommand(userId), cancellationToken);

        // 3. Result nesnesine göre cevap dönüyoruz
        if (!response.IsSuccess)
        {
            return BadRequest(new { Message = response.ErrorMessage });
        }

        return Ok(new { Message = response.Data });
    }
}