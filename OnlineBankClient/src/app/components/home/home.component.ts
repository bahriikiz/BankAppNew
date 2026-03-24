import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms'; 
import { VakifbankService } from '../../services/vakifbank.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {
  vakifbankService = inject(VakifbankService);

  cities: any[] = [];
  districts: any[] = [];
  branches: any[] = [];

  selectedCityCode: string = '';
  BankDistrictCode: string = '';
  isLoadingBranches = signal(false);

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
                console.log("Eşleşen İlçeler (BankDistrictCode ile):", this.districts);
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
                console.error("Şube getirme hatası:", err);
                this.branches = [];
                this.isLoadingBranches.set(false);
            }
        });
    }
}
}