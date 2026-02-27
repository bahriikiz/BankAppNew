import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms'; // YENİ: Input'lar için
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AccountService } from '../../services/account.service';

@Component({
  selector: 'app-account-details',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './account-details.component.html',
  styleUrl: './account-details.component.css'
})
export class AccountDetailsComponent implements OnInit {
  public accountDetails = signal<any>(null); // Üst kart verileri
  public transactions = signal<any[]>([]); // Liste verileri
  
  public isLoading = signal(true);
  public isTransactionsLoading = signal(false);
  public errorMessage = signal('');

  // Tarih Filtreleri
  public startDate = signal<string>('');
  public endDate = signal<string>('');
  private readonly currentAccountId = signal<number>(0);

  private readonly route = inject(ActivatedRoute);
  private readonly accountService = inject(AccountService);

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.currentAccountId.set(Number(id));
      this.fetchAccountInfo(id);
      this.setQuickDate(30); // Ekrana ilk girişte 30 günlük işlemleri çeker
    } else {
      this.errorMessage.set('Hesap ID bulunamadı.');
      this.isLoading.set(false);
    }
  }

  // 1. Üstteki Bakiye ve IBAN Kartını Doldur
  fetchAccountInfo(id: string) {
    this.accountService.getAccountDetails(id).subscribe({
      next: (res) => {
        this.accountDetails.set(res);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Hesap bilgileri yüklenirken bir hata oluştu.');
        this.isLoading.set(false);
      }
    });
  }

  // 2. Hızlı Tarih Seçimi Butonları (1 Hafta, 1 Ay, 3 Ay)
  setQuickDate(days: number) {
    const end = new Date();
    const start = new Date();
    start.setDate(end.getDate() - days);

    this.startDate.set(start.toISOString().split('T')[0]);
    this.endDate.set(end.toISOString().split('T')[0]);
    
    this.applyFilter();
  }

  // 3. API'den İşlemleri Çek (Tarih Filtreli)
  applyFilter() {
    this.isTransactionsLoading.set(true);
    
    // .NET DateTime formatına tam uyumlu olması için saatleri manuel ekliyoruz
    const payload = {
      accountId: this.currentAccountId(),
      startDate: this.startDate() ? `${this.startDate()}T00:00:00` : undefined,
      endDate: this.endDate() ? `${this.endDate()}T23:59:59` : undefined
    };

    this.accountService.getAccountActivities(payload).subscribe({
      next: (res) => {
        this.transactions.set(res);
        this.isTransactionsLoading.set(false);
      },
      error: (err) => {
        console.error("Hareketler yüklenemedi", err);
        this.isTransactionsLoading.set(false);
      }
    });
  }
}