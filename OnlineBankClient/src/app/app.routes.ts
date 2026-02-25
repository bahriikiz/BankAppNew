import { Routes } from '@angular/router';
import { HomeComponent } from './components/home/home.component';
import { LoginComponent } from './components/login/login.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { ProfileComponent } from './components/profile/profile.component';

export const routes: Routes = [
  // Herkesi karşılayan ana sayfa
  { path: '', component: HomeComponent, pathMatch: 'full' }, 
  
  // Giriş ve Kayıt sayfaları
  { path: 'login', component: LoginComponent },
  { path: 'register', component: LoginComponent }, 
  
  // SADECE GİRİŞ YAPANLAR İÇİN: Dashboard ve Profil
  { path: 'dashboard', component: DashboardComponent },
  { path: 'profile', component: ProfileComponent },
  
  // Yanlış URL'leri ana sayfaya yolla
  { path: '**', redirectTo: '' }
];