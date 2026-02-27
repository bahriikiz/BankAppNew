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
  
  // Vakıfbank hesaplarını senkronize etme metodu
  syncVakifbankAccounts(rizaNo: string): Observable<any> {
    if (!this.authService.isAuthenticated()) return of(null);
    return this.http.post(`${this.apiUrl}/sync-vakifbank`, { rizaNo }, { headers: this.getHeaders() });
  }
  
  // Yeni İKİZ BANK Hesabı Açma İsteği
  createAccount(model: { accountName: string, currencyType: string }): Observable<any> {
    if (!this.authService.isAuthenticated()) return of(null);
    return this.http.post(`${this.apiUrl}/create-account`, model, { headers: this.getHeaders() });
  }

  // ID'ye Göre Hesap Detayı ve İşlem Geçmişi Getirme
  getAccountDetails(accountId: number | string): Observable<any> {
    if (!this.authService.isAuthenticated()) return of(null);
    return this.http.get(`${this.apiUrl}/${accountId}`, { headers: this.getHeaders() });
  }

  // TARİH FİLTRELİ HESAP HAREKETLERİNİ GETİRİR (get-activities)
  getAccountActivities(payload: { accountId: number, startDate?: string, endDate?: string }): Observable<any[]> {
    if (!this.authService.isAuthenticated()) return of([]);
    const baseUrl = 'https://localhost:7241/api';
    return this.http.post<any[]>(`${baseUrl}/Transactions/get-activities`, payload, { headers: this.getHeaders() });
  }

  // Para Transferi Metodu
  transferMoney(payload: any): Observable<any> {
    const baseUrl = 'https://localhost:7241/api';
    return this.http.post(`${baseUrl}/Transactions/transfer`, payload, { headers: this.getHeaders() });
  }

  // KAYITLI ALICILARI GETİR
  getBeneficiaries(): Observable<any> {
    const baseUrl = 'https://localhost:7241/api';
    return this.http.get(`${baseUrl}/Beneficiaries/GetAll`, { headers: this.getHeaders() });
  }

  // YENİ ALICI KAYDET
  createBeneficiary(payload: { name: string, iban: string }): Observable<any> {
    const baseUrl = 'https://localhost:7241/api';
    return this.http.post(`${baseUrl}/Beneficiaries/Create`, payload, { headers: this.getHeaders() });
  }

  downloadReceipt(accountId: number, transactionId: string, format: string = '2'): Observable<any> {
    const baseUrl = 'https://localhost:7241/api';
    return this.http.get(`${baseUrl}/Transactions/${accountId}/receipt/${transactionId}?format=${format}`, { headers: this.getHeaders() });
  }
}