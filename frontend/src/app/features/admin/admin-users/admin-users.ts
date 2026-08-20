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
  readonly pageSizeOptions = [10, 25, 50] as const;

  readonly users = signal<UserListItem[]>([]);
  readonly roles = signal<RoleListItem[]>([]);
  readonly institutions = signal<InstitutionListItem[]>([]);
  readonly loading = signal(true);
  readonly submitting = signal(false);
  readonly modalOpen = signal(false);
  readonly error = signal<string | null>(null);
  readonly modalError = signal<string | null>(null);
  readonly invalidFields = signal<ReadonlySet<string>>(new Set());
  readonly busyUserId = signal<number | null>(null);
  readonly roleEditorUserId = signal<number | null>(null);
  readonly roleEditorError = signal<string | null>(null);
  roleEditorSelectedIds = new Set<number>();

  readonly page = signal(1);
  readonly pageSize = signal(10);
  readonly totalCount = signal(0);
  readonly totalPages = signal(0);
  readonly hasPrevious = signal(false);
  readonly hasNext = signal(false);

  firstName = '';
  lastName = '';
  academicTitle: AcademicTitleValue = AcademicTitle.Dr;
  orcid = '';
  institutionId: number | null = null;
  selectedRoleIds = new Set<number>();

  ngOnInit(): void {
    this.loadLookups();
    this.loadUsers();
  }

  emailPreview(): string {
    const institution = this.institutions().find((i) => i.id === this.institutionId);
    return buildEmailPreview(this.firstName, this.lastName, institution?.emailDomain);
  }

  openRoleEditor(user: UserListItem, event: MouseEvent): void {
    event.stopPropagation();
    if (!this.canManage || this.busyUserId() === user.id) {
      return;
    }

    this.roleEditorUserId.set(user.id);
    this.roleEditorSelectedIds = new Set(user.roleIds);
    this.roleEditorError.set(null);
  }

  closeRoleEditor(): void {
    if (this.busyUserId() != null) {
      return;
    }

    this.roleEditorUserId.set(null);
    this.roleEditorSelectedIds = new Set();
    this.roleEditorError.set(null);
  }

  isRoleEditorOpen(userId: number): boolean {
    return this.roleEditorUserId() === userId;
  }

  isEditorRoleSelected(roleId: number): boolean {
    return this.roleEditorSelectedIds.has(roleId);
  }

  toggleEditorRole(roleId: number, checked: boolean): void {
    if (checked) {
      this.roleEditorSelectedIds.add(roleId);
    } else {
      this.roleEditorSelectedIds.delete(roleId);
    }
  }

  saveRoleEditor(user: UserListItem): void {
    if (!this.canManage) {
      return;
    }

    const roleIds = [...this.roleEditorSelectedIds];
    if (roleIds.length === 0) {
      this.roleEditorError.set('En az bir rol seçilmelidir.');
      return;
    }

    const current = [...user.roleIds].sort((a, b) => a - b);
    const next = [...roleIds].sort((a, b) => a - b);
    if (current.length === next.length && current.every((id, i) => id === next[i])) {
      this.closeRoleEditor();
      return;
    }

    this.busyUserId.set(user.id);
    this.roleEditorError.set(null);
    this.error.set(null);

    this.api.updateRoles(user.id, roleIds).subscribe({
      next: () => {
        this.busyUserId.set(null);
        this.closeRoleEditor();
        this.loadUsers();
      },
      error: (err: unknown) => {
        this.busyUserId.set(null);
        this.roleEditorError.set(this.readError(err) ?? 'Rol güncellenemedi.');
      },
    });
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages()) {
      return;
    }

    this.page.set(page);
    this.loadUsers();
  }

  changePageSize(size: number): void {
    if (!(this.pageSizeOptions as readonly number[]).includes(size)) {
      return;
    }

    this.pageSize.set(size);
    this.page.set(1);
    this.loadUsers();
  }

  titleValue(value: UserListItem['academicTitle'] | string | number): AcademicTitleValue {
    if (typeof value === 'number') {
      const byNumber: Record<number, AcademicTitleValue> = {
        1: 'ProfDr',
        2: 'DocDr',
        3: 'DrOgrUyesi',
        4: 'OgrGor',
        5: 'ArsGor',
        6: 'Dr',
      };
      return byNumber[value] ?? AcademicTitle.Dr;
    }

    return AcademicTitleOptions.some((o) => o.value === value)
      ? (value as AcademicTitleValue)
      : AcademicTitle.Dr;
  }

  changeAcademicTitle(user: UserListItem, next: AcademicTitleValue): void {
    if (!this.canManage || this.busyUserId() === user.id) {
      return;
    }

    const current = this.titleValue(user.academicTitle);
    if (current === next) {
      return;
    }

    this.busyUserId.set(user.id);
    this.error.set(null);

    this.api.updateAcademicTitle(user.id, next).subscribe({
      next: () => {
        this.users.update((list) =>
          list.map((u) => (u.id === user.id ? { ...u, academicTitle: next } : u)),
        );
        this.busyUserId.set(null);
      },
      error: (err: unknown) => {
        this.busyUserId.set(null);
        this.error.set(this.readError(err) ?? 'Unvan güncellenemedi.');
      },
    });
  }

  toggleActive(user: UserListItem): void {
    if (!this.canManage || this.busyUserId() === user.id) {
      return;
    }

    const next = !user.isActive;
    this.busyUserId.set(user.id);
    this.error.set(null);

    this.api.updateActiveStatus(user.id, next).subscribe({
      next: () => {
        this.users.update((list) =>
          list.map((u) => (u.id === user.id ? { ...u, isActive: next } : u)),
        );
        this.busyUserId.set(null);
      },
      error: (err: unknown) => {
        this.busyUserId.set(null);
        this.error.set(this.readError(err) ?? 'Durum güncellenemedi.');
      },
    });
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
    this.loadUsers();
  }

  private loadUsers(): void {
    this.loading.set(true);
    this.error.set(null);

    this.api
      .getAll({
        page: this.page(),
        pageSize: this.pageSize(),
      })
      .subscribe({
        next: (data) => {
          this.users.set(data.items);
          this.totalCount.set(data.totalCount);
          this.totalPages.set(data.totalPages);
          this.hasPrevious.set(data.hasPrevious);
          this.hasNext.set(data.hasNext);
          this.page.set(data.page);
          this.pageSize.set(data.pageSize);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Kullanıcılar yüklenemedi.');
          this.loading.set(false);
        },
      });
  }

  private loadLookups(): void {
    this.api.getRoles().subscribe({
      next: (data) => this.roles.set(data),
      error: () => {
        if (this.canManage) {
          this.error.set('Roller yüklenemedi.');
        }
      },
    });

    if (this.canManage) {
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

    this.closeRoleEditor();
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
