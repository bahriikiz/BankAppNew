import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { VakifbankService } from '../../services/vakifbank.service';

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
  
  public registerModel = { 
    firstName: '', lastName: '', email: '', password: '',
    identityNumber: '', phoneNumber: '',
    cityCode: '', cityName: '',
    districtCode: '', districtName: '',
    neighborhoodCode: '', neighborhoodName: '',
    address: '' 
  };

  public cities: any[] = [];
  public districts: any[] = [];
  public neighborhoods: any[] = [];

  private readonly authService = inject(AuthService);
  private readonly vakifbankService = inject(VakifbankService);
  private readonly router = inject(Router);

  ngOnInit() {
    this.isLoginMode = !this.router.url.includes('register');
    this.loadCities();
  }

  toggleMode() {
    this.isLoginMode = !this.isLoginMode;
    this.errorMessage = '';
    this.successMessage = '';
  }

  loadCities() {
    this.vakifbankService.getCities().subscribe({
      next: (res: any) => {
        // API'den gelen veri yapısına göre hem 'city' hem de 'City' kontrolü yapıyoruz
        const responseData = res.data?.Data || res.data?.data || res.Data || res.data || res;
        this.cities = responseData?.City || responseData?.city || [];
      }
    });
  }

  onCityChange() {
    this.registerModel.districtCode = '';
    this.registerModel.neighborhoodCode = '';
    this.districts = [];
    this.neighborhoods = [];

    // Hem cityCode hem CityCode ihtimalini kontrol et, == ile string/number farkını ez
    const selected = this.cities.find(c => (c.cityCode || c.CityCode) == this.registerModel.cityCode);
    this.registerModel.cityName = selected ? (selected.cityName || selected.CityName) : '';

    if (this.registerModel.cityCode) {

      const formattedCityCode = String(this.registerModel.cityCode).padStart(2, '0');
     this.vakifbankService.getDistricts(formattedCityCode).subscribe({
        next: (res: any) => {
          const responseData = res.data?.Data || res.data?.data || res.Data || res.data || res;
          this.districts = responseData?.District || responseData?.district || [];
        },
        error: (err: any) => console.error("İlçeler çekilirken hata:", err)
      });
    }
  }

  onDistrictChange() {
    this.registerModel.neighborhoodCode = '';
    this.neighborhoods = [];

    // Hem districtCode hem DistrictCode ihtimalini kontrol et, == ile string/number farkını ez
    const selected = this.districts.find(d => (d.districtCode || d.DistrictCode) == this.registerModel.districtCode);
    this.registerModel.districtName = selected ? (selected.districtName || selected.DistrictName) : '';

    // Mahalle servisi sadece districtCode bekliyor (Servisi önceki adımda böyle güncellemiştik)
    if (this.registerModel.districtCode) {
      this.vakifbankService.getNeighborhoods(this.registerModel.districtCode).subscribe({
        next: (res: any) => {
          // İç içe DTO (Data.Neighborhood) katmanlarını çözüyoruz
          const responseData = res.data?.Data || res.data?.data || res.Data || res.data || res;
          this.neighborhoods = responseData?.Neighborhood || responseData?.neighborhood || [];
        },
        error: (err: any) => console.error("Mahalleler çekilirken hata:", err)
      });
    }
  }

  onNeighborhoodChange() {
    // Hem neighborhoodCode hem NeighborhoodCode ihtimalini kontrol et, == ile string/number farkını ez
    const selected = this.neighborhoods.find(n => (n.neighborhoodCode || n.NeighborhoodCode) == this.registerModel.neighborhoodCode);
    this.registerModel.neighborhoodName = selected ? (selected.neighborhoodName || selected.NeighborhoodName) : '';
  }

  onLogin() {
    this.isLoading = true;
    this.errorMessage = '';
    
    this.authService.login(this.loginModel).subscribe({
      next: (res: any) => {
        const userInfo = { name: res.firstName || 'Değerli', surname: res.lastName || 'Müşterimiz' };
        this.authService.setSession(userInfo);
        this.isLoading = false;
        this.router.navigate(['/dashboard']);
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
    
    const payload = {
      firstName: this.registerModel.firstName,
      lastName: this.registerModel.lastName,
      email: this.registerModel.email,
      password: this.registerModel.password,
      identityNumber: this.registerModel.identityNumber,
      phoneNumber: this.registerModel.phoneNumber,
      city: this.registerModel.cityName, 
      district: this.registerModel.districtName,
      neighborhood: this.registerModel.neighborhoodName,
      address: this.registerModel.address
    };
    
    this.authService.register(payload).subscribe({
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