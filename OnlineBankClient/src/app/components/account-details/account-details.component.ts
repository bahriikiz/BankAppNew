import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AccountService } from '../../services/account.service';

@Component({
  selector: 'app-account-details',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './account-details.component.html',
  styleUrl: './account-details.component.css'
})
export class AccountDetailsComponent implements OnInit {
  public accountDetails = signal<any>(null);
  public isLoading = signal(true);
  public errorMessage = signal('');

  private readonly route = inject(ActivatedRoute);
  private readonly accountService = inject(AccountService);

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.fetchDetails(id);
    } else {
      this.errorMessage.set('Hesap ID bulunamadı.');
      this.isLoading.set(false);
    }
  }

  fetchDetails(id: string) {
    this.isLoading.set(true);
    this.accountService.getAccountDetails(id).subscribe({
      next: (res) => {
        this.accountDetails.set(res);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error("Detaylar yüklenemedi", err);
        this.errorMessage.set('Hesap detayları yüklenirken bir hata oluştu.');
        this.isLoading.set(false);
      }
    });
  }
}