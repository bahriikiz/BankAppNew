import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AccountService } from '../../services/account.service';

@Component({
  selector: 'app-create-account',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './create-account.component.html',
  styleUrl: './create-account.component.css'
})
export class CreateAccountComponent {
  public model = { accountName: '', currencyType: 'TRY' };
  public isLoading = signal(false);
  public errorMessage = signal('');

  private readonly accountService = inject(AccountService);
  private readonly router = inject(Router);

  onSubmit() {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.accountService.createAccount(this.model).subscribe({
      next: () => {
        alert('İKİZ BANK hesabınız başarıyla oluşturuldu!');
        this.router.navigate(['/dashboard']);
      },
      error: (err: any) => {
        console.error(err);
        this.isLoading.set(false);
        this.errorMessage.set(err.error?.Message || err.error?.message || 'Hesap oluşturulurken bir hata oluştu.');
      }
    });
  }
}