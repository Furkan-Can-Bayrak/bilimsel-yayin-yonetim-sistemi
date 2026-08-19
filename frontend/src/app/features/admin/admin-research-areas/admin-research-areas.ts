import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Permissions } from '../../../core/auth/permissions';
import { ResearchArea } from '../../../core/models/manuscript.model';
import { AuthService } from '../../../core/services/auth.service';
import { ResearchAreaService } from '../../../core/services/research-area.service';

@Component({
  selector: 'app-admin-research-areas',
  imports: [FormsModule, RouterLink],
  templateUrl: './admin-research-areas.html',
  styleUrl: './admin-research-areas.css',
})
export class AdminResearchAreas implements OnInit {
  private readonly api = inject(ResearchAreaService);
  private readonly auth = inject(AuthService);

  readonly canManage = this.auth.hasPermission(Permissions.ResearchAreas.Manage);
  readonly items = signal<ResearchArea[]>([]);
  readonly loading = signal(true);
  readonly submitting = signal(false);
  readonly error = signal<string | null>(null);
  readonly editingId = signal<number | null>(null);

  newName = '';
  editName = '';

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.getAll().subscribe({
      next: (data) => {
        this.items.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Araştırma alanları yüklenemedi.');
        this.loading.set(false);
      },
    });
  }

  create(): void {
    const name = this.newName.trim();
    if (!this.canManage || !name) {
      return;
    }

    this.submitting.set(true);
    this.error.set(null);
    this.api.create(name).subscribe({
      next: () => {
        this.newName = '';
        this.submitting.set(false);
        this.reload();
      },
      error: (err: unknown) => {
        this.submitting.set(false);
        this.error.set(this.readError(err) ?? 'Alan eklenemedi.');
      },
    });
  }

  startEdit(area: ResearchArea): void {
    if (!this.canManage) {
      return;
    }

    this.editingId.set(area.id);
    this.editName = area.name;
    this.error.set(null);
  }

  cancelEdit(): void {
    this.editingId.set(null);
    this.editName = '';
  }

  saveEdit(area: ResearchArea): void {
    const name = this.editName.trim();
    if (!this.canManage || !name) {
      return;
    }

    this.submitting.set(true);
    this.error.set(null);
    this.api.update(area.id, name).subscribe({
      next: () => {
        this.submitting.set(false);
        this.cancelEdit();
        this.reload();
      },
      error: (err: unknown) => {
        this.submitting.set(false);
        this.error.set(this.readError(err) ?? 'Alan güncellenemedi.');
      },
    });
  }

  remove(area: ResearchArea): void {
    if (!this.canManage) {
      return;
    }

    if (!confirm(`"${area.name}" silinsin mi?`)) {
      return;
    }

    this.submitting.set(true);
    this.error.set(null);
    this.api.delete(area.id).subscribe({
      next: () => {
        this.submitting.set(false);
        if (this.editingId() === area.id) {
          this.cancelEdit();
        }
        this.reload();
      },
      error: (err: unknown) => {
        this.submitting.set(false);
        this.error.set(this.readError(err) ?? 'Alan silinemedi.');
      },
    });
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
