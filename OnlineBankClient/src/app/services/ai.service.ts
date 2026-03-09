import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AiService {
  private readonly apiUrl = 'https://localhost:7241/api/Ai';
  private readonly http = inject(HttpClient);

  askAI(message: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/ask`, { message });
  }
}