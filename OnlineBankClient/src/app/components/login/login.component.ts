import { Component, inject } from '@angular/core';
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
export class LoginComponent {
  public isLoginMode: boolean = true;
  public isLoading: boolean = false;
  public errorMessage: string = '';
  public successMessage: string = '';

  // Backend'in birebir beklediği property isimleri
  public loginModel = { email: '', password: '' };
  public registerModel = { firstName: '', lastName: '', email: '', password: '' };

  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  public toggleMode(): void {
    this.isLoginMode = !this.isLoginMode;
    this.errorMessage = '';
    this.successMessage = '';
  }

  public onLogin(): void {
    this.isLoading = true;
    this.errorMessage = '';
    
    this.authService.login(this.loginModel).subscribe({
      next: (res: any) => {
        localStorage.setItem('token', res.accessToken);
        
        // Token içinden kullanıcı adını çözüyoruz
        const payload = JSON.parse(atob(res.accessToken.split('.')[1]));
        const userInfo = { name: payload.Name, surname: payload.Surname };
        
        localStorage.setItem('user_info', JSON.stringify(userInfo));
        this.authService.currentUser.set(userInfo);

        this.isLoading = false;
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'E-posta veya şifre hatalı!';
      }
    });
  }

  public onRegister(): void {
    this.isLoading = true;
    this.errorMessage = '';
    
    this.authService.register(this.registerModel).subscribe({
      next: (res: any) => {
        this.successMessage = 'Hesabınız başarıyla oluşturuldu. Lütfen giriş yapınız.';
        
        // Kayıt başarılıysa, giriş ekranındaki E-postayı otomatik doldur
        this.loginModel.email = this.registerModel.email; 
        
        // Formu temizle
        this.registerModel = { firstName: '', lastName: '', email: '', password: '' };
        this.isLoginMode = true; 
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Kayıt işlemi sırasında bir hata oluştu.';
      }
    });
  }
}