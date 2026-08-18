import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AdminPostDetail,
  AdminPostListItem,
  CreatePostRequest,
  CreatePostResult,
  PagedResult,
  PostDetail,
  PostListItem,
  PostListQuery,
  UpdatePostRequest,
} from '../models/post.model';

@Injectable({
  providedIn: 'root',
})
export class PostService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/posts`;

  getPosts(query: PostListQuery = {}): Observable<PagedResult<PostListItem>> {
    return this.http.get<PagedResult<PostListItem>>(this.baseUrl, {
      params: this.toParams(query),
    });
  }

  getBySlug(slug: string): Observable<PostDetail> {
    return this.http.get<PostDetail>(`${this.baseUrl}/${slug}`);
  }

  /** Admin: taslaklar dahil — JWT gerekir */
  getAdminPosts(query: PostListQuery = {}): Observable<PagedResult<AdminPostListItem>> {
    return this.http.get<PagedResult<AdminPostListItem>>(`${this.baseUrl}/admin`, {
      params: this.toParams(query),
    });
  }

  getAdminById(id: number): Observable<AdminPostDetail> {
    return this.http.get<AdminPostDetail>(`${this.baseUrl}/admin/${id}`);
  }

  create(body: CreatePostRequest): Observable<CreatePostResult> {
    return this.http.post<CreatePostResult>(this.baseUrl, body);
  }

  update(id: number, body: UpdatePostRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, body);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  private toParams(query: PostListQuery): HttpParams {
    let params = new HttpParams()
      .set('page', String(query.page ?? 1))
      .set('pageSize', String(query.pageSize ?? 10));

    const search = query.search?.trim();
    if (search) {
      params = params.set('search', search);
    }
    if (query.categoryId != null) {
      params = params.set('categoryId', String(query.categoryId));
    }
    if (query.isPublished != null) {
      params = params.set('isPublished', String(query.isPublished));
    }

    return params;
  }
}
