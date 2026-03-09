import { Component, ElementRef, ViewChild, inject, signal, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AiService } from '../../services/ai.service';

interface ChatMessage { text: string; sender: 'user' | 'ai'; }

@Component({
  selector: 'app-ai-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './ai-chat.component.html',
  styleUrl: './ai-chat.component.css'
})
export class AiChatComponent {
  public isOpen = signal(false);
  public showTooltip = signal(true); 
  public isTyping = signal(false);
  public userInput = signal('');
  public messages = signal<ChatMessage[]>([
    { text: 'Merhaba! Ben İKİZ AI. Size krediler, döviz, borsa ve tüm finansal işlemlerinizde nasıl yardımcı olabilirim?', sender: 'ai' }
  ]);

  // YENİ: Botun ekrandaki konumunu tutan sinyaller (Açılma yönü için)
  public isTopHalf = signal(false);
  public isLeftHalf = signal(false);

  private readonly aiService = inject(AiService);
  @ViewChild('chatScroll') private readonly chatScrollContainer!: ElementRef;

  // --- YENİ: AKILLI SINIR VE KADRAN MOTORU ---
  public transformStyle = signal('translate3d(0px, 0px, 0)');
  private isDragging = false;
  private dragMoved = false;

  private currentX = 0;
  private currentY = 0;
  private initialX = 0;
  private initialY = 0;

  @HostListener('document:mousemove', ['$event'])
  @HostListener('document:touchmove', ['$event'])
  onDrag(event: MouseEvent | TouchEvent) {
    if (!this.isDragging) return;
    event.preventDefault(); 
    
    const clientX = event instanceof MouseEvent ? event.clientX : event.touches[0].clientX;
    const clientY = event instanceof MouseEvent ? event.clientY : event.touches[0].clientY;

    const deltaX = clientX - this.initialX;
    const deltaY = clientY - this.initialY;

    if (Math.abs(deltaX) > 2 || Math.abs(deltaY) > 2) {
      this.dragMoved = true;
      this.showTooltip.set(false); 
    }

    this.currentX += deltaX;
    this.currentY += deltaY;

    // EKRAN DIŞINA ÇIKMAYI ENGELLEYEN GÖRÜNMEZ DUVAR (CLAMPING)
    if (globalThis.window !== undefined) {
      const btnSize = 75;
      const margin = 40;

      // Maksimum gidebileceği mesafeler
      const maxRight = margin;
      const maxLeft = -(globalThis.window.innerWidth - btnSize - margin);
      const maxDown = margin;
      const maxUp = -(globalThis.window.innerHeight - btnSize - margin);

      // X ve Y koordinatlarını sınırların içine kilitle
      this.currentX = Math.max(maxLeft, Math.min(this.currentX, maxRight));
      this.currentY = Math.max(maxUp, Math.min(this.currentY, maxDown));

      // Botun ekranın hangi yarısında olduğunu bul (Yön hesaplaması)
      const absX = globalThis.window.innerWidth - margin - (btnSize / 2) + this.currentX;
      const absY = globalThis.window.innerHeight - margin - (btnSize / 2) + this.currentY;

      this.isTopHalf.set(absY < globalThis.window.innerHeight / 2);
      this.isLeftHalf.set(absX < globalThis.window.innerWidth / 2);
    }

    this.initialX = clientX;
    this.initialY = clientY;

    this.transformStyle.set(`translate3d(${this.currentX}px, ${this.currentY}px, 0)`);
  }

  @HostListener('document:mouseup')
  @HostListener('document:touchend')
  onDragEnd() {
    if (!this.isDragging) return;
    setTimeout(() => this.isDragging = false, 50); 
  }

  onDragStart(event: MouseEvent | TouchEvent) {
    this.initialX = event instanceof MouseEvent ? event.clientX : event.touches[0].clientX;
    this.initialY = event instanceof MouseEvent ? event.clientY : event.touches[0].clientY;
    this.isDragging = true;
    this.dragMoved = false;
  }
  // --- MOTOR BİTİŞ ---

  toggleChat() {
    if (this.dragMoved) {
      this.dragMoved = false;
      return; 
    }
    this.isOpen.set(!this.isOpen());
    this.showTooltip.set(false); 
  }

  sendMessage() {
    const text = this.userInput().trim();
    if (!text) return;

    this.messages.update(m => [...m, { text, sender: 'user' }]);
    this.userInput.set('');
    this.isTyping.set(true);
    this.scrollToBottom();

    this.aiService.askAI(text).subscribe({
      next: (res) => {
        this.messages.update(m => [...m, { text: res.message, sender: 'ai' }]);
        this.isTyping.set(false);
        this.scrollToBottom();
      },
      error: (err) => {
        this.messages.update(m => [...m, { text: 'Bağlantı hatası oluştu.', sender: 'ai' }]);
        this.isTyping.set(false);
        this.scrollToBottom();
      }
    });
  }

  private scrollToBottom() {
    setTimeout(() => {
      if (this.chatScrollContainer) {
        this.chatScrollContainer.nativeElement.scrollTop = this.chatScrollContainer.nativeElement.scrollHeight;
      }
    }, 100);
  }
}