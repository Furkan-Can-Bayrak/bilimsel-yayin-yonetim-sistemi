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
  readonly reviews = signal<ReviewSummary[]>([]);
  readonly rejectionReason = signal<string | null>(null);
  readonly candidates = signal<ReviewerCandidate[]>([]);
  readonly assignOpen = signal(false);
  readonly rejectOpen = signal(false);
  rejectReason = '';
  rejectError: string | null = null;
  readonly candidatesLoading = signal(false);
  selectedReviewerId: number | null = null;

  readonly form = this.fb.group({
    title: ['', [Validators.maxLength(200)]],
    summary: ['', [Validators.maxLength(500)]],
    content: [''],
    researchAreaId: this.fb.control<number | null>(null),
  });

  readonly highlightedFields = signal<ReadonlySet<string>>(new Set());

  get isEdit(): boolean {
    return this.editId() !== null;
  }

  get backLink(): string {
    if (this.auth.hasPermission(Permissions.Manuscripts.Create)) {
      return '/admin/mine';
    }
    if (this.auth.hasPermission(Permissions.Manuscripts.ViewAll)) {
      return '/admin';
    }
    return '/';
  }

  get canEditContent(): boolean {
    if (!this.isEdit) {
      return true;
    }

    return this.isOwn && (this.status() === 'Draft' || this.status() === 'Rejected');
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
      !this.isOwn &&
      (this.status() === 'Submitted' || this.status() === 'UnderReview');
  }

  get canAssign(): boolean {
    if (!this.auth.hasPermission(Permissions.Reviews.Assign) || this.isOwn) {
      return false;
    }

    const status = this.status();
    if (status === 'Submitted') {
      return true;
    }

    if (status !== 'UnderReview') {
      return false;
    }

    const current = this.currentReview();
    return current == null || current.submittedAtUtc != null;
  }

  get assignLabel(): string {
    return this.reviews().length > 0 ? 'Başka hakem ata' : 'Hakem ata';
  }

  get canViewReviews(): boolean {
    return this.auth.hasPermission(Permissions.Reviews.ViewAll);
  }

  get canWithdrawAssignment(): boolean {
    return this.auth.hasPermission(Permissions.Reviews.Assign) && !this.isOwn;
  }

  get canPublish(): boolean {
    return this.auth.hasPermission(Permissions.Manuscripts.Publish) &&
      !this.isOwn &&
      this.status() === 'Accepted';
  }

  get canUnpublish(): boolean {
    return this.auth.hasPermission(Permissions.Manuscripts.Unpublish) &&
      !this.isOwn &&
      this.status() === 'Published';
  }

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    const id = idParam ? Number(idParam) : null;

    this.researchAreasApi.getAll().subscribe({
      next: (areas) => {
        this.researchAreas.set(areas);

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

  isInvalid(controlName: string): boolean {
    return this.highlightedFields().has(controlName);
  }

  clearFieldError(controlName: string): void {
    if (!this.highlightedFields().has(controlName)) {
      return;
    }

    const next = new Set(this.highlightedFields());
    next.delete(controlName);
    this.highlightedFields.set(next);
  }

  submit(): void {
    this.save(false);
  }

  submitForReview(): void {
    this.save(true);
  }

  private save(submitForReview: boolean): void {
    if (!this.canEditContent) {
      return;
    }

    const raw = this.form.getRawValue();
    const title = (raw.title ?? '').trim();
    const content = (raw.content ?? '').trim();
    const summary = (raw.summary ?? '').trim();
    const researchAreaId = raw.researchAreaId;

    if (submitForReview) {
      const missing = new Set<string>();
      if (!title) {
        missing.add('title');
      }
      if (!content) {
        missing.add('content');
      }
      if (researchAreaId == null) {
        missing.add('researchAreaId');
      }

      if (missing.size > 0) {
        this.highlightedFields.set(missing);
        this.error.set('Zorunlu alanları doldurun.');
        return;
      }
    } else {
      const hasAny =
        title.length > 0 ||
        content.length > 0 ||
        summary.length > 0 ||
        researchAreaId != null;

      if (!hasAny) {
        this.highlightedFields.set(new Set(['title', 'summary', 'content', 'researchAreaId']));
        this.error.set('Taslak için en az bir alan doldurun.');
        return;
      }
    }

    const body = {
      title,
      content: raw.content ?? '',
      summary: summary.length > 0 ? summary : null,
      researchAreaId,
      submitForReview,
    };

    this.submitting.set(true);
    this.error.set(null);
    this.highlightedFields.set(new Set());

    const id = this.editId();
    const onOk = () => {
      this.submitting.set(false);
      const next = this.backLink;
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

  openReject(): void {
    this.rejectOpen.set(true);
    this.rejectReason = '';
    this.rejectError = null;
  }

  closeReject(): void {
    this.rejectOpen.set(false);
    this.rejectReason = '';
    this.rejectError = null;
  }

  confirmReject(): void {
    const id = this.editId();
    const reason = this.rejectReason.trim();
    if (!id) {
      return;
    }

    if (!reason) {
      this.rejectError = 'Red gerekçesi yazın.';
      return;
    }

    this.submitting.set(true);
    this.error.set(null);
    this.rejectError = null;
    this.manuscriptsApi.reject(id, reason).subscribe({
      next: () => {
        this.submitting.set(false);
        this.status.set('Rejected');
        this.rejectionReason.set(reason);
        this.applyLock();
        this.closeReject();
      },
      error: (err: unknown) => {
        this.submitting.set(false);
        this.rejectError = this.readError(err) ?? 'Reddedilemedi.';
      },
    });
  }

  openAssign(): void {
    const id = this.editId();
    if (!id) {
      return;
    }

    this.assignOpen.set(true);
    this.selectedReviewerId = null;
    this.candidates.set([]);
    this.error.set(null);
    this.candidatesLoading.set(true);

    this.reviewsApi.getCandidates(id).subscribe({
      next: (candidates) => {
        this.candidates.set(candidates);
        this.candidatesLoading.set(false);
      },
      error: (err: unknown) => {
        this.candidatesLoading.set(false);
        this.error.set(this.readError(err) ?? 'Hakem listesi alınamadı.');
      },
    });
  }

  closeAssign(): void {
    this.assignOpen.set(false);
    this.selectedReviewerId = null;
  }

  withdrawReview(review: ReviewSummary): void {
    const id = this.editId();
    if (!id || review.submittedAtUtc) {
      return;
    }

    if (!confirm('Bu hakem atamasını geri almak istediğinize emin misiniz?')) {
      return;
    }

    this.submitting.set(true);
    this.error.set(null);
    this.reviewsApi.withdraw(review.id).subscribe({
      next: () => {
        this.submitting.set(false);
        this.loadManuscript(id);
      },
      error: (err: unknown) => {
        this.submitting.set(false);
        this.error.set(this.readError(err) ?? 'Atama geri alınamadı.');
        this.loadManuscript(id);
      },
    });
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
        this.closeAssign();
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
        this.reviews.set(manuscript.reviews ?? []);
        this.rejectionReason.set(manuscript.rejectionReason);
        this.applyLock();
        this.loading.set(false);
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
