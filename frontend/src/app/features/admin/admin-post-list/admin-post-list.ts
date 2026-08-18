import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { AdminPostListItem } from '../../../core/models/post.model';
import { PostService } from '../../../core/services/post.service';
import { CategoryService } from '../../../core/services/category.service';
import { Category } from '../../../core/models/auth.model';

@Component({
  selector: 'app-admin-post-list',
  imports: [RouterLink, DatePipe, FormsModule],
  templateUrl: './admin-post-list.html',
  styleUrl: './admin-post-list.css',
})
export class AdminPostList implements OnInit {
  private readonly postsApi = inject(PostService);
  private readonly categoriesApi = inject(CategoryService);

  readonly posts = signal<AdminPostListItem[]>([]);
  readonly categories = signal<Category[]>([]);
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
  categoryId: number | null = null;
  /** '' | 'true' | 'false' — select bağlama kolaylığı */
  publishedFilter: '' | 'true' | 'false' = '';

  ngOnInit(): void {
    this.categoriesApi.getAll().subscribe({
      next: (data) => this.categories.set(data),
    });
    this.reload();
  }

  applyFilters(): void {
    this.page.set(1);
    this.reload();
  }

  clearFilters(): void {
    this.search = '';
    this.categoryId = null;
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

    this.postsApi
      .getAdminPosts({
        page: this.page(),
        pageSize: this.pageSize,
        search: this.search,
        categoryId: this.categoryId,
        isPublished,
      })
      .subscribe({
        next: (data) => {
          this.posts.set(data.items);
          this.totalCount.set(data.totalCount);
          this.totalPages.set(data.totalPages);
          this.hasPrevious.set(data.hasPrevious);
          this.hasNext.set(data.hasNext);
          this.page.set(data.page);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Yazılar yüklenemedi. Token geçerli mi? Backend açık mı?');
          this.loading.set(false);
        },
      });
  }

  togglePublish(post: AdminPostListItem): void {
    this.busyId.set(post.id);

    this.postsApi.getAdminById(post.id).subscribe({
      next: (full) => {
        this.postsApi
          .update(post.id, {
            title: full.title,
            content: full.content,
            summary: full.summary,
            categoryId: full.categoryId,
            isPublished: !full.isPublished,
            slug: full.slug,
          })
          .subscribe({
            next: () => {
              this.busyId.set(null);
              this.reload();
            },
            error: () => {
              this.busyId.set(null);
              this.error.set('Yayın durumu güncellenemedi.');
            },
          });
      },
      error: () => {
        this.busyId.set(null);
        this.error.set('Yazı detayı alınamadı.');
      },
    });
  }

  remove(post: AdminPostListItem): void {
    if (!confirm(`"${post.title}" silinsin mi?`)) {
      return;
    }

    this.busyId.set(post.id);
    this.postsApi.delete(post.id).subscribe({
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
