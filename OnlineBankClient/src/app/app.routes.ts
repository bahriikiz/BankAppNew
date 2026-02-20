import { Routes } from '@angular/router';
import { DashboardComponent } from './components/dashboard/dashboard.component';

export const routes: Routes = [
  // Ana sayfa açıldığında doğrudan Dashboard yüklensin
  { path: '', component: DashboardComponent, pathMatch: 'full' }, 
  
  { path: '**', redirectTo: '' }  // Bu satır her zaman en altta olmalıdır!
];