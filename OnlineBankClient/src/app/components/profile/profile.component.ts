import { Component, OnInit, signal, inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms'; // YENİ: Inputları bağlamak için gerekli
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule], // YENİ: FormsModule eklendi
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {
  authService = inject(AuthService);
  router = inject(Router);
  platformId = inject(PLATFORM_ID);

  public userProfile = signal<any>(null);
  public isLoading = signal(true);
  
  // Düzenleme Modu Kontrolleri
  public isEditMode = signal(false);
  public editData = { phoneNumber: '', address: '' };

  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.authService.getProfile().subscribe({
        next: (res) => {
          this.userProfile.set(res);
          // Gelen verileri düzenleme modeline kopyala
          this.editData = { 
            phoneNumber: res.phoneNumber || res.PhoneNumber || '', 
            address: res.address || res.Address || '' 
          };
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error("Profil bilgileri çekilemedi", err);
          this.isLoading.set(false);
        }
      });
    }
  }

  // Düzenleme modunu aç/kapat
  toggleEditMode(): void {
    this.isEditMode.set(!this.isEditMode());
    // İptal edilirse, orijinal verileri geri yükle
    if (!this.isEditMode()) {
      this.editData = { 
        phoneNumber: this.userProfile().phoneNumber, 
        address: this.userProfile().address 
      };
    }
  }

  // Yeni bilgileri kaydet
  saveProfile(): void {
    this.authService.updateProfile(this.editData).subscribe({
      next: (res) => {
        // Arayüzdeki ana sinyali yeni verilerle güncelle
        const updatedProfile = { ...this.userProfile(), phoneNumber: this.editData.phoneNumber, address: this.editData.address };
        this.userProfile.set(updatedProfile);
        
        this.isEditMode.set(false); // Düzenleme modunu kapat
        alert("Harika! Profil bilgileriniz başarıyla güncellendi.");
      },
      error: (err) => {
        console.error("Güncelleme hatası", err);
        alert("Güncelleme sırasında bir sorun oluştu.");
      }
    });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigateByUrl('/login');
  }

  // YENİ: Şifre Değiştirme Modu Kontrolleri
  public isPasswordEditMode = signal(false);
  public passwordData = { currentPassword: '', newPassword: '', confirmPassword: '' };

  togglePasswordMode(): void {
    this.isPasswordEditMode.set(!this.isPasswordEditMode());
    // İptal edilirse formun içini temizle
    this.passwordData = { currentPassword: '', newPassword: '', confirmPassword: '' };
  }

  savePassword(): void {
    if (this.passwordData.newPassword !== this.passwordData.confirmPassword) {
      alert("Hata: Yeni şifreler birbiriyle uyuşmuyor!");
      return;
    }

    this.authService.changePassword(this.passwordData).subscribe({
      next: (res) => {
        alert("Harika! Şifreniz başarıyla güncellendi. Güvenliğiniz için çıkış yapılıyor...");
        this.logout(); // Şifre değiştiği için sistemi sıfırlayıp Login'e atıyoruz!
      },
      error: (err) => {
        console.error("Şifre güncelleme hatası", err);
        alert(err.error?.message || "Şifre değiştirilemedi. Mevcut şifrenizi doğru girdiğinizden emin olun.");
      }
    });
  }
}