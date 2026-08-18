import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ResearchArea } from '../models/manuscript.model';

@Injectable({
  providedIn: 'root',
})
export class ResearchAreaService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/research-areas`;

  getAll(): Observable<ResearchArea[]> {
    return this.http.get<ResearchArea[]>(this.baseUrl);
  }
}
