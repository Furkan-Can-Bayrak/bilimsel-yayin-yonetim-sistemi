import { Routes } from '@angular/router';
import { ManuscriptList } from './features/manuscripts/manuscript-list/manuscript-list';
import { ManuscriptDetailPage } from './features/manuscripts/manuscript-detail/manuscript-detail';
import { LoginPage } from './features/auth/login/login';
import { AdminManuscriptList } from './features/admin/admin-manuscript-list/admin-manuscript-list';
import { AdminManuscriptForm } from './features/admin/admin-manuscript-form/admin-manuscript-form';
import { AdminNotifications } from './features/admin/admin-notifications/admin-notifications';
import { AdminReviewQueue } from './features/admin/admin-review-queue/admin-review-queue';
import { AdminReviewForm } from './features/admin/admin-review-form/admin-review-form';
import { AdminResearchAreas } from './features/admin/admin-research-areas/admin-research-areas';
import { AdminUsers } from './features/admin/admin-users/admin-users';
import { authGuard, permissionGuard } from './core/guards/auth.guard';
import { Permissions } from './core/auth/permissions';

export const routes: Routes = [
  { path: '', component: ManuscriptList },
  { path: 'manuscripts/:slug', component: ManuscriptDetailPage },
  { path: 'admin/login', component: LoginPage },
  {
    path: 'admin',
    canActivate: [authGuard],
    children: [
      {
        path: '',
        component: AdminManuscriptList,
        canActivate: [permissionGuard(Permissions.Manuscripts.ViewAll)],
        data: { editorPanel: true },
      },
      {
        path: 'mine',
        component: AdminManuscriptList,
        canActivate: [permissionGuard(Permissions.Manuscripts.Create)],
        data: { editorPanel: false },
      },
      {
        path: 'manuscripts/new',
        component: AdminManuscriptForm,
        canActivate: [permissionGuard(Permissions.Manuscripts.Create)],
      },
      {
        path: 'manuscripts/:id/edit',
        component: AdminManuscriptForm,
        canActivate: [permissionGuard(Permissions.Manuscripts.Update)],
      },
      {
        path: 'reviews',
        component: AdminReviewQueue,
        canActivate: [permissionGuard(Permissions.Reviews.Submit)],
      },
      {
        path: 'reviews/:id',
        component: AdminReviewForm,
        canActivate: [permissionGuard(Permissions.Reviews.Submit)],
      },
      {
        path: 'research-areas',
        component: AdminResearchAreas,
        canActivate: [permissionGuard(Permissions.ResearchAreas.Manage)],
      },
      {
        path: 'users',
        component: AdminUsers,
        canActivate: [permissionGuard(Permissions.Users.Manage)],
      },
      {
        path: 'notifications',
        component: AdminNotifications,
        canActivate: [permissionGuard(Permissions.Notifications.View)],
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
