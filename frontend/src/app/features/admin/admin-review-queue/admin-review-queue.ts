import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MANUSCRIPT_STATUS_LABELS } from '../../../core/models/manuscript.model';
import { REVIEW_RECOMMENDATION_LABELS, MyReviewListItem } from '../../../core/models/review.model';
import { ReviewService } from '../../../core/services/review.service';

@Component({
  selector: 'app-admin-review-queue',
  imports: [RouterLink, DatePipe],
  templateUrl: './admin-review-queue.html',
  styleUrl: './admin-review-queue.css',
})
export class AdminReviewQueue implements OnInit {
  private readonly reviewsApi = inject(ReviewService);

  readonly statusLabels = MANUSCRIPT_STATUS_LABELS;
  readonly recommendationLabels = REVIEW_RECOMMENDATION_LABELS;

  readonly items = signal<MyReviewListItem[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.reviewsApi.getMine().subscribe({
      next: (data) => {
        this.items.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Değerlendirmeler yüklenemedi.');
        this.loading.set(false);
      },
    });
  }
}
