export type ManuscriptStatus =
  | 'Draft'
  | 'Submitted'
  | 'UnderReview'
  | 'Accepted'
  | 'Rejected'
  | 'Published';

export const MANUSCRIPT_STATUS_LABELS: Record<ManuscriptStatus, string> = {
  Draft: 'Taslak',
  Submitted: 'Gönderildi',
  UnderReview: 'İncelemede',
  Accepted: 'Kabul',
  Rejected: 'Ret',
  Published: 'Yayında',
};

export const MANUSCRIPT_STATUSES: ManuscriptStatus[] = [
  'Draft',
  'Submitted',
  'UnderReview',
  'Accepted',
  'Rejected',
  'Published',
];

export interface ManuscriptListItem {
  id: number;
  title: string;
  slug: string;
  summary: string | null;
  publishedAt: string | null;
  researchAreaName: string;
  authorName: string;
}

export interface ManuscriptDetail {
  id: number;
  title: string;
  slug: string;
  content: string;
  summary: string | null;
  publishedAt: string | null;
  researchAreaName: string;
  researchAreaSlug: string;
  authorName: string;
}

export type ReviewRecommendation = 'Accept' | 'Reject';

export interface ReviewSummary {
  id: number;
  reviewerId: number;
  reviewerName: string;
  assignedAtUtc: string;
  submittedAtUtc: string | null;
  recommendation: ReviewRecommendation | null;
  comments: string | null;
}

export interface AdminManuscriptListItem {
  id: number;
  title: string;
  slug: string;
  summary: string | null;
  publishedAt: string | null;
  status: ManuscriptStatus;
  researchAreaId: number | null;
  researchAreaName: string;
  authorId: number;
  authorName: string;
  currentReview: ReviewSummary | null;
  reviews: ReviewSummary[];
  rejectionReason: string | null;
}

export interface AdminManuscriptDetail {
  id: number;
  title: string;
  slug: string;
  content: string;
  summary: string | null;
  publishedAt: string | null;
  status: ManuscriptStatus;
  researchAreaId: number | null;
  researchAreaName: string;
  authorId: number;
  authorName: string;
  currentReview: ReviewSummary | null;
  reviews: ReviewSummary[];
  rejectionReason: string | null;
}

export interface CreateManuscriptRequest {
  title: string;
  content: string;
  summary: string | null;
  researchAreaId: number | null;
  submitForReview?: boolean;
}

export interface UpdateManuscriptRequest {
  title: string;
  content: string;
  summary: string | null;
  researchAreaId: number | null;
  submitForReview?: boolean;
}

export interface CreateManuscriptResult {
  id: number;
  slug: string;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export interface ManuscriptListQuery {
  page?: number;
  pageSize?: number;
  search?: string;
  researchAreaId?: number | null;
  status?: ManuscriptStatus | null;
}

export interface ResearchArea {
  id: number;
  name: string;
  slug: string;
  manuscriptCount: number;
}

export interface CreateResearchAreaResult {
  id: number;
  slug: string;
}
