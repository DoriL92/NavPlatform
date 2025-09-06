import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <nav class="navbar" *ngIf="authService.isAuthenticated">
      <div class="navbar-container">
        <div class="navbar-brand">
          <h2>Navigation Platform</h2>
        </div>
        
        <div class="navbar-menu">
          <div class="navbar-nav">
            <a 
              routerLink="/listing" 
              routerLinkActive="active"
              class="nav-link"
            >
              <i class="icon-list"></i>
              My Journeys
            </a>
            
            <a 
              *ngIf="isAdmin"
              routerLink="/admin" 
              routerLinkActive="active"
              class="nav-link admin-link"
            >
              <i class="icon-admin"></i>
              Admin Dashboard
            </a>
          </div>
          
          <div class="navbar-user">
            <div class="user-info" *ngIf="authService.currentUser">
              <span class="username">{{ authService.currentUser.name || authService.currentUser.email }}</span>
              <span class="user-role" *ngIf="isAdmin">Admin</span>
            </div>
            <button class="logout-btn" (click)="logout()">
              <i class="icon-logout"></i>
              Logout
            </button>
          </div>
        </div>
        
        <!-- Mobile menu button -->
        <button class="mobile-menu-btn" (click)="toggleMobileMenu()">
          <span class="hamburger-line"></span>
          <span class="hamburger-line"></span>
          <span class="hamburger-line"></span>
        </button>
      </div>
      
      <!-- Mobile menu -->
      <div class="mobile-menu" [class.active]="isMobileMenuOpen">
        <a 
          routerLink="/listing" 
          routerLinkActive="active"
          class="mobile-nav-link"
          (click)="closeMobileMenu()"
        >
          <i class="icon-list"></i>
          My Journeys
        </a>
        
        <a 
          *ngIf="isAdmin"
          routerLink="/admin" 
          routerLinkActive="active"
          class="mobile-nav-link"
          (click)="closeMobileMenu()"
        >
          <i class="icon-admin"></i>
          Admin Dashboard
        </a>
        
        <div class="mobile-user-section">
          <div class="user-info" *ngIf="authService.currentUser">
            <span class="username">{{ authService.currentUser.name || authService.currentUser.email }}</span>
            <span class="user-role" *ngIf="isAdmin">Admin</span>
          </div>
          <button class="logout-btn" (click)="logout()">
            <i class="icon-logout"></i>
            Logout
          </button>
        </div>
      </div>
    </nav>
  `,
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent {
  isMobileMenuOpen = false;

  constructor(
    public authService: AuthService,
    private router: Router
  ) {}

  get isAdmin(): boolean {
    const user = this.authService.currentUser;
    if (!user || !user.roles || !Array.isArray(user.roles)) {
      return false;
    }
    return user.roles.includes('Admin');
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  toggleMobileMenu(): void {
    this.isMobileMenuOpen = !this.isMobileMenuOpen;
  }

  closeMobileMenu(): void {
    this.isMobileMenuOpen = false;
  }
}
