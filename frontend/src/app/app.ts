import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter, Subscription } from 'rxjs';
import { Permissions } from './core/auth/permissions';
import { AuthService } from './core/services/auth.service';
import { NotificationService } from './core/services/notification.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App implements OnInit, OnDestroy {
  readonly auth = inject(AuthService);
  readonly notifications = inject(NotificationService);
  readonly Permissions = Permissions;
  readonly year = new Date().getFullYear();

  private readonly router = inject(Router);
  private navSub: Subscription | null = null;

  ngOnInit(): void {
    this.refreshNotificationBadge();
    this.navSub = this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe(() => this.refreshNotificationBadge());
  }

  ngOnDestroy(): void {
    this.navSub?.unsubscribe();
  }

  logout(): void {
    this.notifications.clearUnreadCount();
    this.auth.logout();
  }

  private refreshNotificationBadge(): void {
    if (
      this.auth.isLoggedIn() &&
      this.auth.hasPermission(Permissions.Notifications.View)
    ) {
      this.notifications.refreshUnreadCount();
      return;
    }

    this.notifications.clearUnreadCount();
  }
}
