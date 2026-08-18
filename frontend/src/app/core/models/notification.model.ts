export interface AppNotification {
  id: number;
  title: string;
  message: string;
  relatedPostId: number | null;
  createdAtUtc: string;
  isRead: boolean;
}
