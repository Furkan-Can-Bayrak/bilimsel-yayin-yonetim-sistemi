import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { ManuscriptService } from '../../../core/services/manuscript.service';
import { ResearchAreaService } from '../../../core/services/research-area.service';
import { ManuscriptListItem, ResearchArea } from '../../../core/models/manuscript.model';

@Component({
  selector: 'app-manuscript-list',
  imports: [RouterLink, DatePipe, FormsModule],
  templateUrl: './manuscript-list.html',
  styleUrl: './manuscript-list.css',
})
export class ManuscriptList implements OnInit {
  private readonly manuscriptsApi = inject(ManuscriptService);
  private readonly researchAreasApi = inject(ResearchAreaService);

  readonly manuscripts = signal<ManuscriptListItem[]>([]);
  readonly researchAreas = signal<ResearchArea[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  readonly page = signal(1);
  readonly pageSize = 10;
  readonly totalCount = signal(0);
  readonly totalPages = signal(0);
  readonly hasPrevious = signal(false);
  readonly hasNext = signal(false);

  search = '';
  researchAreaId: number | null = null;

  ngOnInit(): void {
    this.researchAreasApi.getAll().subscribe({
      next: (data) => this.researchAreas.set(data),
    });
    this.load();
  }

  applyFilters(): void {
    this.page.set(1);
    this.load();
  }

  clearFilters(): void {
    this.search = '';
    this.researchAreaId = null;
    this.page.set(1);
    this.load();
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages()) {
      return;
    }
    this.page.set(page);
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.error.set(null);

    this.manuscriptsApi
      .getPublished({
        page: this.page(),
        pageSize: this.pageSize,
        search: this.search,
        researchAreaId: this.researchAreaId,
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
          this.error.set('Makaleler yüklenemedi. Yayın sunucusuna ulaşılamıyor.');
          this.loading.set(false);
        },
      });
  }
}
