import { HttpClient } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = 'https://localhost:7241/api/Auth';
  
  // Artık Token'ı değişkende bile tutmuyoruz, her şey Cookie'de güvenle saklanıyor!
  currentUser = signal<{ name: string, surname: string } | null>(null);

  constructor(private readonly http: HttpClient) {}

  login(model: any): Observable<any> {
    // API'den dönen response içinde artık token ile ilgilenmiyoruz, çerezler (Cookie) otomatik setleniyor
    return this.http.post(`${this.apiUrl}/login`, model);
  }

  register(model: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, model);
  }

  // Giriş başarılı olduğunda artık sadece kullanıcı adını ekrana basmak için tutuyoruz
  setSession(userInfo: { name: string, surname: string }) {
    this.currentUser.set(userInfo);
  }

  // Tarayıcıdaki Cookie'ler Backend tarafında silinecek, biz sadece kendi sinyalimizi temizliyoruz.
  logout() {
    this.currentUser.set(null);
    return this.http.get(`${this.apiUrl}/logout`, { withCredentials: true });
  }

  // (Çünkü artık token'ı biz JavaScript ile göremiyoruz, sadece sunucu görebilir)
  isAuthenticated(): boolean {
    return this.currentUser() !== null; 
  }

  // Tarayıcı bunu HttpOnly Cookie ile otomatik ve güvenli şekilde (XSS Korumalı) gönderir.

  getProfile(): Observable<any> {
    return this.http.get(`${this.apiUrl}/my-profile`, { withCredentials: true });
  }

  updateProfile(data: { phoneNumber: string, city: string, district: string, neighborhood: string, address: string }): Observable<any> {
   return this.http.put(`${this.apiUrl}/update-profile`, data);
  }

  changePassword(data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/change-password`, data);
  }
}