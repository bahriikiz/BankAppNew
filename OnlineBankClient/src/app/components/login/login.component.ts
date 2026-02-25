import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent implements OnInit {
  public isLoginMode = true;
  public isLoading = false;
  public errorMessage = '';
  public successMessage = '';

  public loginModel = { email: '', password: '' };
  public registerModel = { firstName: '', lastName: '', email: '', password: '' };

  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  // SAYFA YÜKLENDİĞİNDE ÇALIŞAN KISIM
  ngOnInit() {
    // Eğer adres çubuğunda 'register' yazıyorsa, Kayıt Ol modunu aktif et
    if (this.router.url.includes('register')) {
      this.isLoginMode = false;
    } else {
      this.isLoginMode = true;
    }
  }

  toggleMode() {
    this.isLoginMode = !this.isLoginMode;
    this.errorMessage = '';
    this.successMessage = '';
  }

  onLogin() {
    this.isLoading = true;
    this.errorMessage = '';
    this.authService.login(this.loginModel).subscribe({
      next: (res: any) => {
        const token = res.token || res.Token; 
        if (token) {
          try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            const fullName = payload.fullName || 'Misafir Kullanıcı';
            const nameParts = fullName.split(' ');
            const userInfo = { 
              name: nameParts[0] || 'M', 
              surname: nameParts.length > 1 ? nameParts[nameParts.length - 1] : 'K' 
            };
            this.authService.setSession(token, userInfo);
          } catch (e) {
            console.error("Token çözülemedi", e);
          }
          this.isLoading = false;
          this.router.navigate(['/dashboard']);
        } else {
          this.isLoading = false;
          this.errorMessage = "Sunucudan geçerli bir anahtar alınamadı.";
        }
      },
      error: (err: any) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'E-posta veya şifre hatalı!';
      }
    });
  }

  onRegister() {
    this.isLoading = true;
    this.errorMessage = '';
    this.authService.register(this.registerModel).subscribe({
      next: () => {
        this.successMessage = 'Hesabınız başarıyla oluşturuldu. Giriş yapabilirsiniz.';
        this.loginModel.email = this.registerModel.email;
        this.isLoginMode = true;
        this.isLoading = false;
      },
      error: (err: any) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Kayıt işlemi başarısız.';
      }
    });
  }
}