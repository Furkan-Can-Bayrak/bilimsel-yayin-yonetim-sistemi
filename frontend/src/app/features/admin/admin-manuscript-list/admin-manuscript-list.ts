import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { AdminManuscriptListItem, ResearchArea } from '../../../core/models/manuscript.model';
import { Permissions } from '../../../core/auth/permissions';
import { AuthService } from '../../../core/services/auth.service';
import { ManuscriptService } from '../../../core/services/manuscript.service';
import { ResearchAreaService } from '../../../core/services/research-area.service';

@Component({
  selector: 'app-admin-manuscript-list',
  imports: [RouterLink, DatePipe, FormsModule],
  templateUrl: './admin-manuscript-list.html',
  styleUrl: './admin-manuscript-list.css',
})
export class AdminManuscriptList implements OnInit {
  private readonly manuscriptsApi = inject(ManuscriptService);
  private readonly researchAreasApi = inject(ResearchAreaService);
  private readonly auth = inject(AuthService);

  readonly canCreate = this.auth.hasPermission(Permissions.Manuscripts.Create);
  readonly canUpdate = this.auth.hasPermission(Permissions.Manuscripts.Update);
  readonly canDelete = this.auth.hasPermission(Permissions.Manuscripts.Delete);
  readonly canPublish = this.auth.hasPermission(Permissions.Manuscripts.Publish);
  readonly canUnpublish = this.auth.hasPermission(Permissions.Manuscripts.Unpublish);
  readonly isEditorPanel = this.auth.hasPermission(Permissions.Manuscripts.ViewAll);

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
  publishedFilter: '' | 'true' | 'false' = '';

  ngOnInit(): void {
    this.researchAreasApi.getAll().subscribe({
      next: (data) => this.researchAreas.set(data),
    });
    this.reload();
  }

  applyFilters(): void {
    this.page.set(1);
    this.reload();
  }

  clearFilters(): void {
    this.search = '';
    this.researchAreaId = null;
    this.publishedFilter = '';
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

    let isPublished: boolean | null = null;
    if (this.publishedFilter === 'true') {
      isPublished = true;
    } else if (this.publishedFilter === 'false') {
      isPublished = false;
    }

    this.manuscriptsApi
      .getAdminList({
        page: this.page(),
        pageSize: this.pageSize,
        search: this.search,
        researchAreaId: this.researchAreaId,
        isPublished,
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
        },
        error: () => {
          this.error.set('Makaleler yüklenemedi. Oturumunuzun süresi dolmuş olabilir.');
          this.loading.set(false);
        },
      });
  }

  togglePublish(manuscript: AdminManuscriptListItem): void {
    this.busyId.set(manuscript.id);
    this.error.set(null);

    const request = manuscript.isPublished
      ? this.manuscriptsApi.unpublish(manuscript.id)
      : this.manuscriptsApi.publish(manuscript.id);

    request.subscribe({
      next: () => {
        this.busyId.set(null);
        this.reload();
      },
      error: () => {
        this.busyId.set(null);
        this.error.set('Yayın durumu güncellenemedi.');
      },
    });
  }

  remove(manuscript: AdminManuscriptListItem): void {
    if (!confirm(`"${manuscript.title}" silinsin mi?`)) {
      return;
    }

    this.busyId.set(manuscript.id);
    this.manuscriptsApi.delete(manuscript.id).subscribe({
      next: () => {
        this.busyId.set(null);
        this.reload();
      },
      error: () => {
        this.busyId.set(null);
        this.error.set('Silme başarısız.');
      },
    });
  }
}
