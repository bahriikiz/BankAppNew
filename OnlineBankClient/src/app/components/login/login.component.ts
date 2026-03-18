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
  
  // YENİ: Kayıt modelini tüm detaylarıyla genişlettik
  public registerModel = { 
    firstName: '', lastName: '', email: '', password: '',
    identityNumber: '', phoneNumber: '',
    cityCode: '', cityName: '',
    districtCode: '', districtName: '',
    neighborhoodCode: '', neighborhoodName: '',
    address: '' 
  };

  // Şelale dropdownlar için listelerimiz
  public cities: any[] = [];
  public districts: any[] = [];
  public neighborhoods: any[] = [];

  private readonly authService = inject(AuthService);
  private readonly vakifbankService = inject(VakifbankService);
  private readonly router = inject(Router);

  ngOnInit() {
    this.isLoginMode = !this.router.url.includes('register');
    this.loadCities(); // Ekran açılır açılmaz İlleri getir!
  }

  toggleMode() {
    this.isLoginMode = !this.isLoginMode;
    this.errorMessage = '';
    this.successMessage = '';
  }

  // Dropdown motoru
  loadCities() {
    this.vakifbankService.getCities().subscribe({
      next: (res: any) => this.cities = res.data?.city || []
    });
  }

  onCityChange() {
    // İl değişirse altındaki her şeyi temizle!
    this.registerModel.districtCode = '';
    this.registerModel.neighborhoodCode = '';
    this.districts = [];
    this.neighborhoods = [];

   const selected = this.cities.find(c => c.cityCode === this.registerModel.cityCode);
    this.registerModel.cityName = selected ? selected.cityName : '';

    if (this.registerModel.cityCode) {
      this.vakifbankService.getDistricts(this.registerModel.cityCode).subscribe({
        next: (res: any) => {
          this.districts = res.Data?.District || res.data?.district || [];
        }
      });
    }
  }

  onDistrictChange() {
    this.registerModel.neighborhoodCode = '';
    this.neighborhoods = [];

    const selected = this.districts.find(d => d.DistrictCode === this.registerModel.districtCode);
    this.registerModel.districtName = selected ? selected.DistrictName : '';

    if (this.registerModel.cityCode && this.registerModel.districtCode) {
      this.vakifbankService.getNeighborhoods(this.registerModel.cityCode, this.registerModel.districtCode).subscribe({
        next: (res: any) => this.neighborhoods = res.data?.neighborhood || []
      });
    }
  }

  onNeighborhoodChange() {
    const selected = this.neighborhoods.find(n => n.NeighborhoodCode === this.registerModel.neighborhoodCode);
    this.registerModel.neighborhoodName = selected ? selected.NeighborhoodName : '';
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
    
    // Backend'in beklediği DTO formatına (İsimler gidecek şekilde) çeviriyoruz
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