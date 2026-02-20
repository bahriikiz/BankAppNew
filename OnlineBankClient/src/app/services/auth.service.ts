import { HttpClient } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = 'https://localhost:7241/api/Auth/Login';
  
  // Kullanıcı bilgisini saklayan sinyal
  currentUser = signal<{ name: string, surname: string } | null>(null);

  constructor(private readonly http: HttpClient) {
    if ( globalThis.window !== undefined) {
      const savedUser = localStorage.getItem('user_info');
      if (savedUser) {
        this.currentUser.set(JSON.parse(savedUser));
      }
    }
  }

  login(model: any) {
    return this.http.post(this.apiUrl, model);
  }

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user_info');
    this.currentUser.set(null);
  }
}