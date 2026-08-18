import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ResearchArea } from '../../../core/models/manuscript.model';
import { Permissions } from '../../../core/auth/permissions';
import { AuthService } from '../../../core/services/auth.service';
import { ResearchAreaService } from '../../../core/services/research-area.service';
import { ManuscriptService } from '../../../core/services/manuscript.service';

@Component({
  selector: 'app-admin-manuscript-form',
  imports: [ReactiveFormsModule, FormsModule, RouterLink],
  templateUrl: './admin-manuscript-form.html',
  styleUrl: './admin-manuscript-form.css',
})
export class AdminManuscriptForm implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly manuscriptsApi = inject(ManuscriptService);
  private readonly researchAreasApi = inject(ResearchAreaService);
  private readonly auth = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly researchAreas = signal<ResearchArea[]>([]);
  readonly loading = signal(true);
  readonly submitting = signal(false);
  readonly error = signal<string | null>(null);
  readonly editId = signal<number | null>(null);
  readonly isPublished = signal(false);
  readonly authorName = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    slug: [''],
    summary: [''],
    content: ['', Validators.required],
    researchAreaId: [0, [Validators.required, Validators.min(1)]],
  });

  get isEdit(): boolean {
    return this.editId() !== null;
  }

  get backLink(): string {
    return this.auth.hasAnyPermission(Permissions.Manuscripts.ViewAll, Permissions.Manuscripts.Create)
      ? '/admin'
      : '/';
  }

  get canPublish(): boolean {
    return this.auth.hasPermission(Permissions.Manuscripts.Publish);
  }

  get canUnpublish(): boolean {
    return this.auth.hasPermission(Permissions.Manuscripts.Unpublish);
  }

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    const id = idParam ? Number(idParam) : null;

    this.researchAreasApi.getAll().subscribe({
      next: (areas) => {
        this.researchAreas.set(areas);
        if (!id && areas.length > 0) {
          this.form.patchValue({ researchAreaId: areas[0].id });
        }

        if (id && !Number.isNaN(id)) {
          this.editId.set(id);
          this.loadManuscript(id);
        } else {
          this.loading.set(false);
        }
      },
      error: () => {
        this.error.set('Araştırma alanları yüklenemedi.');
        this.loading.set(false);
      },
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const body = {
      title: raw.title,
      content: raw.content,
      summary: raw.summary.trim() ? raw.summary.trim() : null,
      researchAreaId: raw.researchAreaId,
      slug: raw.slug.trim() ? raw.slug.trim() : null,
    };

    this.submitting.set(true);
    this.error.set(null);

    const id = this.editId();
    const onOk = () => {
      this.submitting.set(false);
      const next = this.auth.hasAnyPermission(Permissions.Manuscripts.ViewAll, Permissions.Manuscripts.Create)
        ? '/admin'
        : '/';
      void this.router.navigateByUrl(next);
    };
    const onErr = (err: unknown) => {
      this.submitting.set(false);
      this.error.set(this.readError(err) ?? 'Kayıt başarısız.');
    };

    if (id) {
      this.manuscriptsApi.update(id, body).subscribe({ next: onOk, error: onErr });
    } else {
      this.manuscriptsApi.create(body).subscribe({ next: onOk, error: onErr });
    }
  }

  publish(): void {
    const id = this.editId();
    if (!id) {
      return;
    }

    this.submitting.set(true);
    this.error.set(null);
    this.manuscriptsApi.publish(id).subscribe({
      next: () => {
        this.submitting.set(false);
        this.isPublished.set(true);
      },
      error: (err: unknown) => {
        this.submitting.set(false);
        this.error.set(this.readError(err) ?? 'Yayınlama başarısız.');
      },
    });
  }

  unpublish(): void {
    const id = this.editId();
    if (!id) {
      return;
    }

    this.submitting.set(true);
    this.error.set(null);
    this.manuscriptsApi.unpublish(id).subscribe({
      next: () => {
        this.submitting.set(false);
        this.isPublished.set(false);
      },
      error: (err: unknown) => {
        this.submitting.set(false);
        this.error.set(this.readError(err) ?? 'Yayından alma başarısız.');
      },
    });
  }

  private loadManuscript(id: number): void {
    this.manuscriptsApi.getAdminById(id).subscribe({
      next: (manuscript) => {
        this.form.patchValue({
          title: manuscript.title,
          slug: manuscript.slug,
          summary: manuscript.summary ?? '',
          content: manuscript.content,
          researchAreaId: manuscript.researchAreaId,
        });
        this.isPublished.set(manuscript.isPublished);
        this.authorName.set(manuscript.authorName);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Makale bulunamadı.');
        this.loading.set(false);
      },
    });
  }

  private readError(err: unknown): string | null {
    const e = err as { error?: { title?: string; detail?: string; errors?: Record<string, string[]> } };
    if (e?.error?.detail) {
      return e.error.detail;
    }
    if (e?.error?.title) {
      return e.error.title;
    }
    const errors = e?.error?.errors;
    if (errors) {
      return Object.values(errors).flat().join(' ');
    }
    return null;
  }
}
