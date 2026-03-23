import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { VakifbankService } from '../../services/vakifbank.service';

@Component({
  selector: 'app-bank-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './bank-list.component.html',
  styleUrl: './bank-list.component.css'
})
export class BankListComponent implements OnInit {
  vakifbankService = inject(VakifbankService);

  // --- Banka Listesi ---
  banks: any[] = [];
  isLoadingBanks = signal(true);

  // --- Şube Arama Şelalesi ---
  cities: any[] = [];
  districts: any[] = [];
  branches: any[] = [];

  selectedCityCode: string = '';
  selectedDistrictCode: string = '';
  isLoadingBranches = signal(false);

  ngOnInit() {
    this.loadBanks();
    this.loadCities();
  }

  loadBanks() {
    this.vakifbankService.getBanks().subscribe({
      next: (res: any) => {
        const data = res.data?.Data || res.data?.data || res.Data || res.data || res;
        this.banks = data?.Bank || data?.bank || [];
        this.isLoadingBanks.set(false);
      },
      error: (err) => {
        console.error("Banka listesi çekilemedi", err);
        this.isLoadingBanks.set(false);
      }
    });
  }

  loadCities() {
    this.vakifbankService.getCities().subscribe({
      next: (res: any) => {
        const data = res.data?.Data || res.data?.data || res.Data || res.data || res;
        const rawCities = data?.City || data?.city || [];
        this.cities = rawCities.map((c: any) => ({
          cityCode: String(c.cityCode || c.CityCode).padStart(2, '0'), // Güvenlik 1: Sıfır ekleme
          cityName: c.cityName || c.CityName
        }));
      }
    });
  }

  onCityChange() {
    this.selectedDistrictCode = '';
    this.districts = [];
    this.branches = []; // Şehir değişirse şubeleri temizle

    if (this.selectedCityCode) {
      this.vakifbankService.getDistricts(this.selectedCityCode).subscribe({
        next: (res: any) => {
          const data = res.data?.Data || res.data?.data || res.Data || res.data || res;
          const rawDistricts = data?.District || data?.district || [];
          this.districts = rawDistricts.map((d: any) => ({
            districtCode: String(d.districtCode || d.DistrictCode),
            districtName: d.districtName || d.DistrictName
          }));
        }
      });
    }
  }

  onDistrictChange() {
    this.branches = [];
    if (this.selectedCityCode && this.selectedDistrictCode) {
      this.isLoadingBranches.set(true);
      this.vakifbankService.getBranches(this.selectedCityCode, this.selectedDistrictCode).subscribe({
        next: (res: any) => {
          const data = res.data?.Data || res.data?.data || res.Data || res.data || res;
          this.branches = data?.Branch || data?.branch || [];
          this.isLoadingBranches.set(false);
        },
        error: (err) => {
          console.error("Şubeler çekilemedi", err);
          this.isLoadingBranches.set(false);
        }
      });
    }
  }
}