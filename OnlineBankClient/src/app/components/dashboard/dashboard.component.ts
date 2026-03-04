import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms'; 
import { AccountService } from '../../services/account.service';
import { AuthService } from '../../services/auth.service';
import { Account } from '../../models/account.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule], 
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  public accounts = signal<Account[]>([]);
  public isLoading = signal(true);
  public isSyncing = signal(false);

  // Modal'ı kontrol edecek sinyaller
  public isModalOpen = signal(false);
  public rizaNoInput = signal('');

  private readonly accountService = inject(AccountService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  ngOnInit() {
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/login']);
      return;
    }
    this.fetchAccounts();
  }

  fetchAccounts() {
    this.isLoading.set(true);
    this.accountService.getAccounts().subscribe({
      next: (res: Account[]) => {
        this.accounts.set(res);
        this.isLoading.set(false);
      },
      error: (err: any) => {
        console.error("Hesaplar yüklenirken hata oluştu:", err);
        this.isLoading.set(false);
      }
    });
  }

  // Modalı Aç/Kapat fonksiyonu
  toggleModal() {
    this.isModalOpen.set(!this.isModalOpen());
    if (!this.isModalOpen()) {
      this.rizaNoInput.set(''); // Kapanırken inputu temizle
    }
  }

  // Modaldan gelen Rıza Numarasını Gönder
  submitRizaNo() {
    const rizaNo = this.rizaNoInput();
    
    if (!rizaNo || rizaNo.trim() === '') {
      return; 
    }

    this.isModalOpen.set(false); // İşlem başlarken modalı şak diye kapat
    this.isSyncing.set(true);
    
    this.accountService.syncVakifbankAccounts(rizaNo).subscribe({
      next: () => {
        alert("Hesaplarınız başarıyla Açık Bankacılık ağına dahil edildi!");
        this.fetchAccounts(); 
        this.isSyncing.set(false);
        this.rizaNoInput.set(''); 
      },
      error: (err: any) => {
        console.error("Senkronizasyon hatası:", err);
        const hataMesaji = err.error?.Message || err.error?.message || "Bağlantı sırasında bir hata oluştu!";
        alert(hataMesaji); 
        this.isSyncing.set(false);
      }
    });
  }
}