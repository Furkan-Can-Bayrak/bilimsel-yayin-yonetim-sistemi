import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { Permissions } from '../../../core/auth/permissions';
import { AppNotification } from '../../../core/models/notification.model';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ReviewService } from '../../../core/services/review.service';

@Component({
  selector: 'app-admin-notifications',
  imports: [DatePipe, RouterLink],
  templateUrl: './admin-notifications.html',
  styleUrl: './admin-notifications.css',
})
export class AdminNotifications implements OnInit {
  private readonly api = inject(NotificationService);
  private readonly auth = inject(AuthService);
  private readonly reviews = inject(ReviewService);
  private readonly router = inject(Router);

  readonly canViewAll = this.auth.hasPermission(Permissions.Manuscripts.ViewAll);
  readonly canSubmitReview = this.auth.hasPermission(Permissions.Reviews.Submit);

  readonly items = signal<AppNotification[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly busyId = signal<number | null>(null);

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.loading.set(true);
    this.error.set(null);

    this.api.getAll().subscribe({
      next: (data) => {
        this.items.set(data);
        this.api.syncUnreadFrom(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Bildirimler yüklenemedi.');
        this.loading.set(false);
      },
    });
  }

  hasTarget(item: AppNotification): boolean {
    return item.relatedReviewId != null || item.relatedManuscriptId != null;
  }

  openTarget(item: AppNotification): void {
    if (!this.hasTarget(item)) {
      return;
    }

    this.markRead(item);

    if (item.relatedReviewId && this.canSubmitReview) {
      void this.router.navigateByUrl(`/admin/reviews/${item.relatedReviewId}`);
      return;
    }

    if (this.canSubmitReview && item.relatedManuscriptId) {
      this.reviews.getMine().subscribe({
        next: (list) => {
          const review = list.find((r) => r.manuscriptId === item.relatedManuscriptId);
          if (review) {
            void this.router.navigateByUrl(`/admin/reviews/${review.id}`);
            return;
          }

          this.openManuscriptOrReviews(item.relatedManuscriptId);
        },
        error: () => this.openManuscriptOrReviews(item.relatedManuscriptId),
      });
      return;
    }

    this.openManuscriptOrReviews(item.relatedManuscriptId);
  }

  markRead(item: AppNotification, event?: Event): void {
    event?.stopPropagation();

    if (item.isRead) {
      return;
    }

    this.busyId.set(item.id);
    this.api.markRead(item.id).subscribe({
      next: () => {
        this.busyId.set(null);
        this.items.update((list) =>
          list.map((n) => (n.id === item.id ? { ...n, isRead: true } : n)),
        );
      },
      error: () => {
        this.busyId.set(null);
        this.error.set('Okundu işaretlenemedi.');
      },
    });
  }

  private openManuscriptOrReviews(manuscriptId: number | null): void {
    if (
      manuscriptId &&
      this.auth.hasAnyPermission(Permissions.Manuscripts.ViewAll, Permissions.Manuscripts.Create)
    ) {
      void this.router.navigateByUrl(`/admin/manuscripts/${manuscriptId}`);
      return;
    }

    if (this.canSubmitReview) {
      void this.router.navigateByUrl('/admin/reviews');
    }
  }
}
