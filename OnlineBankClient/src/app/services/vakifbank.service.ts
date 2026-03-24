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
}