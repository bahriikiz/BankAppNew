using MediatR;
using Microsoft.AspNetCore.Http;
using OnlineBankAppServer.Domain.Entities;
using OnlineBankAppServer.Persistance;
using System.Security.Claims;

namespace OnlineBankAppServer.Application.Features.Beneficiaries.Commands.CreateBeneficiary;

internal sealed class CreateBeneficiaryCommandHandler(
    AppDbContext context,
    IHttpContextAccessor httpContextAccessor) : IRequestHandler<CreateBeneficiaryCommand, string>
{
    public async Task<string> Handle(CreateBeneficiaryCommand request, CancellationToken cancellationToken)
    {
        // 1. Kullanıcıyı Bul
        var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null) throw new Exception("Kullanıcı bulunamadı.");
        int userId = int.Parse(userIdClaim.Value);

        // 2. Aynı IBAN daha önce eklenmiş mi? 
        bool isExists = context.Beneficiaries.Any(x => x.UserId == userId && x.Iban == request.Iban);
        if (isExists)
        {
            throw new Exception("Bu IBAN zaten rehberinizde kayıtlı.");
        }

        // 3. Kaydı Oluştur
        Beneficiary beneficiary = new()
        {
            UserId = userId,
            Name = request.Name,
            Iban = request.Iban,
            CreatedDate = DateTime.Now 
        };

        // 4. Veritabanına Ekle
        await context.Beneficiaries.AddAsync(beneficiary, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return "Alıcı başarıyla rehbere eklendi.";
    }
}