import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  AdminManuscriptDetail,
  MANUSCRIPT_STATUS_LABELS,
  ReviewSummary,
} from '../../../core/models/manuscript.model';
import { REVIEW_RECOMMENDATION_LABELS } from '../../../core/models/review.model';
import { Permissions } from '../../../core/auth/permissions';
import { AuthService } from '../../../core/services/auth.service';
import { ManuscriptService } from '../../../core/services/manuscript.service';
import { ReviewService } from '../../../core/services/review.service';
import { ManuscriptBody } from '../../../shared/manuscript-body/manuscript-body';

@Component({
  selector: 'app-admin-manuscript-view',
  imports: [RouterLink, FormsModule, ManuscriptBody],
  templateUrl: './admin-manuscript-view.html',
  styleUrl: './admin-manuscript-view.css',
})
export class AdminManuscriptView implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly manuscriptsApi = inject(ManuscriptService);
  private readonly reviewsApi = inject(ReviewService);
  private readonly auth = inject(AuthService);

  readonly statusLabels = MANUSCRIPT_STATUS_LABELS;
  readonly recommendationLabels = REVIEW_RECOMMENDATION_LABELS;

  readonly manuscript = signal<AdminManuscriptDetail | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly busy = signal(false);
  readonly rejectOpen = signal(false);
  rejectReason = '';
  rejectError: string | null = null;

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) {
      this.error.set('Makale bulunamadı.');
      this.loading.set(false);
      return;
    }

    this.load(id);
  }

  get isOwn(): boolean {
    const item = this.manuscript();
    return item != null && this.auth.userId() === item.authorId;
  }

  get backLink(): string {
    return this.isOwn || !this.auth.hasPermission(Permissions.Manuscripts.ViewAll)
      ? '/admin/mine'
      : '/admin';
  }

  get canDecide(): boolean {
    const item = this.manuscript();
    return (
      this.auth.hasPermission(Permissions.Manuscripts.Decide) &&
      !this.isOwn &&
      (item?.status === 'Submitted' || item?.status === 'UnderReview')
    );
  }

  get canPublish(): boolean {
    return (
      this.auth.hasPermission(Permissions.Manuscripts.Publish) &&
      !this.isOwn &&
      this.manuscript()?.status === 'Accepted'
    );
  }

  get canUnpublish(): boolean {
    return (
      this.auth.hasPermission(Permissions.Manuscripts.Unpublish) &&
      !this.isOwn &&
      this.manuscript()?.status === 'Published'
    );
  }

  get canEdit(): boolean {
    const item = this.manuscript();
    return (
      this.auth.hasPermission(Permissions.Manuscripts.Update) &&
      this.isOwn &&
      (item?.status === 'Draft' || item?.status === 'Rejected')
    );
  }

  get canWithdrawAssignment(): boolean {
    return this.auth.hasPermission(Permissions.Reviews.Assign) && !this.isOwn;
  }

  accept(): void {
    this.run((id) => this.manuscriptsApi.accept(id), 'Kabul edilemedi.');
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
    const item = this.manuscript();
    const reason = this.rejectReason.trim();
    if (!item) {
      return;
    }

    if (!reason) {
      this.rejectError = 'Red gerekçesi yazın.';
      return;
    }

    this.busy.set(true);
    this.error.set(null);
    this.rejectError = null;
    this.manuscriptsApi.reject(item.id, reason).subscribe({
      next: () => {
        this.busy.set(false);
        this.closeReject();
        this.load(item.id);
      },
      error: (err: unknown) => {
        this.busy.set(false);
        const detail = (err as { error?: { detail?: string } })?.error?.detail;
        this.rejectError = detail ?? 'Reddedilemedi.';
      },
    });
  }

  withdrawReview(review: ReviewSummary): void {
    const item = this.manuscript();
    if (!item || review.submittedAtUtc) {
      return;
    }

    if (!confirm('Bu hakem atamasını geri almak istediğinize emin misiniz?')) {
      return;
    }

    this.busy.set(true);
    this.error.set(null);
    this.reviewsApi.withdraw(review.id).subscribe({
      next: () => {
        this.busy.set(false);
        this.load(item.id);
      },
      error: (err: unknown) => {
        this.busy.set(false);
        const detail = (err as { error?: { detail?: string } })?.error?.detail;
        this.error.set(detail ?? 'Atama geri alınamadı.');
      },
    });
  }

  publish(): void {
    this.run((id) => this.manuscriptsApi.publish(id), 'Yayınlanamadı.');
  }

  unpublish(): void {
    this.run((id) => this.manuscriptsApi.unpublish(id), 'Yayından alınamadı.');
  }

  private load(id: number): void {
    this.loading.set(true);
    this.manuscriptsApi.getAdminById(id).subscribe({
      next: (data) => {
        this.manuscript.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Makale bulunamadı veya görüntüleme yetkiniz yok.');
        this.loading.set(false);
      },
    });
  }

  private run(action: (id: number) => ReturnType<ManuscriptService['accept']>, failMessage: string): void {
    const item = this.manuscript();
    if (!item) {
      return;
    }

    this.busy.set(true);
    this.error.set(null);
    action(item.id).subscribe({
      next: () => {
        this.busy.set(false);
        this.load(item.id);
      },
      error: (err: unknown) => {
        this.busy.set(false);
        const detail = (err as { error?: { detail?: string } })?.error?.detail;
        this.error.set(detail ?? failMessage);
      },
    });
  }
}
