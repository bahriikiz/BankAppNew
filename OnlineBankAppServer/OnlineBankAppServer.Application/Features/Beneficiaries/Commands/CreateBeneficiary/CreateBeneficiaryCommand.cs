using MediatR;
using OnlineBankAppServer.Domain.Abstractions; 

namespace OnlineBankAppServer.Application.Features.Beneficiaries.Commands.CreateBeneficiary;

public sealed record CreateBeneficiaryCommand(
    string Name, // Kayıt Adı (Örn: "Kira Hesabı")
    string Iban  // Kaydedilecek IBAN
) : IRequest<string>;