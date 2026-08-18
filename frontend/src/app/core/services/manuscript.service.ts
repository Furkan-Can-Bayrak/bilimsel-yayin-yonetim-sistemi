import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AdminManuscriptDetail,
  AdminManuscriptListItem,
  CreateManuscriptRequest,
  CreateManuscriptResult,
  ManuscriptDetail,
  ManuscriptListItem,
  ManuscriptListQuery,
  PagedResult,
  UpdateManuscriptRequest,
} from '../models/manuscript.model';

@Injectable({
  providedIn: 'root',
})
export class ManuscriptService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/manuscripts`;

  getPublished(query: ManuscriptListQuery = {}): Observable<PagedResult<ManuscriptListItem>> {
    return this.http.get<PagedResult<ManuscriptListItem>>(this.baseUrl, {
      params: this.toParams(query),
    });
  }

  getBySlug(slug: string): Observable<ManuscriptDetail> {
    return this.http.get<ManuscriptDetail>(`${this.baseUrl}/${slug}`);
  }

  getAdminList(query: ManuscriptListQuery = {}): Observable<PagedResult<AdminManuscriptListItem>> {
    return this.http.get<PagedResult<AdminManuscriptListItem>>(`${this.baseUrl}/admin`, {
      params: this.toParams(query),
    });
  }

  getAdminById(id: number): Observable<AdminManuscriptDetail> {
    return this.http.get<AdminManuscriptDetail>(`${this.baseUrl}/admin/${id}`);
  }

  create(body: CreateManuscriptRequest): Observable<CreateManuscriptResult> {
    return this.http.post<CreateManuscriptResult>(this.baseUrl, body);
  }

  update(id: number, body: UpdateManuscriptRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, body);
  }

  publish(id: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/publish`, {});
  }

  unpublish(id: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/unpublish`, {});
  }

  submit(id: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/submit`, {});
  }

  accept(id: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/accept`, {});
  }

  reject(id: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/reject`, {});
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  private toParams(query: ManuscriptListQuery): HttpParams {
    let params = new HttpParams()
      .set('page', String(query.page ?? 1))
      .set('pageSize', String(query.pageSize ?? 10));

    const search = query.search?.trim();
    if (search) {
      params = params.set('search', search);
    }
    if (query.researchAreaId != null) {
      params = params.set('researchAreaId', String(query.researchAreaId));
    }
    if (query.status) {
      params = params.set('status', query.status);
    }

    return params;
  }
}
