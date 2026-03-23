import { Component, OnInit, signal, inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { VakifbankService } from '../../services/vakifbank.service';
import { firstValueFrom } from 'rxjs';

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
      this.authService.getProfile().subscribe({
        next: (res) => {
          this.userProfile.set(res);
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

          this.loadCitiesAndAutoSelect();
        },
        error: (err) => {
          console.error("Profil bilgileri çekilemedi", err);
          this.isLoading.set(false);
        }
      });
    }
  }

  async loadCitiesAndAutoSelect() {
    try {
      // 1. ŞEHİRLER: Katmanı çöz ve HTML'in beklediği küçük harfli formata dönüştür (.map ile)
      const cRes: any = await firstValueFrom(this.vakifbankService.getCities());
      const cityData = cRes.data?.Data || cRes.data?.data || cRes.Data || cRes.data || cRes;
      const rawCities = cityData?.City || cityData?.city || [];
      
      this.cities = rawCities.map((c: any) => ({
        cityCode: String(c.cityCode || c.CityCode),
        cityName: c.cityName || c.CityName
      }));
      
      if (!this.editData.city) {
        this.resetDropdownState(1);
        return; 
      }
      
      const matchedCity = this.cities.find(c => c.cityName?.trim().toUpperCase() === this.editData.city?.trim().toUpperCase());
      if (!matchedCity) {
        this.resetDropdownState(1);
        return; 
      }
      
      this.selectedCityCode = matchedCity.cityCode;
      const formattedCityCode = String(this.selectedCityCode).padStart(2, '0');

      // 2. İLÇELER: Katmanı çöz ve dönüştür
      const dRes: any = await firstValueFrom(this.vakifbankService.getDistricts(formattedCityCode));
      const districtData = dRes.data?.Data || dRes.data?.data || dRes.Data || dRes.data || dRes;
      const rawDistricts = districtData?.District || districtData?.district || [];

      this.districts = rawDistricts.map((d: any) => ({
        districtCode: String(d.districtCode || d.DistrictCode),
        districtName: d.districtName || d.DistrictName
      }));

      if (!this.editData.district) {
        this.resetDropdownState(2);
        return; 
      }

      const matchedDistrict = this.districts.find(d => d.districtName?.trim().toUpperCase() === this.editData.district?.trim().toUpperCase());
      if (!matchedDistrict) {
        this.resetDropdownState(2);
        return;
      }

      this.selectedDistrictCode = matchedDistrict.districtCode;

      // 3. MAHALLELER: Katmanı çöz ve dönüştür
      const nRes: any = await firstValueFrom(this.vakifbankService.getNeighborhoods(this.selectedDistrictCode));
      const neighborhoodData = nRes.data?.Data || nRes.data?.data || nRes.Data || nRes.data || nRes;
      const rawNeighborhoods = neighborhoodData?.Neighborhood || neighborhoodData?.neighborhood || [];

      this.neighborhoods = rawNeighborhoods.map((n: any) => ({
        neighborhoodCode: String(n.neighborhoodCode || n.NeighborhoodCode),
        neighborhoodName: n.neighborhoodName || n.NeighborhoodName
      }));

      if (!this.editData.neighborhood) {
        this.resetDropdownState(3);
        return; 
      }

      const matchedNeigh = this.neighborhoods.find(n => n.neighborhoodName?.trim().toUpperCase() === this.editData.neighborhood?.trim().toUpperCase());
      if (!matchedNeigh) {
        this.resetDropdownState(3);
        return;
      }

      this.selectedNeighborhoodCode = matchedNeigh.neighborhoodCode;

    } catch (err) {
      console.error("Adres eşleştirme veya yükleme sırasında hata oluştu:", err);
    }
  }

  private resetDropdownState(level: number) {
    if (level <= 1) this.selectedCityCode = '';
    if (level <= 2) {
      this.selectedDistrictCode = '';
      this.districts = [];
    }
    if (level <= 3) {
      this.selectedNeighborhoodCode = '';
      this.neighborhoods = [];
    }
  }

  // Kullanıcı dropdown'dan yeni bir il seçtiğinde tetiklenir
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
      const formattedCityCode = String(this.selectedCityCode).padStart(2, '0');

      this.vakifbankService.getDistricts(formattedCityCode).subscribe({
        next: (res: any) => {
          // İlçeler gelirken de güvenli şekilde map'le
          const districtData = res.data?.Data || res.data?.data || res.Data || res.data || res;
          const rawDistricts = districtData?.District || districtData?.district || [];
          
          this.districts = rawDistricts.map((d: any) => ({
            districtCode: String(d.districtCode || d.DistrictCode),
            districtName: d.districtName || d.DistrictName
          }));
        }
      });
    }
  }

  // Kullanıcı dropdown'dan yeni bir ilçe seçtiğinde tetiklenir
  onDistrictChange() {
    this.selectedNeighborhoodCode = '';
    this.editData.neighborhood = '';
    this.neighborhoods = [];

    const selected = this.districts.find(d => d.districtCode === this.selectedDistrictCode);
    this.editData.district = selected ? selected.districtName : '';

    if (this.selectedDistrictCode) {
      this.vakifbankService.getNeighborhoods(this.selectedDistrictCode).subscribe({
        next: (res: any) => {
          // Mahalleler gelirken de güvenli şekilde map'le
          const neighborhoodData = res.data?.Data || res.data?.data || res.Data || res.data || res;
          const rawNeighborhoods = neighborhoodData?.Neighborhood || neighborhoodData?.neighborhood || [];
          
          this.neighborhoods = rawNeighborhoods.map((n: any) => ({
            neighborhoodCode: String(n.neighborhoodCode || n.NeighborhoodCode),
            neighborhoodName: n.neighborhoodName || n.NeighborhoodName
          }));
        }
      });
    }
  }

  onNeighborhoodChange() {
    const selected = this.neighborhoods.find(n => n.neighborhoodCode === this.selectedNeighborhoodCode);
    this.editData.neighborhood = selected ? selected.neighborhoodName : '';
  }

  toggleEditMode(): void {
    this.isEditMode.set(!this.isEditMode());
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
      
      this.loadCitiesAndAutoSelect();
    }
  }

  saveProfile(): void {
    if (!this.editData.city || !this.editData.district || !this.editData.neighborhood || !this.editData.address) {
      alert("Lütfen İl, İlçe, Mahalle ve Açık Adres bilgilerinizi eksiksiz doldurun!");
      return;
    }

    const payload = {
      phoneNumber: this.editData.phoneNumber,
      city: this.editData.city,
      district: this.editData.district,
      neighborhood: this.editData.neighborhood,
      address: this.editData.address
    };

    this.authService.updateProfile(payload).subscribe({
      next: (res) => {
        const updatedProfile = { 
          ...this.userProfile(), 
          phoneNumber: this.editData.phoneNumber, 
          city: this.editData.city,
          district: this.editData.district,
          neighborhood: this.editData.neighborhood,
          address: this.editData.address 
        };
        this.userProfile.set(updatedProfile);
        
        this.isEditMode.set(false);
        alert("Harika! Profil bilgileriniz başarıyla güncellendi.");
      },
      error: (err) => {
        console.error("Güncelleme hatası", err);
        alert("Güncelleme sırasında bir sorun oluştu: " + (err.error?.message || err.message));
      }
    });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigateByUrl('/login');
  }

  public isPasswordEditMode = signal(false);
  public passwordData = { currentPassword: '', newPassword: '', confirmPassword: '' };

  togglePasswordMode(): void {
    this.isPasswordEditMode.set(!this.isPasswordEditMode());
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
        this.logout();
      },
      error: (err) => {
        console.error("Şifre güncelleme hatası", err);
        alert(err.error?.message || "Şifre değiştirilemedi. Mevcut şifrenizi doğru girdiğinizden emin olun.");
      }
    });
  }
}