import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-auth-status',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="auth-status">
      <div *ngIf="authService.isLoading" class="loading">
        Loading...
      </div>
      <div class="user-info" *ngIf="!authService.isLoading">
        <span class="user-name">{{ getUserDisplayName() }}</span>
        <button class="md-button md-button-secondary sign-out-btn" (click)="onSignOut()">
          <span class="material-icons">logout</span>
          Sign Out
        </button>
      </div>
    </div>
  `,
  styles: [`
    .auth-status {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .loading {
      color: #666;
      font-size: 0.8rem;
    }

    .user-info {
      display: flex;
      align-items: center;
      gap: 1rem;
    }

    .user-name {
      font-size: 0.9rem;
      color: #333;
      font-weight: 500;
    }

    .sign-out-btn {
      padding: 0.5rem 1rem;
      font-size: 0.8rem;
      min-width: auto;
    }

    .sign-out-btn .material-icons {
      font-size: 16px;
    }
  `]
})
export class AuthStatusComponent {
  constructor(public authService: AuthService) {}

  getUserDisplayName(): string {
    if (this.authService.currentUser?.name) {
      return this.authService.currentUser.name;
    }
    if (this.authService.currentUser?.email) {
      return this.authService.currentUser.email;
    }
    return 'Unknown';
  }

  onSignOut(): void {
    this.authService.logout();
  }
} 