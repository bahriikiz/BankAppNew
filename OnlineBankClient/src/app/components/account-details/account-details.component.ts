import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AccountService } from '../../services/account.service';
import { AiService } from '../../services/ai.service'; 

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
  // Genel yüklenme ve hata durumları için sinyaller
  public isLoading = signal(true);
  public isTransactionsLoading = signal(false);
  public isReceiptLoading = signal<string | null>(null);
  public errorMessage = signal('');

  // --- DEKONT MODALI SİNYALLERİ ---
  public showReceiptModal = signal(false);
  public currentReceiptText = signal('');
  public currentReceiptTxn = signal<any>(null);
  public isPdfLoading = signal(false);

  // --- YAPAY ZEKA VE GRAFİK SİNYALLERİ ---
  public showAiModal = signal(false);
  public isAiAnalyzing = signal(false);
  public aiAnalysisResult = signal('');
  
  // Görsel Grafik İçin Eklenen Sinyaller
  public incomeTotal = signal<number>(0);
  public expenseTotal = signal<number>(0);
  public incomePercentage = signal<number>(0);
  public expensePercentage = signal<number>(0);

  public startDate = signal<string>('');
  public endDate = signal<string>('');
  private readonly currentAccountId = signal<number>(0);

  private readonly route = inject(ActivatedRoute);
  private readonly accountService = inject(AccountService);
  private readonly aiService = inject(AiService); 

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

  // --- KUSURSUZ YAPAY ZEKA VE MATEMATİK MOTORU ---
  analyzeWithAI() {
    const txns = this.transactions();
    if (txns.length === 0) return;

    // 1. Gelir/Gider Matematiğini Hesapla
    let inc = 0;
    let exp = 0;
    txns.forEach(t => {
      if (t.amount > 0) inc += t.amount;
      else exp += Math.abs(t.amount); 
    });
    
    // Yüzdelik dilimleri bul (Progress Bar için)
    const total = inc + exp;
    this.incomeTotal.set(inc);
    this.expenseTotal.set(exp);
    this.incomePercentage.set(total === 0 ? 0 : Math.round((inc / total) * 100));
    this.expensePercentage.set(total === 0 ? 0 : Math.round((exp / total) * 100));

    // 2. Modalı aç ve AI'a istek at
    this.showAiModal.set(true);
    this.isAiAnalyzing.set(true);
    this.aiAnalysisResult.set('');

    const txnDetails = txns.map(t => 
      `${new Date(t.transactionDate).toLocaleDateString()} - ${t.description}: ${t.amount > 0 ? '+' : ''}${t.amount} ${this.accountDetails().currency}`
    ).join('\n');

    const prompt = `Merhaba İKİZ AI. Aşağıdaki ${this.accountDetails().currency} hesabıma ait hareketleri inceleyip bana profesyonel bir finansal özet çıkarır mısın? 
    Lütfen şunları yap:
    1. Toplam Gelir ve Gider durumumu analiz et.
    2. Harcama trendlerimi (en çok paranın nereye gittiğini vb.) bul.
    3. Bana kurumsal, net ve kısa bir finansal tavsiyede bulun.
    
    İşte işlemlerim:\n${txnDetails}`;

    this.aiService.askAI(prompt).subscribe({
      next: (res) => {
        this.aiAnalysisResult.set(res.message);
        this.isAiAnalyzing.set(false);
      },
      error: () => {
        this.aiAnalysisResult.set("Yapay zeka sistemleri şu an meşgul. Lütfen daha sonra tekrar deneyin.");
        this.isAiAnalyzing.set(false);
      }
    });
  }

  closeAiModal() {
    this.showAiModal.set(false);
    this.aiAnalysisResult.set('');
  }

  // --- DEKONT İŞLEMLERİ ---
  getReceipt(txn: any) {
    const ref = txn.transactionReference; 
    if (!ref) {
      alert("Bu işlem için dekont numarası bulunamadı.");
      return;
    }
    const loaderId = txn.id || ref;
    this.isReceiptLoading.set(loaderId); 
    this.accountService.downloadReceipt(this.currentAccountId(), ref, '2').subscribe({
      next: (res) => {
        this.isReceiptLoading.set(null);
        this.currentReceiptText.set(res.data || res);
        this.currentReceiptTxn.set(txn);
        this.showReceiptModal.set(true); 
      },
      error: () => {
        this.isReceiptLoading.set(null);
        alert("Dekont alınırken bir hata oluştu.");
      }
    });
  }

  closeReceiptModal() {
    this.showReceiptModal.set(false);
    this.currentReceiptText.set('');
    this.currentReceiptTxn.set(null);
  }

  downloadPdf() {
    const txn = this.currentReceiptTxn();
    if (!txn?.transactionReference) return;
    this.isPdfLoading.set(true);
    this.accountService.downloadReceipt(this.currentAccountId(), txn.transactionReference, '1').subscribe({
      next: (res) => {
        this.isPdfLoading.set(false);
        const base64Data = res.data || res;
        try {
          const byteCharacters = atob(base64Data);
          const byteNumbers = new Array(byteCharacters.length);
          for (let i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.codePointAt(i) ?? 0;
          }
          const byteArray = new Uint8Array(byteNumbers);
          const blob = new Blob([byteArray], { type: 'application/pdf' });
          const link = document.createElement('a');
          link.href = URL.createObjectURL(blob);
          link.download = `Dekont_${txn.transactionReference}.pdf`;
          link.click();
          URL.revokeObjectURL(link.href);
        } catch (error) {
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