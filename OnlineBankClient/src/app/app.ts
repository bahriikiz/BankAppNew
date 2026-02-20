import { Component, OnInit, Inject, PLATFORM_ID, ChangeDetectorRef } from '@angular/core'; 
import { isPlatformBrowser, CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { ExchangeService } from './services/exchanges.service';
import { LoginComponent } from './components/login/login.component';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,

  imports: [CommonModule, RouterOutlet, LoginComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class AppComponent implements OnInit {
  rates: any[] = [];
  lastUpdated: string = '';

  constructor(
    public readonly authService: AuthService, // public yaptık!
    private readonly exchangeService: ExchangeService,
    @Inject(PLATFORM_ID) private readonly platformId: any,
    private readonly cdr: ChangeDetectorRef 
  ) {}

  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.loadRates(); 
      // Kurları her 30 saniyede bir güncelle
      setInterval(() => this.loadRates(), 30000); 
    }
  }

  loadRates() {
    this.exchangeService.getLiveRates().subscribe({
      next: (res: any) => {
        this.rates = res.rates;
        this.lastUpdated = res.lastUpdated;
        
        // Zoneless modda veya asenkron işlemlerde arayüzü zorla güncellemek için:
        this.cdr.detectChanges(); 
      },
      error: (err: any) => console.error('Kurlar çekilemedi:', err)
    });
  }
}