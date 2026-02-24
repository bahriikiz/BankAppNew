import { Component, OnInit, Inject, PLATFORM_ID, ChangeDetectorRef } from '@angular/core'; 
import { isPlatformBrowser, CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink } from '@angular/router'; 
import { ExchangeService } from './services/exchanges.service';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  // LoginComponent kaldırıldı, RouterLink eklendi!
  imports: [CommonModule, RouterOutlet, RouterLink], 
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class AppComponent implements OnInit {
  rates: any[] = [];
  lastUpdated: string = '';

  constructor(
    public readonly authService: AuthService,
    private readonly exchangeService: ExchangeService,
    @Inject(PLATFORM_ID) private readonly platformId: any,
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
        this.rates = res.rates;
        this.lastUpdated = res.lastUpdated;
        this.cdr.detectChanges(); 
      },
      error: (err: any) => console.error('Kurlar çekilemedi:', err)
    });
  }
}