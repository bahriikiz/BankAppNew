import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../../services/account.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-money-transfer', 
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './money-transfer.component.html'
})
export class MoneyTransferComponent implements OnInit {
  private readonly accountService = inject(AccountService);
  private readonly router = inject(Router);

  senderAccountId = signal<string>('');
  TargetIban = signal<string>('');
  amount = signal<number | null>(null);
  description = signal<string>('');

  myAccounts = signal<any[]>([]);
  isSubmitting = signal(false);

  ngOnInit() {
    this.loadMyAccounts();
  }

  loadMyAccounts() {
    this.accountService.getAccounts().subscribe({
      next: (res: any) => {
        const accountsList = res.data ? res.data : res;
        this.myAccounts.set(accountsList);
        if (accountsList && accountsList.length > 0) {
          this.senderAccountId.set(accountsList[0].id);
        }
      },
      error: (err) => console.error(err)
    });
  }

  getSelectedCurrencySymbol(): string {
    const selectedId = this.senderAccountId();
    
    if (!selectedId || !this.myAccounts() || this.myAccounts().length === 0) {
      return '₺';
    }

    const selectedAccount = this.myAccounts().find(a => a.id == selectedId);
    
    if (!selectedAccount) return '₺';

    switch (selectedAccount.currencyType) {
      case 'TRY': return '₺';
      case 'USD': return '$';
      case 'EUR': return '€';
      case 'XAU': return '🥇';
      default: return selectedAccount.currencyType || '₺';
    }
  }

  onSubmit() {
    if (!this.senderAccountId() || !this.TargetIban() || !this.amount()) {
      alert("Lütfen tüm alanları doldurun!");
      return;
    }

    const payload = {
      accountId: Number(this.senderAccountId()),
      targetIban: this.TargetIban(),
      amount: this.amount(),
      description: this.description() || 'Para Transferi'
    };

    this.isSubmitting.set(true);
    
    this.accountService.transferMoney(payload).subscribe({
      next: () => {
        alert("Transfer işlemi başarıyla gerçekleşti!");
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        const hataMesaji = err.error?.Message || err.error?.message || err.error || "Transfer sırasında bir hata oluştu.";
        alert(hataMesaji);
        this.isSubmitting.set(false);
      }
    });
  }
}