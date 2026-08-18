export interface AppNotification {
  id: number;
  title: string;
  message: string;
  relatedManuscriptId: number | null;
  createdAtUtc: string;
  isRead: boolean;
}
