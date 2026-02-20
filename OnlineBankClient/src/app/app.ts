// 1. ChangeDetectorRef'i import ediyoruz
import { Component, OnInit, Inject, PLATFORM_ID, ChangeDetectorRef } from '@angular/core'; 
import { isPlatformBrowser, CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { ExchangeService } from './services/exchanges.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class AppComponent implements OnInit {
  rates: any[] = [];
  lastUpdated: string = '';

  constructor(
    private readonly exchangeService: ExchangeService,
    @Inject(PLATFORM_ID) private readonly platformId: any,
    // 2. Değişiklik algılayıcıyı içeri alıyoruz
    private readonly cdr: ChangeDetectorRef 
  ) {}

  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.loadRates(); 
      setInterval(() => this.loadRates(), 30000); 
    }
  }

  loadRates() {
    this.exchangeService.getLiveRates().subscribe({
      next: (res: any) => {
        // Veriler geldi, diziyi doldurduk
        this.rates = res.rates;
        this.lastUpdated = res.lastUpdated;
        
        // 3. KRİTİK NOKTA: Angular'a "Veriler değişti, HTML'i hemen güncelle!" diyoruz.
        this.cdr.detectChanges(); 
      },
      error: (err: any) => console.error('Kurlar çekilemedi', err)
    });
  }
}