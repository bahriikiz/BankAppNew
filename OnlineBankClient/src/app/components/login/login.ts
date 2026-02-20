import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms'; 
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule], 
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'    
})
export class LoginComponent { // Sınıf adının LoginComponent olduğuna dikkat et
  
  // 1. 'model does not exist' hatasını çözen değişken
  model = {
    email: '',     
    password: ''
  };

  // 2. 'isLoading does not exist' hatasını çözen değişkenler
  isLoading = signal<boolean>(false);
  errorMessage = signal<string>('');

  constructor(
    private readonly authService: AuthService,
    private readonly router: Router
  ) {}

  // 3. 'onLogin does not exist' hatasını çözen giriş fonksiyonu
  onLogin() {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.authService.login(this.model).subscribe({
      next: (res: any) => {
        // Giriş başarılıysa token'ı kaydet ve anasayfaya yönlendir
        localStorage.setItem('token', res.token);
        this.isLoading.set(false);
        this.router.navigateByUrl('/');
        console.log("Giriş Başarılı:", res);
      },
      error: (err: any) => {
        // Hata varsa ekrana yazdır
        this.isLoading.set(false);
        this.errorMessage.set('Kullanıcı adı veya şifre hatalı!');
        console.error('Giriş Hatası:', err);
      }
    });
  }
}