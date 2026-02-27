import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
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
  public accountDetails = signal<any>(null);
  public transactions = signal<any[]>([]);
  
  public isLoading = signal(true);
  public isTransactionsLoading = signal(false);
  public isReceiptLoading = signal<string | null>(null);
  public errorMessage = signal('');

  // --- YENİ: DEKONT MODALI SİNYALLERİ ---
  public showReceiptModal = signal(false);
  public currentReceiptText = signal('');
  public currentReceiptTxn = signal<any>(null);
  public isPdfLoading = signal(false);

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
      this.setQuickDate(30);
    } else {
      this.errorMessage.set('Hesap ID bulunamadı.');
      this.isLoading.set(false);
    }
  }

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

  setQuickDate(days: number) {
    const end = new Date();
    const start = new Date();
    start.setDate(end.getDate() - days);

    this.startDate.set(start.toISOString().split('T')[0]);
    this.endDate.set(end.toISOString().split('T')[0]);
    
    this.applyFilter();
  }

  applyFilter() {
    this.isTransactionsLoading.set(true);
    
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

  // --- YENİ: DEKONTU MODALDA AÇ ---
  getReceipt(txn: any) {
    const ref = txn.transactionReference; 
    if (!ref) {
      alert("Bu işlem için dekont numarası bulunamadı.");
      return;
    }

    const loaderId = txn.id || ref;
    this.isReceiptLoading.set(loaderId); 

    // Önce ekranda göstermek için TXT (2) formatında çekiyoruz
    this.accountService.downloadReceipt(this.currentAccountId(), ref, '2').subscribe({
      next: (res) => {
        this.isReceiptLoading.set(null);
        this.currentReceiptText.set(res.data || res);
        this.currentReceiptTxn.set(txn);
        this.showReceiptModal.set(true); // Modalı aç
      },
      error: () => {
        this.isReceiptLoading.set(null);
        alert("Dekont alınırken bir hata oluştu.");
      }
    });
  }

  // --- YENİ: MODALI KAPAT ---
  closeReceiptModal() {
    this.showReceiptModal.set(false);
    this.currentReceiptText.set('');
    this.currentReceiptTxn.set(null);
  }

  // --- YENİ: PDF OLARAK İNDİR ---
  downloadPdf() {
    const txn = this.currentReceiptTxn();
    if (!txn?.transactionReference) return;

    this.isPdfLoading.set(true);

    // API'den PDF (1) formatında Base64 verisini istiyoruz
    this.accountService.downloadReceipt(this.currentAccountId(), txn.transactionReference, '1').subscribe({
      next: (res) => {
        this.isPdfLoading.set(false);
        const base64Data = res.data || res;
        
        try {
          // Base64'ü tarayıcının indirebileceği bir Blob'a (Dosyaya) dönüştür
          const byteCharacters = atob(base64Data);
          const byteNumbers = new Array(byteCharacters.length);
          for (let i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.codePointAt(i) ?? 0;
          }
          const byteArray = new Uint8Array(byteNumbers);
          const blob = new Blob([byteArray], { type: 'application/pdf' });
          
          // Görünmez bir link yarat ve tıklat (Dosyayı indirir)
          const link = document.createElement('a');
          link.href = URL.createObjectURL(blob);
          link.download = `Dekont_${txn.transactionReference}.pdf`;
          link.click();
          URL.revokeObjectURL(link.href);
        } 
         catch (error) {
          // UYARI 3 ÇÖZÜMÜ: Yakalanan hatayı yutmuyoruz, geliştirici için konsola basıyoruz
          console.error("PDF Dönüştürme Hatası:", error);
          alert("PDF indirilirken bir dönüştürme hatası oluştu.");
        }
      },
      error: () => {
        this.isPdfLoading.set(false);
        alert("PDF indirilirken bir hata oluştu.");
      }
    });
  }
}