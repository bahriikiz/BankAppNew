import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { VakifbankService } from '../../services/vakifbank.service';

@Component({
  selector: 'app-bank-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './bank-list.component.html',
  styleUrl: './bank-list.component.css' 
})
export class BankListComponent implements OnInit {
  vakifbankService = inject(VakifbankService);

  banks: any[] = [];
  filteredBanks: any[] = [];
  searchTerm: string = '';
  isLoading = signal(true);

  ngOnInit() {
    this.loadBanks();
  }

  loadBanks() {
    this.vakifbankService.getBankList().subscribe({
      next: (res: any) => {
        const data = res.data?.Data || res.data?.data || res.Data || res.data || res;
        
        this.banks = data?.Banks || data?.banks || []; 
        this.filteredBanks = this.banks;
        
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error("Bankalar getirilemedi:", err);
        this.isLoading.set(false);
      }
    });
  }

  // Arama çubuğuna yazıldıkça çalışan filtre fonksiyonu
  filterBanks() {
    if (this.searchTerm) { 
      this.filteredBanks = this.banks.filter(b => 
        (b.bankName || b.BankName)?.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        (b.bankCode || b.BankCode)?.includes(this.searchTerm)
      );
    } else {
      this.filteredBanks = this.banks;
    }
  }
}