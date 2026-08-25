import { Component, input } from '@angular/core';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-manuscript-body',
  imports: [DatePipe],
  templateUrl: './manuscript-body.html',
  styleUrl: './manuscript-body.css',
})
export class ManuscriptBody {
  readonly title = input.required<string>();
  readonly authorName = input<string | null>(null);
  readonly researchAreaName = input<string | null>(null);
  readonly publishedAt = input<string | null>(null);
  readonly summary = input<string | null>(null);
  readonly content = input<string | null>(null);
  readonly compact = input(false);
}
