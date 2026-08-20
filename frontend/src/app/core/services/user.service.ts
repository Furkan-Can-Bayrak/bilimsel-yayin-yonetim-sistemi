import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/manuscript.model';
import {
  AcademicTitleValue,
  CreateUserRequest,
  CreateUserResult,
  InstitutionListItem,
  RoleListItem,
  UserListItem,
  UserListQuery,
} from '../models/user.model';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private readonly http = inject(HttpClient);
  private readonly usersUrl = `${environment.apiBaseUrl}/users`;
  private readonly rolesUrl = `${environment.apiBaseUrl}/roles`;
  private readonly institutionsUrl = `${environment.apiBaseUrl}/institutions`;

  getAll(query: UserListQuery = {}): Observable<PagedResult<UserListItem>> {
    const params = new HttpParams()
      .set('page', String(query.page ?? 1))
      .set('pageSize', String(query.pageSize ?? 10));

    return this.http.get<PagedResult<UserListItem>>(this.usersUrl, { params });
  }

  create(body: CreateUserRequest): Observable<CreateUserResult> {
    return this.http.post<CreateUserResult>(this.usersUrl, body);
  }

  updateRoles(userId: number, roleIds: number[]): Observable<void> {
    return this.http.put<void>(`${this.usersUrl}/${userId}/roles`, { roleIds });
  }

  updateAcademicTitle(userId: number, academicTitle: AcademicTitleValue): Observable<void> {
    return this.http.put<void>(`${this.usersUrl}/${userId}/academic-title`, { academicTitle });
  }

  updateActiveStatus(userId: number, isActive: boolean): Observable<void> {
    return this.http.put<void>(`${this.usersUrl}/${userId}/active`, { isActive });
  }

  getRoles(): Observable<RoleListItem[]> {
    return this.http.get<RoleListItem[]>(this.rolesUrl);
  }

  getInstitutions(): Observable<InstitutionListItem[]> {
    return this.http.get<InstitutionListItem[]>(this.institutionsUrl);
  }
}
