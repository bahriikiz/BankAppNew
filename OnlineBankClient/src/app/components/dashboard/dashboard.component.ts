import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { AccountService } from '../../services/account.service';
import { AuthService } from '../../services/auth.service';
import { Account } from '../../models/account.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  public accounts = signal<Account[]>([]);
  public isLoading = signal(true);
  public isSyncing = signal(false); // Vakıfbank butonu için yükleniyor durumu

  private readonly accountService = inject(AccountService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  ngOnInit() {
    // Güvenlik: Oturum yoksa login'e gönder
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/login']);
      return;
    }
    this.fetchAccounts();
  }

  // Backend'den kullanıcının hesaplarını çeker
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

  addOtherBankAccount() {
    // Kullanıcıdan Rıza No istiyoruz
    const rizaNo = globalThis.prompt("Lütfen bankanızdan aldığınız Açık Bankacılık 'Rıza Numarasını' (Rıza No) giriniz:");
    
    // Eğer kullanıcı iptale basarsa veya boş bırakırsa işlemi durdur
    if (!rizaNo || rizaNo.trim() === '') {
      return; 
    }

    this.isSyncing.set(true);
    
    // Rıza No'yu backend'e yolluyoruz
    this.accountService.syncVakifbankAccounts(rizaNo).subscribe({
      next: () => {
        alert("Hesaplarınız başarıyla Açık Bankacılık ağına dahil edildi!");
        this.fetchAccounts(); 
        this.isSyncing.set(false);
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