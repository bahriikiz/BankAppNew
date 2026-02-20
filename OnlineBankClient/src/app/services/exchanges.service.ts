import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ExchangeService {
  private readonly apiUrl = 'https://localhost:7241/api/Exchange/live-rates';

  constructor(private readonly http: HttpClient) { }

  getLiveRates(): Observable<any> {
    return this.http.get<any>(this.apiUrl);
  }
}