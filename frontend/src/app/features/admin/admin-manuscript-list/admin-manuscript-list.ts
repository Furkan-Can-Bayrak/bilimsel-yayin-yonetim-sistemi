import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { Observable } from 'rxjs';
import {
  AdminManuscriptListItem,
  MANUSCRIPT_STATUS_LABELS,
  MANUSCRIPT_STATUSES,
  ManuscriptStatus,
  ResearchArea,
  ReviewSummary,
} from '../../../core/models/manuscript.model';
import { ReviewerCandidate } from '../../../core/models/review.model';
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
  private readonly route = inject(ActivatedRoute);

  readonly statusLabels = MANUSCRIPT_STATUS_LABELS;
  isEditorPanel = false;

  readonly canCreate = this.auth.hasPermission(Permissions.Manuscripts.Create);
  readonly canUpdate = this.auth.hasPermission(Permissions.Manuscripts.Update);
  readonly canDelete = this.auth.hasPermission(Permissions.Manuscripts.Delete);
  readonly canDecide = this.auth.hasPermission(Permissions.Manuscripts.Decide);
  readonly canPublish = this.auth.hasPermission(Permissions.Manuscripts.Publish);
  readonly canUnpublish = this.auth.hasPermission(Permissions.Manuscripts.Unpublish);
  readonly canAssign = this.auth.hasPermission(Permissions.Reviews.Assign);
  readonly canViewReviews = this.auth.hasPermission(Permissions.Reviews.ViewAll);

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

  readonly assignTarget = signal<AdminManuscriptListItem | null>(null);
  readonly reportTarget = signal<{ title: string; review: ReviewSummary } | null>(null);
  readonly modalCandidates = signal<ReviewerCandidate[]>([]);
  readonly candidatesLoading = signal(false);
  readonly modalError = signal<string | null>(null);
  modalReviewerId: number | null = null;

  ngOnInit(): void {
    this.researchAreasApi.getAll().subscribe({
      next: (data) => this.researchAreas.set(data),
    });

    this.route.data.subscribe((data) => {
      this.isEditorPanel = data['editorPanel'] === true;
      if (this.isEditorPanel && this.statusFilter === 'Draft') {
        this.statusFilter = '';
      }
      this.page.set(1);
      this.reload();
    });
  }

  get statusOptions(): ManuscriptStatus[] {
    return this.isEditorPanel
      ? MANUSCRIPT_STATUSES.filter((status) => status !== 'Draft')
      : MANUSCRIPT_STATUSES;
  }

  isOwn(manuscript: AdminManuscriptListItem): boolean {
    return this.auth.userId() === manuscript.authorId;
  }

  canShowEdit(manuscript: AdminManuscriptListItem): boolean {
    return (
      this.canUpdate &&
      this.isOwn(manuscript) &&
      (manuscript.status === 'Draft' || manuscript.status === 'Rejected')
    );
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

    const query = {
      page: this.page(),
      pageSize: this.pageSize,
      search: this.search,
      researchAreaId: this.researchAreaId,
      status: this.statusFilter || null,
    };

    const request = this.isEditorPanel
      ? this.manuscriptsApi.getAdminList(query)
      : this.manuscriptsApi.getMyList(query);

    request
      .subscribe({
        next: (data) => {
          this.manuscripts.set(data.items);
          this.totalCount.set(data.totalCount);
          this.totalPages.set(data.totalPages);
          this.hasPrevious.set(data.hasPrevious);
          this.hasNext.set(data.hasNext);
          this.page.set(data.page);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Makaleler yüklenemedi. Oturumunuzun süresi dolmuş olabilir.');
          this.loading.set(false);
        },
      });
  }

  accept(manuscript: AdminManuscriptListItem): void {
    this.run(manuscript.id, this.manuscriptsApi.accept(manuscript.id), 'Kabul edilemedi.');
  }

  reject(manuscript: AdminManuscriptListItem): void {
    this.run(manuscript.id, this.manuscriptsApi.reject(manuscript.id), 'Reddedilemedi.');
  }

  openAssign(manuscript: AdminManuscriptListItem): void {
    this.assignTarget.set(manuscript);
    this.modalReviewerId = null;
    this.modalCandidates.set([]);
    this.modalError.set(null);
    this.candidatesLoading.set(true);

    this.reviewsApi.getCandidates(manuscript.id).subscribe({
      next: (candidates) => {
        this.modalCandidates.set(candidates);
        this.candidatesLoading.set(false);
      },
      error: (err: unknown) => {
        this.candidatesLoading.set(false);
        const detail = (err as { error?: { detail?: string } })?.error?.detail;
        this.modalError.set(detail ?? 'Hakem listesi alınamadı.');
      },
    });
  }

  closeAssign(): void {
    this.assignTarget.set(null);
    this.modalReviewerId = null;
    this.modalError.set(null);
  }

  openReport(manuscript: AdminManuscriptListItem): void {
    const review = manuscript.currentReview;
    if (!review) {
      return;
    }

    this.reportTarget.set({ title: manuscript.title, review });
  }

  closeReport(): void {
    this.reportTarget.set(null);
  }

  confirmAssign(): void {
    const manuscript = this.assignTarget();
    const reviewerId = this.modalReviewerId;
    if (!manuscript || !reviewerId) {
      this.modalError.set('Hakem seçin.');
      return;
    }

    this.busyId.set(manuscript.id);
    this.modalError.set(null);
    this.reviewsApi.assign(manuscript.id, reviewerId).subscribe({
      next: () => {
        this.busyId.set(null);
        this.closeAssign();
        this.reload();
      },
      error: (err: unknown) => {
        this.busyId.set(null);
        const detail = (err as { error?: { detail?: string } })?.error?.detail;
        this.modalError.set(detail ?? 'Hakem atanamadı.');
      },
    });
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
}
