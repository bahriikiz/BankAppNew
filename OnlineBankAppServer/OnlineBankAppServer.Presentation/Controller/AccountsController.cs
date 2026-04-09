using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBankAppServer.Application.Features.Accounts.Commands.CreateAccount;
using OnlineBankAppServer.Application.Features.Accounts.Commands.DeleteAccount;
using OnlineBankAppServer.Application.Features.Accounts.Commands.SyncVakifbankAccounts;
using OnlineBankAppServer.Application.Features.Accounts.Queries.GetById;
using OnlineBankAppServer.Application.Features.Accounts.Queries.GetByUserId;
using OnlineBankAppServer.Application.Features.Accounts.Queries.GetStatementPdf;
using OnlineBankAppServer.Domain.Entities;
using OnlineBankAppServer.Persistance;
using OnlineBankAppServer.Presentation.Abstraction;

namespace OnlineBankAppServer.Presentation.Controller;

[Authorize]
public sealed class AccountsController(IMediator mediator, AppDbContext context) : ApiController(mediator)
{
    [HttpPost("create-account")]
    public async Task<IActionResult> Create([FromBody] CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(new { Message = response });
    }

    [HttpGet("get-my-accounts")]
    public async Task<IActionResult> GetMyAccounts(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetAccountByUserIdQuery(), cancellationToken);
        return Ok(response);
    }

    [HttpGet("get-by-id/{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")
                            ?? User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)
                            ?? User.Claims.FirstOrDefault(c => c.Type == "sub");

        if (userIdClaim == null)
        {
            return StatusCode(401, new { Message = "Kullanıcı kimliği doğrulanamadı. Lütfen tekrar giriş yapın." });
        }

        int userId = int.Parse(userIdClaim.Value);

        var response = await _mediator.Send(new GetAccountByIdQuery(id, userId), cancellationToken);

        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAccount(int id, CancellationToken cancellationToken)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")
                          ?? User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)
                          ?? User.Claims.FirstOrDefault(c => c.Type == "sub");

        if (userIdClaim == null) return StatusCode(401, new { Message = "Kullanıcı doğrulanamadı." });

        int userId = int.Parse(userIdClaim.Value);

        var response = await _mediator.Send(new DeleteAccountCommand(id, userId), cancellationToken);

        if (!response.IsSuccess)
        {
            return BadRequest(new { Message = response.ErrorMessage });
        }

        return Ok(new { Message = response.Data });
    }

    [HttpGet("{id}/statement")]
    public async Task<IActionResult> GetAccountStatementPdf(int id, CancellationToken cancellationToken)
    {
        var pdfBytes = await _mediator.Send(new GetAccountStatementPdfQuery(id), cancellationToken);

        return File(pdfBytes, "application/pdf", $"Hesap_Ekstresi_{id}_{DateTime.Now:yyyyMMdd}.pdf");
    }

    [HttpPost("sync-vakifbank")]
    public async Task<IActionResult> SyncVakifbank([FromBody] SyncVakifbankAccountsCommand request, CancellationToken cancellationToken)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")
                          ?? User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)
                          ?? User.Claims.FirstOrDefault(c => c.Type == "sub");

        if (userIdClaim == null) return StatusCode(401, new { Message = "Kullanıcı doğrulanamadı." });

        if (!int.TryParse(userIdClaim.Value, out int userId))
        {
            return BadRequest(new { Message = "Geçersiz kullanıcı kimliği." });
        }

        request.UserId = userId;

        var response = await _mediator.Send(request, cancellationToken);

        if (!response.IsSuccess)
        {
            return BadRequest(new { response.Message });
        }

        return Ok(response);
    }

    [HttpPost("cheat-deposit/{accountId}")]
    public async Task<IActionResult> CheatDeposit(int accountId, [FromQuery] decimal amount)
    {
        var account = await context.Accounts.FindAsync(accountId);
        if (account == null) return NotFound(new { Message = "Hesap bulunamadı kral." });

        account.Balance += amount;
        account.AvailableBalance += amount;

        var transaction = new BankTransaction
        {
            AccountId = accountId,
            Amount = amount,
            Description = "Swagger üzerinden sistem parası basıldı.",
            TransactionDate = DateTime.UtcNow,
            TargetIban = account.Iban
        };

        context.BankTransactions.Add(transaction);
        await context.SaveChangesAsync();

        return Ok(new { Message = $"{amount} TL hesaba başarıyla basıldı. Yeni Bakiye: {account.Balance}" });
    }
}