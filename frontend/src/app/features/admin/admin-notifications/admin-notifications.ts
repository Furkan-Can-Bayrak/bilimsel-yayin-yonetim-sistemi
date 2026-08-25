import {
  Component,
  DestroyRef,
  ElementRef,
  OnInit,
  effect,
  inject,
  signal,
  untracked,
  viewChild,
} from '@angular/core';
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
  private static readonly pageSize = 10;

  private readonly api = inject(NotificationService);
  private readonly auth = inject(AuthService);
  private readonly reviews = inject(ReviewService);
  private readonly router = inject(Router);

  readonly canViewAll = this.auth.hasPermission(Permissions.Manuscripts.ViewAll);
  readonly canSubmitReview = this.auth.hasPermission(Permissions.Reviews.Submit);

  readonly items = signal<AppNotification[]>([]);
  readonly loading = signal(true);
  readonly loadingMore = signal(false);
  readonly hasNext = signal(false);
  readonly error = signal<string | null>(null);
  readonly loadError = signal(false);
  readonly busyId = signal<number | null>(null);

  private nextPage = 1;
  private observer: IntersectionObserver | null = null;
  private readonly loadSentinel = viewChild<ElementRef<HTMLElement>>('loadSentinel');

  constructor() {
    inject(DestroyRef).onDestroy(() => this.observer?.disconnect());

    effect(() => {
      const el = this.loadSentinel()?.nativeElement;
      untracked(() => this.observeSentinel(el));
    });
  }

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.nextPage = 1;
    this.items.set([]);
    this.hasNext.set(false);
    this.loadError.set(false);
    this.loadPage(false);
  }

  loadMore(): void {
    if (this.loading() || this.loadingMore() || !this.hasNext() || this.loadError()) {
      return;
    }

    this.loadPage(true);
  }

  retryLoadMore(): void {
    this.loadError.set(false);
    this.loadMore();
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

  private loadPage(append: boolean): void {
    if (append) {
      this.loadingMore.set(true);
    } else {
      this.loading.set(true);
    }

    this.error.set(null);
    const page = this.nextPage;

    this.api.getPage(page, AdminNotifications.pageSize).subscribe({
      next: (data) => {
        const incoming = data.items ?? [];
        if (append) {
          const seen = new Set(this.items().map((item) => item.id));
          this.items.update((list) => [
            ...list,
            ...incoming.filter((item) => !seen.has(item.id)),
          ]);
        } else {
          this.items.set(incoming);
        }

        this.hasNext.set(data.hasNext);
        this.nextPage = page + 1;
        this.loading.set(false);
        this.loadingMore.set(false);
        this.loadError.set(false);
      },
      error: () => {
        this.error.set('Bildirimler yüklenemedi.');
        this.loading.set(false);
        this.loadingMore.set(false);
        if (append) {
          this.loadError.set(true);
        }
      },
    });
  }

  private observeSentinel(el: HTMLElement | undefined): void {
    this.observer?.disconnect();
    this.observer = null;

    if (!el) {
      return;
    }

    this.observer = new IntersectionObserver(
      (entries) => {
        if (entries.some((entry) => entry.isIntersecting)) {
          this.loadMore();
        }
      },
      { root: null, rootMargin: '160px' },
    );
    this.observer.observe(el);
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
