import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthProfile } from '../../shared/models/user.model';
import { AuthService } from '../auth/auth.service';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [FormsModule, RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.css'
})
export class MainLayoutComponent {
  readonly auth = inject(AuthService);
  readonly profile = signal<AuthProfile | null>(null);
  readonly profileMenuOpen = signal(false);
  readonly viewingProfile = signal(false);
  readonly changingPassword = signal(false);
  readonly loadingProfile = signal(false);
  readonly savingPassword = signal(false);
  readonly profileError = signal('');
  readonly passwordError = signal('');
  readonly passwordMessage = signal('');
  passwordForm = { currentPassword: '', newPassword: '', confirmPassword: '' };

  constructor() {
    this.loadProfile();
  }

  loadProfile(): void {
    this.loadingProfile.set(true);
    this.profileError.set('');
    this.auth.getProfile().pipe(finalize(() => this.loadingProfile.set(false))).subscribe({
      next: profile => this.profile.set(profile),
      error: (err: HttpErrorResponse) => this.profileError.set(err.status === 401
        ? 'Your session may have expired. Please sign in again.'
        : 'Could not load your profile. Try again.')
    });
  }

  toggleProfileMenu(): void {
    this.profileMenuOpen.update(open => !open);
  }

  openProfile(): void {
    this.profileMenuOpen.set(false);
    this.viewingProfile.set(true);
  }

  closeProfile(): void {
    this.viewingProfile.set(false);
  }

  openChangePassword(): void {
    this.profileMenuOpen.set(false);
    this.passwordError.set('');
    this.passwordMessage.set('');
    this.passwordForm = { currentPassword: '', newPassword: '', confirmPassword: '' };
    this.changingPassword.set(true);
  }

  closeChangePassword(): void {
    if (!this.savingPassword()) this.changingPassword.set(false);
  }

  submitPasswordChange(): void {
    if (this.savingPassword() || this.passwordForm.newPassword !== this.passwordForm.confirmPassword) return;

    this.savingPassword.set(true);
    this.passwordError.set('');
    this.passwordMessage.set('');
    this.auth.changePassword({
      currentPassword: this.passwordForm.currentPassword,
      newPassword: this.passwordForm.newPassword
    }).pipe(finalize(() => this.savingPassword.set(false))).subscribe({
      next: response => {
        this.passwordMessage.set(response.message || 'Password changed successfully.');
        this.passwordForm = { currentPassword: '', newPassword: '', confirmPassword: '' };
      },
      error: (err: HttpErrorResponse) => {
        const errors = err.error?.errors;
        this.passwordError.set(Array.isArray(errors)
          ? errors.join(' ')
          : err.status === 401
            ? 'Your current password is incorrect or your session has expired.'
            : 'Could not change your password. Check your current password and try again.');
      }
    });
  }

  displayName(): string {
    const currentProfile = this.profile();
    return currentProfile?.displayName?.trim()
      || currentProfile?.userName?.trim()
      || currentProfile?.email?.trim()
      || 'Profile';
  }

  profileInitial(): string {
    return this.displayName().charAt(0).toUpperCase();
  }
}
