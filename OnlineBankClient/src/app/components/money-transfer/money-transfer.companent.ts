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

  // Form Verileri
  senderAccountId = signal<string>('');
  receiverIban = signal<string>('');
  amount = signal<number | null>(null);
  description = signal<string>('');

  // Yardımcı Veriler
  myAccounts = signal<any[]>([]);
  isSubmitting = signal(false);

  ngOnInit() {
    this.loadMyAccounts();
  }

  loadMyAccounts() {
    this.accountService.getAccounts().subscribe(res => {
      this.myAccounts.set(res);
      if (res.length > 0) this.senderAccountId.set(res[0].id);
    });
  }

  onSubmit() {
    if (!this.senderAccountId() || !this.receiverIban() || !this.amount()) {
      alert("Lütfen tüm alanları doldurun!");
      return;
    }

    const payload = {
      accountId: this.senderAccountId(),
      receiverIban: this.receiverIban(),
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
        alert(err.error?.Message || "Transfer sırasında bir hata oluştu.");
        this.isSubmitting.set(false);
      }
    });
  }
}