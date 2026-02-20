import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable, PLATFORM_ID, Inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Account } from '../models/account.model';
import { Observable, of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private readonly apiUrl = 'https://localhost:7241/api/Accounts/get-my-accounts';

  constructor(
    private readonly http: HttpClient,
    // SonarLint S2933: Değişmeyen üyeler 'readonly' olmalıdır
    @Inject(PLATFORM_ID) private readonly platformId: Object 
  ) {}

  getAccounts(): Observable<Account[]> {
    // Sadece tarayıcıdaysak (Browser) localStorage'a erişebiliriz
    if (isPlatformBrowser(this.platformId)) {
      const token = localStorage.getItem('token');
      const headers = new HttpHeaders().set('Authorization', `Bearer ${token ?? ''}`);
      
      return this.http.get<Account[]>(this.apiUrl, { headers });
    }
    
    // Sunucu (Node.js) tarafındaysak tarayıcıyı beklemek için boş dizi dönüyoruz
    return of([]);
  }
}