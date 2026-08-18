import { ManuscriptStatus, ReviewRecommendation, ReviewSummary } from './manuscript.model';

export type { ReviewRecommendation, ReviewSummary };

export const REVIEW_RECOMMENDATION_LABELS: Record<ReviewRecommendation, string> = {
  Accept: 'Kabul önerisi',
  Reject: 'Ret önerisi',
};

export interface ReviewerCandidate {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
}

export interface MyReviewListItem {
  id: number;
  manuscriptId: number;
  manuscriptTitle: string;
  manuscriptStatus: ManuscriptStatus;
  assignedAtUtc: string;
  submittedAtUtc: string | null;
  recommendation: ReviewRecommendation | null;
}

export interface ReviewDetail {
  id: number;
  manuscriptId: number;
  manuscriptTitle: string;
  manuscriptContent: string;
  manuscriptSummary: string | null;
  reviewerId: number;
  reviewerName: string;
  assignedAtUtc: string;
  submittedAtUtc: string | null;
  summary: ReviewSummary;
}

export interface SubmitReviewRequest {
  recommendation: ReviewRecommendation;
  comments: string;
}
