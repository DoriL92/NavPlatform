import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class OAuthService {
  
  constructor() {}

  // Initiate OAuth login - redirects to .NET backend
  login(): void {
    const authUrl = `${environment.gatewayBaseUrl}/auth/login?returnUrl=${encodeURIComponent(environment.redirectUrl)}`;
    window.location.href = authUrl;
  }

  // Logout - redirects to .NET backend
  logout(): void {
    window.location.href = `${environment.gatewayBaseUrl}/auth/logout`;
  }

  // For .NET backend with cookie-based auth, we don't need token management
  // The backend handles authentication state via cookies
  isAuthenticated(): boolean {
    // This will be determined by the backend's /auth/me endpoint
    return false;
  }
} 