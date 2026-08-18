import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { PostService } from '../../../core/services/post.service';
import { CategoryService } from '../../../core/services/category.service';
import { PostListItem } from '../../../core/models/post.model';
import { Category } from '../../../core/models/auth.model';

@Component({
  selector: 'app-post-list',
  imports: [RouterLink, DatePipe, FormsModule],
  templateUrl: './post-list.html',
  styleUrl: './post-list.css',
})
export class PostList implements OnInit {
  private readonly postsApi = inject(PostService);
  private readonly categoriesApi = inject(CategoryService);

  readonly posts = signal<PostListItem[]>([]);
  readonly categories = signal<Category[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  readonly page = signal(1);
  readonly pageSize = 10;
  readonly totalCount = signal(0);
  readonly totalPages = signal(0);
  readonly hasPrevious = signal(false);
  readonly hasNext = signal(false);

  search = '';
  categoryId: number | null = null;

  ngOnInit(): void {
    this.categoriesApi.getAll().subscribe({
      next: (data) => this.categories.set(data),
    });
    this.load();
  }

  applyFilters(): void {
    this.page.set(1);
    this.load();
  }

  clearFilters(): void {
    this.search = '';
    this.categoryId = null;
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

    this.postsApi
      .getPosts({
        page: this.page(),
        pageSize: this.pageSize,
        search: this.search,
        categoryId: this.categoryId,
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
          this.error.set(
            'Yazılar yüklenemedi. Backend çalışıyor mu? (dotnet run --project src/Blog.API)',
          );
          this.loading.set(false);
        },
      });
  }
}
