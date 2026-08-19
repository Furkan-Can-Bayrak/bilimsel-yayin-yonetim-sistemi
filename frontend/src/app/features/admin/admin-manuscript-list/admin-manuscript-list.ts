import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { Observable } from 'rxjs';
import {
  AdminManuscriptListItem,
  MANUSCRIPT_STATUS_LABELS,
  MANUSCRIPT_STATUSES,
  ManuscriptStatus,
  ResearchArea,
} from '../../../core/models/manuscript.model';
import { REVIEW_RECOMMENDATION_LABELS, ReviewerCandidate } from '../../../core/models/review.model';
import { Permissions } from '../../../core/auth/permissions';
import { AuthService } from '../../../core/services/auth.service';
import { ManuscriptService } from '../../../core/services/manuscript.service';
import { ResearchAreaService } from '../../../core/services/research-area.service';
import { ReviewService } from '../../../core/services/review.service';

@Component({
  selector: 'app-admin-manuscript-list',
  imports: [RouterLink, DatePipe, FormsModule],
  templateUrl: './admin-manuscript-list.html',
  styleUrl: './admin-manuscript-list.css',
})
export class AdminManuscriptList implements OnInit {
  private readonly manuscriptsApi = inject(ManuscriptService);
  private readonly researchAreasApi = inject(ResearchAreaService);
  private readonly reviewsApi = inject(ReviewService);
  private readonly auth = inject(AuthService);

  readonly statusLabels = MANUSCRIPT_STATUS_LABELS;
  readonly statusOptions = MANUSCRIPT_STATUSES;
  readonly recommendationLabels = REVIEW_RECOMMENDATION_LABELS;

  readonly canCreate = this.auth.hasPermission(Permissions.Manuscripts.Create);
  readonly canUpdate = this.auth.hasPermission(Permissions.Manuscripts.Update);
  readonly canDelete = this.auth.hasPermission(Permissions.Manuscripts.Delete);
  readonly canSubmit = this.auth.hasPermission(Permissions.Manuscripts.Submit);
  readonly canDecide = this.auth.hasPermission(Permissions.Manuscripts.Decide);
  readonly canPublish = this.auth.hasPermission(Permissions.Manuscripts.Publish);
  readonly canUnpublish = this.auth.hasPermission(Permissions.Manuscripts.Unpublish);
  readonly canAssign = this.auth.hasPermission(Permissions.Reviews.Assign);
  readonly canViewReviews = this.auth.hasPermission(Permissions.Reviews.ViewAll);
  readonly isEditorPanel = this.auth.hasPermission(Permissions.Manuscripts.ViewAll);
  readonly canManageResearchAreas = this.auth.hasPermission(Permissions.ResearchAreas.Manage);

  readonly manuscripts = signal<AdminManuscriptListItem[]>([]);
  readonly researchAreas = signal<ResearchArea[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly busyId = signal<number | null>(null);

  readonly page = signal(1);
  readonly pageSize = 10;
  readonly totalCount = signal(0);
  readonly totalPages = signal(0);
  readonly hasPrevious = signal(false);
  readonly hasNext = signal(false);

  search = '';
  researchAreaId: number | null = null;
  statusFilter: ManuscriptStatus | '' = '';
  selectedReviewerId: Record<number, number | null> = {};
  readonly candidatesByManuscript = signal<Partial<Record<number, ReviewerCandidate[]>>>({});

  ngOnInit(): void {
    this.researchAreasApi.getAll().subscribe({
      next: (data) => this.researchAreas.set(data),
    });
    this.reload();
  }

  isOwn(manuscript: AdminManuscriptListItem): boolean {
    return this.auth.userId() === manuscript.authorId;
  }

  applyFilters(): void {
    this.page.set(1);
    this.reload();
  }

  clearFilters(): void {
    this.search = '';
    this.researchAreaId = null;
    this.statusFilter = '';
    this.page.set(1);
    this.reload();
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages()) {
      return;
    }
    this.page.set(page);
    this.reload();
  }

  reload(): void {
    this.loading.set(true);
    this.error.set(null);

    this.manuscriptsApi
      .getAdminList({
        page: this.page(),
        pageSize: this.pageSize,
        search: this.search,
        researchAreaId: this.researchAreaId,
        status: this.statusFilter || null,
      })
      .subscribe({
        next: (data) => {
          this.manuscripts.set(data.items);
          this.totalCount.set(data.totalCount);
          this.totalPages.set(data.totalPages);
          this.hasPrevious.set(data.hasPrevious);
          this.hasNext.set(data.hasNext);
          this.page.set(data.page);
          this.loading.set(false);
          this.loadCandidates(data.items);
        },
        error: () => {
          this.error.set('Makaleler yüklenemedi. Oturumunuzun süresi dolmuş olabilir.');
          this.loading.set(false);
        },
      });
  }

  submitForReview(manuscript: AdminManuscriptListItem): void {
    this.run(manuscript.id, this.manuscriptsApi.submit(manuscript.id), 'Gönderilemedi.');
  }

  accept(manuscript: AdminManuscriptListItem): void {
    this.run(manuscript.id, this.manuscriptsApi.accept(manuscript.id), 'Kabul edilemedi.');
  }

  reject(manuscript: AdminManuscriptListItem): void {
    this.run(manuscript.id, this.manuscriptsApi.reject(manuscript.id), 'Reddedilemedi.');
  }

  assign(manuscript: AdminManuscriptListItem): void {
    const reviewerId = this.selectedReviewerId[manuscript.id];
    if (!reviewerId) {
      this.error.set('Hakem seçin.');
      return;
    }

    this.run(manuscript.id, this.reviewsApi.assign(manuscript.id, reviewerId), 'Hakem atanamadı.');
  }

  publish(manuscript: AdminManuscriptListItem): void {
    this.run(manuscript.id, this.manuscriptsApi.publish(manuscript.id), 'Yayınlanamadı.');
  }

  unpublish(manuscript: AdminManuscriptListItem): void {
    this.run(manuscript.id, this.manuscriptsApi.unpublish(manuscript.id), 'Yayından alınamadı.');
  }

  remove(manuscript: AdminManuscriptListItem): void {
    if (!confirm(`"${manuscript.title}" silinsin mi?`)) {
      return;
    }

    this.run(manuscript.id, this.manuscriptsApi.delete(manuscript.id), 'Silme başarısız.');
  }

  private run(id: number, request: Observable<unknown>, failMessage: string): void {
    this.busyId.set(id);
    this.error.set(null);
    request.subscribe({
      next: () => {
        this.busyId.set(null);
        this.reload();
      },
      error: (err: unknown) => {
        this.busyId.set(null);
        const detail = (err as { error?: { detail?: string } })?.error?.detail;
        this.error.set(detail ?? failMessage);
      },
    });
  }

  private loadCandidates(items: AdminManuscriptListItem[]): void {
    if (!this.canAssign) {
      return;
    }

    for (const manuscript of items) {
      if (manuscript.status !== 'Submitted') {
        continue;
      }

      this.reviewsApi.getCandidates(manuscript.id).subscribe({
        next: (candidates) => {
          if (this.selectedReviewerId[manuscript.id] === undefined) {
            this.selectedReviewerId[manuscript.id] = null;
          }
          this.candidatesByManuscript.update((current) => ({
            ...current,
            [manuscript.id]: candidates,
          }));
        },
      });
    }
  }
}
