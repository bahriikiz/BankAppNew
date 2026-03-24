import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms'; 
import { VakifbankService } from '../../services/vakifbank.service';

@Component({
  selector: 'app-branch-locator',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './branch-locator.component.html',
  styleUrl: './branch-locator.component.css'
})
export class BranchLocatorComponent implements OnInit {
  vakifbankService = inject(VakifbankService);

  cities: any[] = [];
  districts: any[] = [];
  branches: any[] = [];
  nearestList: any[] = [];

  selectedCityCode: string = '';
  BankDistrictCode: string = '';
  isLoadingBranches = signal(false);
  isLocating = signal(false);
  locationError: string = '';

  ngOnInit() {
    this.loadCities(); 
  }

  loadCities() {
    this.vakifbankService.getCities().subscribe({
      next: (res: any) => {
        const data = res.data?.Data || res.data?.data || res.Data || res.data || res;
        const rawCities = data?.City || data?.city || [];
        this.cities = rawCities.map((c: any) => ({
          cityCode: String(c.cityCode || c.CityCode).trim(),
          cityName: c.cityName || c.CityName
        }));
      }
    });
  }

  onCityChange() {
    this.BankDistrictCode = '';
    this.districts = [];
    this.branches = []; 

    if (this.selectedCityCode) {
      const formattedCityCode = String(this.selectedCityCode).padStart(2, '0');
      
      this.vakifbankService.getDistricts(formattedCityCode).subscribe({
        next: (res: any) => {
          const data = res.data?.Data || res.data?.data || res.Data || res.data || res;
          const rawDistricts = data?.District || data?.district || [];
          this.districts = rawDistricts.map((d: any) => ({
            districtCode: String(d.BankDistrictCode || d.bankDistrictCode).trim(),
            districtName: d.districtName || d.DistrictName
          }));
        }
      });
    }
  }

  onDistrictChange() {
    this.branches = [];
    if (this.selectedCityCode && this.BankDistrictCode) {
      this.isLoadingBranches.set(true);
      
      const city = String(this.selectedCityCode).padStart(2, '0');
      const district = String(this.BankDistrictCode);

      this.vakifbankService.getBranches(city, district).subscribe({
        next: (res: any) => {
          const data = res.data?.Data || res.data?.data || res.Data || res.data || res;
          this.branches = data?.Branch || data?.branch || [];
          this.isLoadingBranches.set(false);
        },
        error: (err) => {
          console.warn("Şube bulunamadı:", err);
          this.branches = [];
          this.isLoadingBranches.set(false);
        }
      });
    }
  }

  findNearestByLocation() {
    this.locationError = '';
    this.nearestList = [];

    if (navigator.geolocation) {
      this.isLocating.set(true);
      
      navigator.geolocation.getCurrentPosition(
        (position) => {
          const lat = position.coords.latitude.toFixed(6).replace('.', ',');
          const lng = position.coords.longitude.toFixed(6).replace('.', ',');
          
          this.vakifbankService.getNearestBranchAndAtm(lat, lng, 5).subscribe({
            next: (res: any) => {
              const data = res.data?.Data || res.data?.data || res.Data || res.data || res;
              this.nearestList = data?.BranchandATM || data?.branchandATM || [];
              this.isLocating.set(false);
            },
            error: (err) => {
              console.error('Yakın şubeler bulunamadı:', err);
              this.locationError = 'Şubeler getirilirken bir hata oluştu.';
              this.isLocating.set(false);
            }
          });
        },
        (error) => {
          console.warn('Konum izni hatası:', error);
          this.locationError = 'Konumunuz alınamadı. Lütfen tarayıcı ayarlarından konum erişimine izin verin.';
          this.isLocating.set(false);
        }
      );
    } else {
      this.locationError = 'Tarayıcınız konum özelliğini desteklemiyor.';
    }
  }

  getMapsUrl(lat: string, lng: string): string {
    if (!lat || !lng) return '#';

    const formattedLat = String(lat).replace(',', '.');
    const formattedLng = String(lng).replace(',', '.');
    
    return `https://www.google.com/maps/dir/?api=1&destination=${formattedLat},${formattedLng}`;
  }
}