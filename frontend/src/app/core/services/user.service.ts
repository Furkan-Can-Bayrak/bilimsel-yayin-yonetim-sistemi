import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreateUserRequest,
  CreateUserResult,
  RoleListItem,
  UserListItem,
} from '../models/user.model';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private readonly http = inject(HttpClient);
  private readonly usersUrl = `${environment.apiBaseUrl}/users`;
  private readonly rolesUrl = `${environment.apiBaseUrl}/roles`;

  getAll(): Observable<UserListItem[]> {
    return this.http.get<UserListItem[]>(this.usersUrl);
  }

  create(body: CreateUserRequest): Observable<CreateUserResult> {
    return this.http.post<CreateUserResult>(this.usersUrl, body);
  }

  getRoles(): Observable<RoleListItem[]> {
    return this.http.get<RoleListItem[]>(this.rolesUrl);
  }
}
