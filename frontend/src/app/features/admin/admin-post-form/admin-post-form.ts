import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Category } from '../../../core/models/auth.model';
import { CategoryService } from '../../../core/services/category.service';
import { PostService } from '../../../core/services/post.service';

@Component({
  selector: 'app-admin-post-form',
  imports: [ReactiveFormsModule, FormsModule, RouterLink],
  templateUrl: './admin-post-form.html',
  styleUrl: './admin-post-form.css',
})
export class AdminPostForm implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly postsApi = inject(PostService);
  private readonly categoriesApi = inject(CategoryService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly categories = signal<Category[]>([]);
  readonly loading = signal(true);
  readonly submitting = signal(false);
  readonly error = signal<string | null>(null);
  readonly editId = signal<number | null>(null);

  readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    slug: [''],
    summary: [''],
    content: ['', Validators.required],
    categoryId: [0, [Validators.required, Validators.min(1)]],
    isPublished: [false],
  });

  get isEdit(): boolean {
    return this.editId() !== null;
  }

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    const id = idParam ? Number(idParam) : null;

    this.categoriesApi.getAll().subscribe({
      next: (cats) => {
        this.categories.set(cats);
        if (!id && cats.length > 0) {
          this.form.patchValue({ categoryId: cats[0].id });
        }

        if (id && !Number.isNaN(id)) {
          this.editId.set(id);
          this.loadPost(id);
        } else {
          this.loading.set(false);
        }
      },
      error: () => {
        this.error.set('Kategoriler yüklenemedi.');
        this.loading.set(false);
      },
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const body = {
      title: raw.title,
      content: raw.content,
      summary: raw.summary.trim() ? raw.summary.trim() : null,
      categoryId: raw.categoryId,
      isPublished: raw.isPublished,
      slug: raw.slug.trim() ? raw.slug.trim() : null,
    };

    this.submitting.set(true);
    this.error.set(null);

    const id = this.editId();
    const onOk = () => {
      this.submitting.set(false);
      void this.router.navigateByUrl('/admin');
    };
    const onErr = (err: unknown) => {
      this.submitting.set(false);
      this.error.set(this.readError(err) ?? 'Kayıt başarısız.');
    };

    if (id) {
      this.postsApi.update(id, body).subscribe({ next: onOk, error: onErr });
    } else {
      this.postsApi.create(body).subscribe({ next: onOk, error: onErr });
    }
  }

  private loadPost(id: number): void {
    this.postsApi.getAdminById(id).subscribe({
      next: (post) => {
        this.form.patchValue({
          title: post.title,
          slug: post.slug,
          summary: post.summary ?? '',
          content: post.content,
          categoryId: post.categoryId,
          isPublished: post.isPublished,
        });
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Yazı bulunamadı.');
        this.loading.set(false);
      },
    });
  }

  private readError(err: unknown): string | null {
    const e = err as { error?: { title?: string; detail?: string; errors?: Record<string, string[]> } };
    if (e?.error?.detail) {
      return e.error.detail;
    }
    if (e?.error?.title) {
      return e.error.title;
    }
    const errors = e?.error?.errors;
    if (errors) {
      return Object.values(errors).flat().join(' ');
    }
    return null;
  }
}
