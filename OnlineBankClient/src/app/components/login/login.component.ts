import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms'; 
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  model = {
    email: '',
    password: ''
  };

  isLoading = signal<boolean>(false);
  errorMessage = signal<string>('');

  constructor(
    private readonly authService: AuthService,
    private readonly router: Router
  ) {}

  onLogin() {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.authService.login(this.model).subscribe({
      next: (res: any) => {
        // HATA AYIKLAMA: Backend'den tam olarak ne geldiğini burada gör
        console.log("Backend Yanıtı:", res);

        // 1. Token'ı kaydet
        localStorage.setItem('token', res.token);
        
        // 2. İsim ve Soyisim verisini güvenli bir şekilde çek (Her ihtimali deniyoruz)
        // res.user?.firstName -> Nesne içindeyse
        // res.firstName -> Doğrudan içindeyse
        const name = res.user?.firstName || res.firstName || res.user?.FirstName || res.FirstName;
        const surname = res.user?.lastName || res.lastName || res.user?.LastName || res.LastName;

        const userInfo = { 
          name: name || 'Bilinmeyen', 
          surname: surname || 'Kullanıcı' 
        };
        
        localStorage.setItem('user_info', JSON.stringify(userInfo));
        
        // 3. Servisteki sinyali güncelle
        this.authService.currentUser.set(userInfo);
        
        this.isLoading.set(false);
        
        // 4. Modal'ı Kapat
        this.closeModal();
        
        console.log("Giriş Başarılı. Hoş geldin:", userInfo.name);
        
        // 5. Başarılı girişte ana sayfaya veya dashboard'a yönlendir
        this.router.navigateByUrl('/');
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorMessage.set('E-posta veya şifre hatalı!');
        console.error("Giriş hatası detayları:", err);
      }
    });
  }

  // Modal kapatma işlemini ayrı bir yere aldık (Daha temiz kod)
  private closeModal() {
    const modalElement = document.getElementById('loginModal');
    if (modalElement) {
      const closeButton = modalElement.querySelector('.btn-close') as HTMLElement;
      closeButton?.click();
      
      // Bootstrap'in bazen arkada bıraktığı koyu gölgeyi (backdrop) temizleyelim
      const backdrop = document.querySelector('.modal-backdrop');
      backdrop?.remove();
      document.body.style.overflow = 'auto';
      document.body.classList.remove('modal-open');
    }
  }
}