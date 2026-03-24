import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { VakifbankService } from '../../services/vakifbank.service';

@Component({
  selector: 'app-deposit-calculator',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './deposit-calculator.component.html',
  styleUrl: './deposit-calculator.component.css'
})
export class DepositCalculatorComponent {
  vakifbankService = inject(VakifbankService);

  // Senin Postman'deki girdi değerlerin (Varsayılan olarak dolu gelsin)
  request = {
    amount: 600000,
    currencyCode: 'TL',
    depositType: 55500003,
    campaignId: 6000002324,
    termDays: 32
  };

  // Sonuç Parametreleri
  result: any = null;
  isLoading = signal(false);
  errorMessage: string = '';

  calculate() {
    this.isLoading.set(true);
    this.errorMessage = '';
    this.result = null;

    this.vakifbankService.calculateDeposit(
      this.request.amount,
      this.request.currencyCode,
      this.request.depositType,
      this.request.campaignId,
      this.request.termDays
    ).subscribe({
      next: (res: any) => {
        // Backend'den dönen Data.Deposit objesine ulaşıyoruz
        const responseData = res.data?.Data || res.data?.data || res.Data || res.data || res;
        this.result = responseData?.Deposit || responseData?.deposit;
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Hesaplama hatası:', err);
        this.errorMessage = 'Mevduat hesaplanırken bir hata oluştu. Lütfen tekrar deneyin.';
        this.isLoading.set(false);
      }
    });
  }
}