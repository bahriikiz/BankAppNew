import { Component, inject, OnInit, signal } from '@angular/core';
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
export class DepositCalculatorComponent implements OnInit {
  vakifbankService = inject(VakifbankService);

  products: any[] = [];
  selectedProduct: any = null;

  // Hesaplama için API'ye gidecek istek gövdesi
  request = {
    amount: 600000,
    currencyCode: 'TL',
    depositType: 0,
    campaignId: 0,
    termDays: 32
  };

  result: any = null;
  isLoading = signal(false);
  errorMessage: string = '';

  ngOnInit() {
    this.loadProducts();
  }

  loadProducts() {
    this.vakifbankService.getDepositProducts().subscribe({
      next: (res: any) => {
        const data = res.data?.Data || res.data?.data || res.Data || res.data || res;
        const rawProducts = data?.DepositProduct || data?.depositProduct || [];
        // Adı cari faiz olan mevduatı dönme! Faiz hesaplayıcı yok.
        this.products = rawProducts.filter((p: any) => {
          const productName = p.ProductName || p.productName || '';
          return !productName.includes('Cari Faiz Oranları');
        });
        
        if (this.products.length > 0) {
          this.selectedProduct = this.products[0];
          this.onProductChange();
        }
      },
      error: (err) => {
        console.error("Mevduat ürünleri getirilemedi:", err);
      }
    });
  }

 
  onProductChange() {
    if (this.selectedProduct) {
      this.request.depositType = Number(this.selectedProduct.ProductCode || this.selectedProduct.productCode);
      this.request.campaignId = Number(this.selectedProduct.CampaignId || this.selectedProduct.campaignId);
      this.errorMessage = ''; 
      
      const supportedCurrencies = this.selectedProduct.CurrencyCode || this.selectedProduct.currencyCode || [];
      if (supportedCurrencies.length > 0 && !supportedCurrencies.includes(this.request.currencyCode)) {
        this.request.currencyCode = supportedCurrencies[0];
      }
    }
  }

  calculate() {
    this.errorMessage = '';
    this.result = null;

    if (this.selectedProduct) {
      const minAmount = this.selectedProduct.MinAmount || this.selectedProduct.minAmount;
      const maxAmount = this.selectedProduct.MaxAmount || this.selectedProduct.maxAmount;
      const minTerm = this.selectedProduct.MinTerm || this.selectedProduct.minTerm;
      const maxTerm = this.selectedProduct.MaxTerm || this.selectedProduct.maxTerm;

      if (this.request.amount < minAmount || this.request.amount > maxAmount) {
        this.errorMessage = `Seçilen ürün için tutar ${minAmount} ile ${maxAmount} arasında olmalıdır.`;
        return;
      }
      if (this.request.termDays < minTerm || this.request.termDays > maxTerm) {
        this.errorMessage = `Seçilen ürün için vade ${minTerm} ile ${maxTerm} gün arasında olmalıdır.`;
        return;
      }
    }

    this.isLoading.set(true);

    this.vakifbankService.calculateDeposit(
      this.request.amount,
      this.request.currencyCode,
      this.request.depositType,
      this.request.campaignId,
      this.request.termDays
    ).subscribe({
      next: (res: any) => {
        const responseData = res.data?.Data || res.data?.data || res.Data || res.data || res;
        this.result = responseData?.Deposit || responseData?.deposit;
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Hesaplama hatası:', err);
        this.errorMessage = err.error?.message || 'Mevduat hesaplanırken bir hata oluştu.';
        this.isLoading.set(false);
      }
    });
  }
}