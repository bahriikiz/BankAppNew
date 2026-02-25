import { HttpClient } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = 'https://localhost:7241/api/Auth';
  
  // Güvenlik: Token sadece RAM'de duracak. Sayfa yenilenirse (F5) uçup gidecek!
  private token: string | null = null;
  currentUser = signal<{ name: string, surname: string } | null>(null);

  constructor(private readonly http: HttpClient) {}

  login(model: any) {
    return this.http.post(`${this.apiUrl}/Login`, model);
  }

  register(model: any) {
    return this.http.post(`${this.apiUrl}/register`, model);
  }

  // Giriş başarılı olduğunda bu metot çalışır ve verileri geçici hafızaya alır
  setSession(token: string, userInfo: { name: string, surname: string }) {
    this.token = token;
    this.currentUser.set(userInfo);
  }

  // Diğer servislerin (AccountService gibi) token'a ulaşabilmesi için
  getToken(): string | null {
    return this.token;
  }

  isAuthenticated(): boolean {
    return this.token !== null;
  }

  logout() {
    this.token = null;
    this.currentUser.set(null);
  }
}