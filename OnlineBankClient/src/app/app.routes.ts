import { Routes } from '@angular/router';
import { HomeComponent } from './components/home/home.component';
import { LoginComponent } from './components/login/login.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { ProfileComponent } from './components/profile/profile.component';

export const routes: Routes = [
  
  { path: '', component: HomeComponent, pathMatch: 'full' }, 
  
  // 2. Kimlik doğrulama işlemleri
  { path: 'login', component: LoginComponent },
  { path: 'register', component: LoginComponent }, 
  
  // 3. Müşteri işlemleri
  { path: 'dashboard', component: DashboardComponent },
  { path: 'profile', component: ProfileComponent },
  
  { path: '**', redirectTo: '' }
];