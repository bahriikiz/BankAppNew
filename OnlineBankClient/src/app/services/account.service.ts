import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Account } from '../models/account.model';
import { Observable, of } from 'rxjs';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private readonly apiUrl = 'https://localhost:7241/api/Accounts';
  
  // Sadece oturum kontrolü (isAuthenticated) yapmak için Auth servisi tutuyoruz
  private readonly authService = inject(AuthService);

  constructor(private readonly http: HttpClient) {}

  getAccounts(): Observable<Account[]> {
    if (!this.authService.isAuthenticated()) return of([]);
    return this.http.get<Account[]>(`${this.apiUrl}/get-my-accounts`);
  }
  
  syncVakifbankAccounts(rizaNo: string): Observable<any> {
    if (!this.authService.isAuthenticated()) return of(null);
    return this.http.post(`${this.apiUrl}/sync-vakifbank`, { rizaNo });
  }
  
  createAccount(model: { accountName: string, currencyType: string }): Observable<any> {
    if (!this.authService.isAuthenticated()) return of(null);
    return this.http.post(`${this.apiUrl}/create-account`, model);
  }

  getAccountDetails(accountId: number | string): Observable<any> {
    if (!this.authService.isAuthenticated()) return of(null);
    return this.http.get(`${this.apiUrl}/${accountId}`);
  }

  getAccountActivities(payload: { accountId: number, startDate?: string, endDate?: string }): Observable<any[]> {
    if (!this.authService.isAuthenticated()) return of([]);
    const baseUrl = 'https://localhost:7241/api';
    return this.http.post<any[]>(`${baseUrl}/Transactions/get-activities`, payload);
  }

  transferMoney(payload: any): Observable<any> {
    const baseUrl = 'https://localhost:7241/api';
    return this.http.post(`${baseUrl}/Transactions/transfer`, payload);
  }

  getBeneficiaries(): Observable<any> {
    const baseUrl = 'https://localhost:7241/api';
    return this.http.get(`${baseUrl}/Beneficiaries/GetAll`);
  }

  createBeneficiary(payload: { name: string, iban: string }): Observable<any> {
    const baseUrl = 'https://localhost:7241/api';
    return this.http.post(`${baseUrl}/Beneficiaries/Create`, payload);
  }

  downloadReceipt(accountId: number, transactionId: string, format: string = '2'): Observable<any> {
    const baseUrl = 'https://localhost:7241/api';
    return this.http.get(`${baseUrl}/Transactions/${accountId}/receipt/${transactionId}?format=${format}`);
  }
}