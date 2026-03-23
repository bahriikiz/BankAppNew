import { Component, OnInit, signal, inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { VakifbankService } from '../../services/vakifbank.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule], 
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {
  authService = inject(AuthService);
  router = inject(Router);
  platformId = inject(PLATFORM_ID);
  vakifbankService = inject(VakifbankService); 

  public userProfile = signal<any>(null);
  public isLoading = signal(true);
  public isEditMode = signal(false);
  public editData: any = { 
    phoneNumber: '', 
    city: '', 
    district: '', 
    neighborhood: '', 
    address: '' 
  };

  // --- Şelale Dropdown Değişkenleri ---
  cities: any[] = [];
  districts: any[] = [];
  neighborhoods: any[] = [];

  selectedCityCode: string = '';
  selectedDistrictCode: string = '';
  selectedNeighborhoodCode: string = '';

  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.loadCities(); // Component yüklenirken şehirleri hazırla

      this.authService.getProfile().subscribe({
        next: (res) => {
          this.userProfile.set(res);
          // Gelen verileri düzenleme modeline kopyala (Tüm zorunlu alanlar)
          this.editData = { 
            firstName: res.firstName || res.FirstName || '',
            lastName: res.lastName || res.LastName || '',
            phoneNumber: res.phoneNumber || res.PhoneNumber || '', 
            city: res.city || res.City || '',
            district: res.district || res.District || '',
            neighborhood: res.neighborhood || res.Neighborhood || '',
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

  loadCities() {
    this.vakifbankService.getCities().subscribe({
      next: (res: any) => {
        this.cities = res.Data?.City || res.data?.city || [];
      },
      error: (err: any) => console.error("İller yüklenirken hata:", err)
    });
  }

  onCityChange() {
    this.selectedDistrictCode = '';
    this.selectedNeighborhoodCode = '';
    this.editData.district = ''; 
    this.editData.neighborhood = '';
    this.districts = [];
    this.neighborhoods = [];

    const selected = this.cities.find(c => c.cityCode === this.selectedCityCode);
    this.editData.city = selected ? selected.cityName : '';

    if (this.selectedCityCode) {
      this.vakifbankService.getDistricts(this.selectedCityCode).subscribe({
        next: (res: any) => {
          this.districts = res.Data?.District || res.data?.district || [];
        }
      });
    }
  }

  onDistrictChange() {
    this.selectedNeighborhoodCode = '';
    this.editData.neighborhood = '';
    this.neighborhoods = [];

    const selected = this.districts.find(d => d.districtCode === this.selectedDistrictCode);
    this.editData.district = selected ? selected.districtName : '';

    if (this.selectedDistrictCode) {
      this.vakifbankService.getNeighborhoods(this.selectedDistrictCode).subscribe({
        next: (res: any) => {
          this.neighborhoods = res.Data?.Neighborhood || res.data?.neighborhood || [];
        }
      });
    }
  }

  onNeighborhoodChange() {
    const selected = this.neighborhoods.find(n => n.neighborhoodCode === this.selectedNeighborhoodCode);
    this.editData.neighborhood = selected ? selected.neighborhoodName : '';
  }

  // Düzenleme modunu aç/kapat
  toggleEditMode(): void {
    this.isEditMode.set(!this.isEditMode());
    // İptal edilirse, orijinal verileri geri yükle
    if (!this.isEditMode()) {
      const res = this.userProfile();
      this.editData = { 
        firstName: res.firstName || res.FirstName || '',
        lastName: res.lastName || res.LastName || '',
        phoneNumber: res.phoneNumber || res.PhoneNumber || '',
        city: res.city || res.City || '',
        district: res.district || res.District || '',
        neighborhood: res.neighborhood || res.Neighborhood || '',
        address: res.address || res.Address || '' 
      };
      
      // Dropdown seçimlerini sıfırla
      this.selectedCityCode = '';
      this.selectedDistrictCode = '';
      this.selectedNeighborhoodCode = '';
      this.districts = [];
      this.neighborhoods = [];
    }
  }

  // Yeni bilgileri kaydet
  saveProfile(): void {
    // Güvenlik Kalkanı: Form eksikse backend'e gitmesin
    if (!this.editData.city || !this.editData.district || !this.editData.neighborhood || !this.editData.address) {
      alert("Lütfen İl, İlçe, Mahalle ve Açık Adres bilgilerinizi eksiksiz doldurun!");
      return;
    }

    this.authService.updateProfile(this.editData).subscribe({
      next: (res) => {
        // Arayüzdeki ana sinyali yeni verilerle güncelle
        const updatedProfile = { 
          ...this.userProfile(), 
          firstName: this.editData.firstName,
          lastName: this.editData.lastName,
          phoneNumber: this.editData.phoneNumber, 
          city: this.editData.city,
          district: this.editData.district,
          neighborhood: this.editData.neighborhood,
          address: this.editData.address 
        };
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

  // Şifre Değiştirme Modu Kontrolleri
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