/** Backend DTO'larıyla aynı şekil (C# PostListItemDto / PostDetailDto) */

export interface PostListItem {
  id: number;
  title: string;
  slug: string;
  summary: string | null;
  publishedAt: string | null;
  categoryName: string;
}

export interface PostDetail {
  id: number;
  title: string;
  slug: string;
  content: string;
  summary: string | null;
  publishedAt: string | null;
  categoryName: string;
  categorySlug: string;
}

/** Admin listesi — taslaklar dahil */
export interface AdminPostListItem {
  id: number;
  title: string;
  slug: string;
  summary: string | null;
  publishedAt: string | null;
  isPublished: boolean;
  categoryId: number;
  categoryName: string;
}

/** Admin düzenleme formu */
export interface AdminPostDetail {
  id: number;
  title: string;
  slug: string;
  content: string;
  summary: string | null;
  publishedAt: string | null;
  isPublished: boolean;
  categoryId: number;
  categoryName: string;
}

export interface CreatePostRequest {
  title: string;
  content: string;
  summary: string | null;
  categoryId: number;
  isPublished: boolean;
  slug?: string | null;
}

export interface UpdatePostRequest {
  title: string;
  content: string;
  summary: string | null;
  categoryId: number;
  isPublished: boolean;
  slug?: string | null;
}

export interface CreatePostResult {
  id: number;
  slug: string;
}

/** Backend PagedResult&lt;T&gt; */
export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export interface PostListQuery {
  page?: number;
  pageSize?: number;
  search?: string;
  categoryId?: number | null;
  isPublished?: boolean | null;
}
