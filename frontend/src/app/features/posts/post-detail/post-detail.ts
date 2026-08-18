import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { PostService } from '../../../core/services/post.service';
import { PostDetail } from '../../../core/models/post.model';

@Component({
  selector: 'app-post-detail',
  imports: [RouterLink, DatePipe],
  templateUrl: './post-detail.html',
  styleUrl: './post-detail.css',
})
export class PostDetailPage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly postsApi = inject(PostService);

  readonly post = signal<PostDetail | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    // URL'deki :slug parametresi (ör. /posts/merhaba-blog)
    const slug = this.route.snapshot.paramMap.get('slug');

    if (!slug) {
      this.error.set('Geçersiz yazı adresi.');
      this.loading.set(false);
      return;
    }

    this.postsApi.getBySlug(slug).subscribe({
      next: (data) => {
        this.post.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Yazı bulunamadı veya API’ye ulaşılamadı.');
        this.loading.set(false);
      },
    });
  }
}
