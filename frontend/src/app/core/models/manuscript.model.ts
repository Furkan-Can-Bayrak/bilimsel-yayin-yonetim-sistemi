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

export interface AdminManuscriptListItem {
  id: number;
  title: string;
  slug: string;
  summary: string | null;
  publishedAt: string | null;
  isPublished: boolean;
  researchAreaId: number;
  researchAreaName: string;
  authorId: number;
  authorName: string;
}

export interface AdminManuscriptDetail {
  id: number;
  title: string;
  slug: string;
  content: string;
  summary: string | null;
  publishedAt: string | null;
  isPublished: boolean;
  researchAreaId: number;
  researchAreaName: string;
  authorId: number;
  authorName: string;
}

export interface CreateManuscriptRequest {
  title: string;
  content: string;
  summary: string | null;
  researchAreaId: number;
  slug?: string | null;
}

export interface UpdateManuscriptRequest {
  title: string;
  content: string;
  summary: string | null;
  researchAreaId: number;
  slug?: string | null;
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
  isPublished?: boolean | null;
}

export interface ResearchArea {
  id: number;
  name: string;
  slug: string;
  manuscriptCount: number;
}
