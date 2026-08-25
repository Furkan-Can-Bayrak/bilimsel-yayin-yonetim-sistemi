import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  AdminManuscriptDetail,
  MANUSCRIPT_STATUS_LABELS,
} from '../../../core/models/manuscript.model';
import { REVIEW_RECOMMENDATION_LABELS } from '../../../core/models/review.model';
import { Permissions } from '../../../core/auth/permissions';
import { AuthService } from '../../../core/services/auth.service';
import { ManuscriptService } from '../../../core/services/manuscript.service';
import { ManuscriptBody } from '../../../shared/manuscript-body/manuscript-body';

@Component({
  selector: 'app-admin-manuscript-view',
  imports: [RouterLink, ManuscriptBody],
  templateUrl: './admin-manuscript-view.html',
  styleUrl: './admin-manuscript-view.css',
})
export class AdminManuscriptView implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly manuscriptsApi = inject(ManuscriptService);
  private readonly auth = inject(AuthService);

  readonly statusLabels = MANUSCRIPT_STATUS_LABELS;
  readonly recommendationLabels = REVIEW_RECOMMENDATION_LABELS;

  readonly manuscript = signal<AdminManuscriptDetail | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly busy = signal(false);

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) {
      this.error.set('Makale bulunamadı.');
      this.loading.set(false);
      return;
    }

    this.load(id);
  }

  get isOwn(): boolean {
    const item = this.manuscript();
    return item != null && this.auth.userId() === item.authorId;
  }

  get backLink(): string {
    return this.isOwn || !this.auth.hasPermission(Permissions.Manuscripts.ViewAll)
      ? '/admin/mine'
      : '/admin';
  }

  get canDecide(): boolean {
    const item = this.manuscript();
    return (
      this.auth.hasPermission(Permissions.Manuscripts.Decide) &&
      !this.isOwn &&
      (item?.status === 'Submitted' || item?.status === 'UnderReview')
    );
  }

  get canPublish(): boolean {
    return (
      this.auth.hasPermission(Permissions.Manuscripts.Publish) &&
      !this.isOwn &&
      this.manuscript()?.status === 'Accepted'
    );
  }

  get canUnpublish(): boolean {
    return (
      this.auth.hasPermission(Permissions.Manuscripts.Unpublish) &&
      !this.isOwn &&
      this.manuscript()?.status === 'Published'
    );
  }

  get canEdit(): boolean {
    const item = this.manuscript();
    return (
      this.auth.hasPermission(Permissions.Manuscripts.Update) &&
      this.isOwn &&
      (item?.status === 'Draft' || item?.status === 'Rejected')
    );
  }

  accept(): void {
    this.run((id) => this.manuscriptsApi.accept(id), 'Kabul edilemedi.');
  }

  reject(): void {
    this.run((id) => this.manuscriptsApi.reject(id), 'Reddedilemedi.');
  }

  publish(): void {
    this.run((id) => this.manuscriptsApi.publish(id), 'Yayınlanamadı.');
  }

  unpublish(): void {
    this.run((id) => this.manuscriptsApi.unpublish(id), 'Yayından alınamadı.');
  }

  private load(id: number): void {
    this.loading.set(true);
    this.manuscriptsApi.getAdminById(id).subscribe({
      next: (data) => {
        this.manuscript.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Makale bulunamadı veya görüntüleme yetkiniz yok.');
        this.loading.set(false);
      },
    });
  }

  private run(action: (id: number) => ReturnType<ManuscriptService['accept']>, failMessage: string): void {
    const item = this.manuscript();
    if (!item) {
      return;
    }

    this.busy.set(true);
    this.error.set(null);
    action(item.id).subscribe({
      next: () => {
        this.busy.set(false);
        this.load(item.id);
      },
      error: (err: unknown) => {
        this.busy.set(false);
        const detail = (err as { error?: { detail?: string } })?.error?.detail;
        this.error.set(detail ?? failMessage);
      },
    });
  }
}
