import { Routes } from '@angular/router';
import { HomeComponent } from './components/home/home.component';
import { LoginComponent } from './components/login/login.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { ProfileComponent } from './components/profile/profile.component';
import { CreateAccountComponent } from './components/create-account/create-account.component';
import { AccountDetailsComponent } from './components/account-details/account-details.component';
import { MoneyTransferComponent } from './components/money-transfer/money-transfer.component';
import { BankListComponent } from './components/bank-list/bank-list.component';
import { DepositCalculatorComponent } from './components/deposit-calculator/deposit-calculator.component';
import { BranchLocatorComponent } from './components/branch-locator/branch-locator.component';

export const routes: Routes = [
  // Herkesi karşılayan ana sayfa
  { path: '', component: HomeComponent, pathMatch: 'full' }, 
  
  // Giriş ve Kayıt sayfaları
  { path: 'login', component: LoginComponent },
  { path: 'register', component: LoginComponent }, 
  
  // SADECE GİRİŞ YAPANLAR İÇİN: Dashboard, Profil ve Hesap Açma
  { path: 'dashboard', component: DashboardComponent },
  { path: 'profile', component: ProfileComponent },
  { path: 'create-account', component: CreateAccountComponent },
  
  { path: 'account/:id', component: AccountDetailsComponent }, // Hesap detayları için dinamik rota
  { path: 'transfer', component: MoneyTransferComponent }, // Para transferi sayfası
  
  { path: 'banks', component: BankListComponent }, // Bankalar 

  { path: 'branch-locator', component: BranchLocatorComponent }, //Banka Şubeleri

  { path: 'deposit-calculator', component: DepositCalculatorComponent }, // Mevduat Hesaplayıcı 


  { path: '**', redirectTo: '' }
];