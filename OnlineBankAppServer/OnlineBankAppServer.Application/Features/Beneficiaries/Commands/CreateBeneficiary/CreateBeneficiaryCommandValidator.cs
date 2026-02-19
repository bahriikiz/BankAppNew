using FluentValidation;

namespace OnlineBankAppServer.Application.Features.Beneficiaries.Commands.CreateBeneficiary;

public sealed class CreateBeneficiaryCommandValidator : AbstractValidator<CreateBeneficiaryCommand>
{
    public CreateBeneficiaryCommandValidator()
    {
        RuleFor(p => p.Name).NotEmpty().WithMessage("Kayıt adı boş olamaz.");
        RuleFor(p => p.Iban).NotEmpty().WithMessage("IBAN boş olamaz.");
        RuleFor(p => p.Iban)
            .NotEmpty().WithMessage("IBAN boş olamaz.")
            .Matches(@"^TR\d{24}$").WithMessage("Geçerli bir TR IBAN giriniz. (TR ile başlamalı ve 26 karakter olmalı).");
    }
}