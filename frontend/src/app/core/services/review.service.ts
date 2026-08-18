import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  MyReviewListItem,
  ReviewDetail,
  ReviewerCandidate,
  SubmitReviewRequest,
} from '../models/review.model';

@Injectable({
  providedIn: 'root',
})
export class ReviewService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/reviews`;

  getCandidates(manuscriptId: number): Observable<ReviewerCandidate[]> {
    return this.http.get<ReviewerCandidate[]>(`${this.baseUrl}/candidates`, {
      params: { manuscriptId },
    });
  }

  getMine(): Observable<MyReviewListItem[]> {
    return this.http.get<MyReviewListItem[]>(`${this.baseUrl}/mine`);
  }

  getById(id: number): Observable<ReviewDetail> {
    return this.http.get<ReviewDetail>(`${this.baseUrl}/${id}`);
  }

  assign(manuscriptId: number, reviewerId: number): Observable<{ id: number }> {
    return this.http.post<{ id: number }>(this.baseUrl, { manuscriptId, reviewerId });
  }

  submit(id: number, body: SubmitReviewRequest): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/submit`, body);
  }
}
