import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router'; 
import { AccountService } from '../../services/account.service';
import { AuthService } from '../../services/auth.service'; 
import { Account } from '../../models/account.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  accounts = signal<Account[]>([]);
  isLoading = signal<boolean>(true);

  constructor(
    private readonly accountService: AccountService,
    private readonly authService: AuthService,
    private readonly router: Router
  ) {}

  ngOnInit() {
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/']);
      return; 
    }


    this.fetchAccounts();
  }

  fetchAccounts() {
    this.accountService.getAccounts().subscribe({
      next: (res) => {
        this.accounts.set(res);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error("Hesaplar yüklenirken hata oluştu:", err);
        this.isLoading.set(false);
      }
    });
  }
}