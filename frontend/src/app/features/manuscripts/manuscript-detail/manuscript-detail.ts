import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ManuscriptService } from '../../../core/services/manuscript.service';
import { ManuscriptDetail } from '../../../core/models/manuscript.model';
import { ManuscriptBody } from '../../../shared/manuscript-body/manuscript-body';

@Component({
  selector: 'app-manuscript-detail',
  imports: [RouterLink, ManuscriptBody],
  templateUrl: './manuscript-detail.html',
  styleUrl: './manuscript-detail.css',
})
export class ManuscriptDetailPage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly manuscriptsApi = inject(ManuscriptService);

  readonly manuscript = signal<ManuscriptDetail | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    const slug = this.route.snapshot.paramMap.get('slug');

    if (!slug) {
      this.error.set('Geçersiz makale adresi.');
      this.loading.set(false);
      return;
    }

    this.manuscriptsApi.getBySlug(slug).subscribe({
      next: (data) => {
        this.manuscript.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Makale bulunamadı veya sunucuya ulaşılamadı.');
        this.loading.set(false);
      },
    });
  }
}
