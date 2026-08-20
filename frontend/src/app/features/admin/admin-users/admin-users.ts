import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Permissions } from '../../../core/auth/permissions';
import {
  AcademicTitle,
  AcademicTitleOptions,
  AcademicTitleValue,
  RoleListItem,
  UserListItem,
  academicTitleLabel,
} from '../../../core/models/user.model';
import { AuthService } from '../../../core/services/auth.service';
import { UserService } from '../../../core/services/user.service';

@Component({
  selector: 'app-admin-users',
  imports: [FormsModule],
  templateUrl: './admin-users.html',
  styleUrl: './admin-users.css',
})
export class AdminUsers implements OnInit {
  private readonly api = inject(UserService);
  private readonly auth = inject(AuthService);

  readonly canManage = this.auth.hasPermission(Permissions.Users.Manage);
  readonly titleOptions = AcademicTitleOptions;
  readonly titleLabel = academicTitleLabel;

  readonly users = signal<UserListItem[]>([]);
  readonly roles = signal<RoleListItem[]>([]);
  readonly loading = signal(true);
  readonly submitting = signal(false);
  readonly modalOpen = signal(false);
  readonly error = signal<string | null>(null);
  readonly modalError = signal<string | null>(null);

  email = '';
  password = '';
  firstName = '';
  lastName = '';
  academicTitle: AcademicTitleValue = AcademicTitle.Dr;
  orcid = '';
  selectedRoleIds = new Set<number>();

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.loading.set(true);
    this.error.set(null);

    this.api.getAll().subscribe({
      next: (data) => {
        this.users.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Kullanıcılar yüklenemedi.');
        this.loading.set(false);
      },
    });

    if (this.canManage) {
      this.api.getRoles().subscribe({
        next: (data) => this.roles.set(data),
        error: () => this.error.set('Roller yüklenemedi.'),
      });
    }
  }

  openCreate(): void {
    if (!this.canManage) {
      return;
    }

    this.resetForm();
    this.modalError.set(null);
    this.modalOpen.set(true);
  }

  closeCreate(): void {
    if (this.submitting()) {
      return;
    }

    this.modalOpen.set(false);
    this.modalError.set(null);
    this.resetForm();
  }

  toggleRole(roleId: number, checked: boolean): void {
    if (checked) {
      this.selectedRoleIds.add(roleId);
    } else {
      this.selectedRoleIds.delete(roleId);
    }
  }

  isRoleSelected(roleId: number): boolean {
    return this.selectedRoleIds.has(roleId);
  }

  create(): void {
    if (!this.canManage) {
      return;
    }

    const email = this.email.trim();
    const password = this.password;
    const firstName = this.firstName.trim();
    const lastName = this.lastName.trim();
    const orcid = this.orcid.trim();
    const roleIds = [...this.selectedRoleIds];

    if (!email || !password || !firstName || !lastName || roleIds.length === 0) {
      this.modalError.set('E-posta, şifre, ad, soyad ve en az bir rol zorunludur.');
      return;
    }

    this.submitting.set(true);
    this.modalError.set(null);

    this.api
      .create({
        email,
        password,
        firstName,
        lastName,
        academicTitle: this.academicTitle,
        orcid: orcid || null,
        institutionId: null,
        roleIds,
      })
      .subscribe({
        next: () => {
          this.submitting.set(false);
          this.modalOpen.set(false);
          this.resetForm();
          this.reload();
        },
        error: (err: unknown) => {
          this.submitting.set(false);
          this.modalError.set(this.readError(err) ?? 'Kullanıcı eklenemedi.');
        },
      });
  }

  private resetForm(): void {
    this.email = '';
    this.password = '';
    this.firstName = '';
    this.lastName = '';
    this.academicTitle = AcademicTitle.Dr;
    this.orcid = '';
    this.selectedRoleIds = new Set();
  }

  private readError(err: unknown): string | null {
    const e = err as { error?: { title?: string; detail?: string; errors?: Record<string, string[]> } };
    if (e?.error?.detail) {
      return e.error.detail;
    }
    const errors = e?.error?.errors;
    if (errors) {
      return Object.values(errors).flat().join(' ');
    }
    if (e?.error?.title) {
      return e.error.title;
    }
    return null;
  }
}
