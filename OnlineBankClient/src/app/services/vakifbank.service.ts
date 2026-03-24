import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class VakifbankService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'https://localhost:7241/api/Vakifbank';

  getCities(): Observable<any> {
    return this.http.get(`${this.baseUrl}/cities`);
  }

  getDistricts(cityCode: string): Observable<any> {
    return this.http.get(`${this.baseUrl}/cities/${cityCode}/districts`);
  }

 getNeighborhoods(districtCode: string): Observable<any> {
    return this.http.get(`${this.baseUrl}/districts/${districtCode}/neighborhoods`);
  }

  getBanks(): Observable<any> {
    return this.http.get(`${this.baseUrl}/banks`);
  }

  getBranches(cityCode: string, bankDistrictCode: string): Observable<any> {
  return this.http.get(`${this.baseUrl}/cities/${cityCode}/districts/${bankDistrictCode}/branches`);
}

calculateDeposit(amount: number, currencyCode: string, depositType: number, campaignId: number, termDays: number): Observable<any> {
    return this.http.get(`${this.baseUrl}/deposit/calculate?amount=${amount}&currencyCode=${currencyCode}&depositType=${depositType}&campaignId=${campaignId}&termDays=${termDays}`);
  }

  getBankList(): Observable<any> {
    return this.http.get(`${this.baseUrl}/banks`); 
  }

  getNearestBranchAndAtm(latitude: string, longitude: string, distanceLimit: number = 5): Observable<any> {
    return this.http.get(`${this.baseUrl}/nearest?latitude=${latitude}&longitude=${longitude}&distanceLimit=${distanceLimit}`);
  }
}