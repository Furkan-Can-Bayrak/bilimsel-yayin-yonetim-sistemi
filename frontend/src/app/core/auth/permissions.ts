/**
 * Backend'deki Permissions sabitlerinin karşılığı.
 * İzin kodları API ile birebir aynı olmalı; yeni izin eklenince burayı da güncelle.
 */
export const Permissions = {
  Manuscripts: {
    Create: 'Manuscript.Create',
    Update: 'Manuscript.Update',
    Delete: 'Manuscript.Delete',
    Submit: 'Manuscript.Submit',
    Decide: 'Manuscript.Decide',
    Publish: 'Manuscript.Publish',
    Unpublish: 'Manuscript.Unpublish',
    ViewAll: 'Manuscript.ViewAll',
  },
  Reviews: {
    Assign: 'Review.Assign',
    Submit: 'Review.Submit',
    ViewAll: 'Review.ViewAll',
  },
  ResearchAreas: {
    Manage: 'ResearchArea.Manage',
  },
  Users: {
    View: 'User.View',
    Manage: 'User.Manage',
  },
  Roles: {
    View: 'Role.View',
    Manage: 'Role.Manage',
  },
  Notifications: {
    View: 'Notification.View',
  },
} as const;
