import { Routes } from '@angular/router';
import { PostList } from './features/posts/post-list/post-list';
import { PostDetailPage } from './features/posts/post-detail/post-detail';
import { LoginPage } from './features/auth/login/login';
import { AdminPostList } from './features/admin/admin-post-list/admin-post-list';
import { AdminPostForm } from './features/admin/admin-post-form/admin-post-form';
import { AdminNotifications } from './features/admin/admin-notifications/admin-notifications';
import { authGuard } from './core/guards/auth.guard';

/**
 * Routes = URL → hangi sayfa?
 * Public: / , /posts/:slug
 * Admin:  /admin/login , /admin , /admin/posts/new , /admin/posts/:id/edit , /admin/notifications
 */
export const routes: Routes = [
  { path: '', component: PostList },
  { path: 'posts/:slug', component: PostDetailPage },
  { path: 'admin/login', component: LoginPage },
  {
    path: 'admin',
    canActivate: [authGuard],
    children: [
      { path: '', component: AdminPostList },
      { path: 'posts/new', component: AdminPostForm },
      { path: 'posts/:id/edit', component: AdminPostForm },
      { path: 'notifications', component: AdminNotifications },
    ],
  },
  { path: '**', redirectTo: '' },
];
