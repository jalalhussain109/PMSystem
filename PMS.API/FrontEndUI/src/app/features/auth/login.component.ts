import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule],
  template: `
    <main class="login-page">
      <section class="login-art">
        <div class="brand brand-light"><span class="brand-mark">P</span><span>PMS <small>Workspace</small></span></div>
        <div class="art-copy"><span class="pill">PLAN · TRACK · DELIVER</span><h1>Great work<br>starts with<br><em>clarity.</em></h1><p>Bring your projects, people, and priorities together in one focused workspace.</p></div>
        <div class="art-footer">PROJECT MANAGEMENT SYSTEM <span>© 2026</span></div>
      </section>
      <section class="login-panel"><form class="login-form" (ngSubmit)="submit()">
        <div class="mobile-brand brand"><span class="brand-mark">P</span><span>PMS <small>Workspace</small></span></div>
        <span class="eyebrow">WELCOME BACK</span><h2>Sign in to PMS</h2><p class="muted">Enter your account details to continue.</p>
        <label for="username">Username</label><input id="username" name="userName" [(ngModel)]="userName" autocomplete="username" placeholder="Enter your username" required>
        <label for="password">Password</label><input id="password" name="password" [(ngModel)]="password" type="password" autocomplete="current-password" placeholder="Enter your password" required>
        @if (error()) { <div class="error-message">{{ error() }}</div> }
        <button class="primary-button login-button" type="submit" [disabled]="loading()">{{ loading() ? 'Signing in…' : 'Sign in' }} <span>→</span></button>
        <p class="login-hint">Use an account created by your PMS administrator.</p>
      </form></section>
    </main>
  `
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  userName = '';
  password = '';
  readonly loading = signal(false);
  readonly error = signal('');

  submit(): void {
    this.error.set('');
    this.loading.set(true);
    this.auth.login({ userName: this.userName.trim(), password: this.password }).subscribe({
      next: () => {
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') || '/dashboard';
        void this.router.navigateByUrl(returnUrl);
      },
      error: err => {
        this.error.set(err?.status === 401 ? 'Invalid username or password.' : 'Unable to sign in. Check the API connection and try again.');
        this.loading.set(false);
      },
      complete: () => this.loading.set(false)
    });
  }
}
