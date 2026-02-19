using FluentValidation;

namespace OnlineBankAppServer.Application.Features.Transactions.Commands.MoneyTransfer;

public sealed class MoneyTransferCommandValidator : AbstractValidator<MoneyTransferCommand>
{
    public MoneyTransferCommandValidator()
    {
        // 1. Temel Kontroller
        RuleFor(p => p.AccountId).NotEmpty().WithMessage("Hesap bilgisi zorunludur.");
        RuleFor(p => p.Amount).GreaterThan(0).WithMessage("Transfer tutarı 0'dan büyük olmalıdır.");

        // 2. Akıllı IBAN Kontrolü
 
        RuleFor(p => p.TargetIban)
            .NotEmpty()
            .When(p => p.BeneficiaryId == null || p.BeneficiaryId <= 0)
            .WithMessage("Lütfen bir IBAN giriniz veya rehberden bir alıcı seçiniz.");

        // Format Kontrolü
      
        RuleFor(p => p.TargetIban)
            .Matches(@"^TR\d{24}$").WithMessage("Geçerli bir TR IBAN giriniz (TR + 24 rakam).")
            .When(p => !string.IsNullOrEmpty(p.TargetIban));
    }
}