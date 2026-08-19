import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import {
  MANUSCRIPT_STATUS_LABELS,
  ManuscriptStatus,
  ResearchArea,
  ReviewSummary,
} from '../../../core/models/manuscript.model';
import { REVIEW_RECOMMENDATION_LABELS, ReviewerCandidate } from '../../../core/models/review.model';
import { Permissions } from '../../../core/auth/permissions';
import { AuthService } from '../../../core/services/auth.service';
import { ResearchAreaService } from '../../../core/services/research-area.service';
import { ManuscriptService } from '../../../core/services/manuscript.service';
import { ReviewService } from '../../../core/services/review.service';

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
  private readonly reviewsApi = inject(ReviewService);
  private readonly auth = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly statusLabels = MANUSCRIPT_STATUS_LABELS;
  readonly recommendationLabels = REVIEW_RECOMMENDATION_LABELS;
  readonly researchAreas = signal<ResearchArea[]>([]);
  readonly loading = signal(true);
  readonly submitting = signal(false);
  readonly error = signal<string | null>(null);
  readonly editId = signal<number | null>(null);
  readonly status = signal<ManuscriptStatus>('Draft');
  readonly authorName = signal<string | null>(null);
  readonly authorId = signal<number | null>(null);
  readonly currentReview = signal<ReviewSummary | null>(null);
  readonly candidates = signal<ReviewerCandidate[]>([]);
  selectedReviewerId: number | null = null;

  readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
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

  get canEditContent(): boolean {
    if (!this.isEdit) {
      return true;
    }

    if (this.auth.hasPermission(Permissions.Manuscripts.ViewAll) &&
        this.auth.hasPermission(Permissions.Manuscripts.Update)) {
      return true;
    }

    return this.status() === 'Draft' || this.status() === 'Rejected';
  }

  get isOwn(): boolean {
    const authorId = this.authorId();
    return authorId != null && this.auth.userId() === authorId;
  }

  get canSubmitForReview(): boolean {
    if (!this.auth.hasPermission(Permissions.Manuscripts.Submit)) {
      return false;
    }

    if (!this.isEdit) {
      return true;
    }

    return this.isOwn && (this.status() === 'Draft' || this.status() === 'Rejected');
  }

  get canDecide(): boolean {
    return this.auth.hasPermission(Permissions.Manuscripts.Decide) &&
      (this.status() === 'Submitted' || this.status() === 'UnderReview');
  }

  get canAssign(): boolean {
    return this.auth.hasPermission(Permissions.Reviews.Assign) && this.status() === 'Submitted';
  }

  get canViewReviews(): boolean {
    return this.auth.hasPermission(Permissions.Reviews.ViewAll);
  }

  get canPublish(): boolean {
    return this.auth.hasPermission(Permissions.Manuscripts.Publish) && this.status() === 'Accepted';
  }

  get canUnpublish(): boolean {
    return this.auth.hasPermission(Permissions.Manuscripts.Unpublish) && this.status() === 'Published';
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
    this.save(false);
  }

  submitForReview(): void {
    this.save(true);
  }

  private save(submitForReview: boolean): void {
    if (!this.canEditContent || this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const body = {
      title: raw.title,
      content: raw.content,
      summary: raw.summary.trim() ? raw.summary.trim() : null,
      researchAreaId: raw.researchAreaId,
      submitForReview,
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

  accept(): void {
    this.runWorkflow(this.manuscriptsApi.accept.bind(this.manuscriptsApi), 'Kabul edildi', 'Accepted');
  }

  reject(): void {
    this.runWorkflow(this.manuscriptsApi.reject.bind(this.manuscriptsApi), 'Reddedildi', 'Rejected');
  }

  assignReviewer(): void {
    const id = this.editId();
    const reviewerId = this.selectedReviewerId;
    if (!id || !reviewerId) {
      this.error.set('Hakem seçin.');
      return;
    }

    this.submitting.set(true);
    this.error.set(null);
    this.reviewsApi.assign(id, reviewerId).subscribe({
      next: () => {
        this.submitting.set(false);
        this.status.set('UnderReview');
        this.applyLock();
        this.loadManuscript(id);
      },
      error: (err: unknown) => {
        this.submitting.set(false);
        this.error.set(this.readError(err) ?? 'Hakem atanamadı.');
      },
    });
  }

  publish(): void {
    this.runWorkflow(this.manuscriptsApi.publish.bind(this.manuscriptsApi), 'Yayınlandı', 'Published');
  }

  unpublish(): void {
    this.runWorkflow(this.manuscriptsApi.unpublish.bind(this.manuscriptsApi), 'Yayından alındı', 'Accepted');
  }

  private runWorkflow(
    action: (id: number) => Observable<void>,
    okFallback: string,
    nextStatus: ManuscriptStatus,
  ): void {
    const id = this.editId();
    if (!id) {
      return;
    }

    this.submitting.set(true);
    this.error.set(null);
    action(id).subscribe({
      next: () => {
        this.submitting.set(false);
        this.status.set(nextStatus);
        this.applyLock();
      },
      error: (err: unknown) => {
        this.submitting.set(false);
        this.error.set(this.readError(err) ?? okFallback + ' işlemi başarısız.');
      },
    });
  }

  private loadManuscript(id: number): void {
    this.manuscriptsApi.getAdminById(id).subscribe({
      next: (manuscript) => {
        this.form.patchValue({
          title: manuscript.title,
          summary: manuscript.summary ?? '',
          content: manuscript.content,
          researchAreaId: manuscript.researchAreaId,
        });
        this.status.set(manuscript.status);
        this.authorName.set(manuscript.authorName);
        this.authorId.set(manuscript.authorId);
        this.currentReview.set(manuscript.currentReview);
        this.applyLock();
        this.loading.set(false);
        if (this.canAssign) {
          this.reviewsApi.getCandidates(id).subscribe({
            next: (candidates) => this.candidates.set(candidates),
          });
        }
      },
      error: () => {
        this.error.set('Makale bulunamadı.');
        this.loading.set(false);
      },
    });
  }

  private applyLock(): void {
    if (this.canEditContent) {
      this.form.enable();
    } else {
      this.form.disable();
    }
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
