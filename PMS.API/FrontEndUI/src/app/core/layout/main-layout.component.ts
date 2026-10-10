import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../auth/auth.service';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  template: `
    <div class="app-shell">
      <aside class="sidebar">
        <div class="brand"><span class="brand-mark">P</span><span>PMS <small>Workspace</small></span></div>
        <div class="nav-label">WORKSPACE</div>
        <nav>
          <a routerLink="/dashboard" routerLinkActive="active"><span>▦</span> Dashboard</a>
          <a routerLink="/projects" routerLinkActive="active"><span>◫</span> Projects</a>
          <a routerLink="/tasks" routerLinkActive="active"><span>✓</span> My Tasks</a>
          @if (auth.hasAnyRole(['Admin'])) {
            <a routerLink="/users" routerLinkActive="active"><span>♙</span> Users</a>
          }
        </nav>
        <div class="sidebar-bottom"><span class="avatar">PM</span><div class="profile"><strong>Signed in</strong><small>PMS account</small></div><button class="icon-button" title="Sign out" (click)="auth.logout()">↗</button></div>
      </aside>
      <main class="main-area">
        <header class="topbar"><div><span class="eyebrow">PROJECT MANAGEMENT</span><h1>Workspace</h1></div><div class="topbar-right"><span class="status-dot"></span><span>Connected to API</span></div></header>
        <section class="page-content"><router-outlet /></section>
      </main>
    </div>
  `
})
export class MainLayoutComponent {
  readonly auth = inject(AuthService);
}
