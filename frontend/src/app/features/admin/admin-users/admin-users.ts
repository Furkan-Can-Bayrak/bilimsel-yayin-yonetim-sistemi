import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Permissions } from '../../../core/auth/permissions';
import {
  AcademicTitle,
  AcademicTitleOptions,
  AcademicTitleValue,
  InstitutionListItem,
  RoleListItem,
  UserListItem,
  academicTitleLabel,
  buildEmailPreview,
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
  readonly institutions = signal<InstitutionListItem[]>([]);
  readonly loading = signal(true);
  readonly submitting = signal(false);
  readonly modalOpen = signal(false);
  readonly error = signal<string | null>(null);
  readonly modalError = signal<string | null>(null);
  readonly invalidFields = signal<ReadonlySet<string>>(new Set());

  firstName = '';
  lastName = '';
  academicTitle: AcademicTitleValue = AcademicTitle.Dr;
  orcid = '';
  institutionId: number | null = null;
  selectedRoleIds = new Set<number>();

  ngOnInit(): void {
    this.reload();
  }

  emailPreview(): string {
    const institution = this.institutions().find((i) => i.id === this.institutionId);
    return buildEmailPreview(this.firstName, this.lastName, institution?.emailDomain);
  }

  isInvalid(field: string): boolean {
    return this.invalidFields().has(field);
  }

  clearFieldError(field: string): void {
    if (!this.invalidFields().has(field)) {
      return;
    }

    const next = new Set(this.invalidFields());
    next.delete(field);
    this.invalidFields.set(next);
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

      this.api.getInstitutions().subscribe({
        next: (data) => this.institutions.set(data),
        error: () => this.error.set('Kurumlar yüklenemedi.'),
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

    if (this.selectedRoleIds.size > 0) {
      this.clearFieldError('roles');
    }
  }

  isRoleSelected(roleId: number): boolean {
    return this.selectedRoleIds.has(roleId);
  }

  create(): void {
    if (!this.canManage) {
      return;
    }

    const firstName = this.firstName.trim();
    const lastName = this.lastName.trim();
    const orcid = this.orcid.trim();
    const roleIds = [...this.selectedRoleIds];
    const selectedInstitutionId = this.institutionId;

    const missing: string[] = [];
    const invalid = new Set<string>();

    if (!firstName) {
      missing.push('Ad');
      invalid.add('firstName');
    }
    if (!lastName) {
      missing.push('Soyad');
      invalid.add('lastName');
    }
    if (selectedInstitutionId == null) {
      missing.push('Kurum');
      invalid.add('institutionId');
    }
    if (roleIds.length === 0) {
      missing.push('en az bir rol');
      invalid.add('roles');
    }

    this.invalidFields.set(invalid);

    if (missing.length > 0) {
      this.modalError.set(`${missing.join(', ')} zorunludur.`);
      return;
    }

    if (selectedInstitutionId == null) {
      return;
    }

    this.submitting.set(true);
    this.modalError.set(null);
    this.invalidFields.set(new Set());

    this.api
      .create({
        firstName,
        lastName,
        academicTitle: this.academicTitle,
        orcid: orcid || null,
        institutionId: selectedInstitutionId,
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
    this.firstName = '';
    this.lastName = '';
    this.academicTitle = AcademicTitle.Dr;
    this.orcid = '';
    this.institutionId = null;
    this.selectedRoleIds = new Set();
    this.invalidFields.set(new Set());
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
