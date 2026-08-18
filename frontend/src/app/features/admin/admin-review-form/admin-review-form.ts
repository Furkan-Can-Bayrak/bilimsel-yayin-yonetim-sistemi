import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  REVIEW_RECOMMENDATION_LABELS,
  ReviewDetail,
  ReviewRecommendation,
} from '../../../core/models/review.model';
import { ReviewService } from '../../../core/services/review.service';

@Component({
  selector: 'app-admin-review-form',
  imports: [FormsModule, RouterLink],
  templateUrl: './admin-review-form.html',
  styleUrl: './admin-review-form.css',
})
export class AdminReviewForm implements OnInit {
  private readonly reviewsApi = inject(ReviewService);
  private readonly route = inject(ActivatedRoute);

  readonly recommendationLabels = REVIEW_RECOMMENDATION_LABELS;
  readonly options: ReviewRecommendation[] = ['Accept', 'Reject'];

  readonly review = signal<ReviewDetail | null>(null);
  readonly loading = signal(true);
  readonly submitting = signal(false);
  readonly error = signal<string | null>(null);

  recommendation: ReviewRecommendation | '' = '';
  comments = '';

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) {
      this.error.set('Değerlendirme bulunamadı.');
      this.loading.set(false);
      return;
    }

    this.reviewsApi.getById(id).subscribe({
      next: (data) => {
        this.review.set(data);
        if (data.summary.recommendation) {
          this.recommendation = data.summary.recommendation;
        }
        this.comments = data.summary.comments ?? '';
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Değerlendirme yüklenemedi.');
        this.loading.set(false);
      },
    });
  }

  get isSubmitted(): boolean {
    return this.review()?.submittedAtUtc != null;
  }

  submit(): void {
    const current = this.review();
    if (!current || this.isSubmitted) {
      return;
    }

    if (this.recommendation !== 'Accept' && this.recommendation !== 'Reject') {
      this.error.set('Kabul veya ret önerisi seçin.');
      return;
    }

    const comments = this.comments.trim();
    if (!comments) {
      this.error.set('Gerekçe zorunludur.');
      return;
    }

    this.submitting.set(true);
    this.error.set(null);
    this.reviewsApi
      .submit(current.id, { recommendation: this.recommendation, comments })
      .subscribe({
        next: () => {
          this.submitting.set(false);
          this.review.set({
            ...current,
            submittedAtUtc: new Date().toISOString(),
            summary: {
              ...current.summary,
              submittedAtUtc: new Date().toISOString(),
              recommendation: this.recommendation as ReviewRecommendation,
              comments,
            },
          });
        },
        error: (err: unknown) => {
          this.submitting.set(false);
          const detail = (err as { error?: { detail?: string } })?.error?.detail;
          this.error.set(detail ?? 'Rapor teslim edilemedi.');
        },
      });
  }
}
