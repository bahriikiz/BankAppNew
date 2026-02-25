import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Account } from '../models/account.model';
import { Observable, of } from 'rxjs';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private readonly apiUrl = 'https://localhost:7241/api/Accounts';
  
  // AuthService'i içeri çağırıyoruz ki token'ı alabilsin
  private readonly authService = inject(AuthService);

  constructor(private readonly http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    // localStorage yerine RAM'deki token'ı alıyoruz
    const token = this.authService.getToken() ?? '';
    return new HttpHeaders().set('Authorization', `Bearer ${token}`);
  }

  getAccounts(): Observable<Account[]> {
    // Oturum yoksa hiç istek atıp yorma
    if (!this.authService.isAuthenticated()) return of([]);
    return this.http.get<Account[]>(`${this.apiUrl}/get-my-accounts`, { headers: this.getHeaders() });
  }

  syncVakifbankAccounts(rizaNo: string): Observable<any> {
    if (!this.authService.isAuthenticated()) return of(null);
    return this.http.post(`${this.apiUrl}/sync-vakifbank`, { rizaNo }, { headers: this.getHeaders() });
  }
}