import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateResearchAreaResult, ResearchArea } from '../models/manuscript.model';

@Injectable({
  providedIn: 'root',
})
export class ResearchAreaService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/research-areas`;

  getAll(): Observable<ResearchArea[]> {
    return this.http.get<ResearchArea[]>(this.baseUrl);
  }

  create(name: string): Observable<CreateResearchAreaResult> {
    return this.http.post<CreateResearchAreaResult>(this.baseUrl, { name });
  }

  update(id: number, name: string): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, { name });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
