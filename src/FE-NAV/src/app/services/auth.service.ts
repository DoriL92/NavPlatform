import { Injectable, OnDestroy } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError, timer, catchError, Subject } from 'rxjs';
import { catchError as rxjsCatchError, tap, switchMap, retry, takeUntil } from 'rxjs/operators';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';

import { OAuthService } from './oauth.service';

export interface User {
  sub: string;
  name: string;
  email: string;
  roles: string[];
}

export interface AuthState {
  isAuthenticated: boolean;
  isLoading: boolean;
  user: User | null;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService implements OnDestroy {
  private destroy$ = new Subject<void>();
  private authStateSubject = new BehaviorSubject<AuthState>({
    isAuthenticated: false,
    isLoading: true,
    user: null
  });

  public authState$ = this.authStateSubject.asObservable();
  public isAuthenticated$ = this.authStateSubject.pipe(
    switchMap(state => new BehaviorSubject(state.isAuthenticated))
  );
  public currentUser$ = this.authStateSubject.pipe(
    switchMap(state => new BehaviorSubject(state.user))
  );

  constructor(
    private http: HttpClient,
    private router: Router,
    private oauthService: OAuthService
  ) {
    this.initializeSessionManagement();
  }

  private initializeSessionManagement(): void {
    this.setLoading(true);
    
    if (typeof window !== 'undefined') {
      const urlParams = new URLSearchParams(window.location.search);
      if (urlParams.has('logout') || urlParams.has('signedout')) {
        this.updateAuthState({
          isAuthenticated: false,
          isLoading: false,
          user: null
        });
        return;
      }
    }
    
    this.checkAuthenticationStatus();
  }

  private checkAuthenticationStatus(): void {
    // Skip auth check during server-side rendering/build
    if (typeof window === 'undefined') {
      this.updateAuthState({
        isAuthenticated: false,
        isLoading: false,
        user: null
      });
      return;
    }

    this.http.get(`${environment.gatewayBaseUrl}/auth/me`, { withCredentials: true })
      .pipe(
        tap((userData: any) => {
          if (userData && userData.sub) {
            const safeUser = {
              ...userData,
              name: userData.name || 'Unknown',
              email: userData.email || ''
            };
            this.updateAuthState({
              isAuthenticated: true,
              isLoading: false,
              user: safeUser
            });
          } else {
            this.updateAuthState({
              isAuthenticated: false,
              isLoading: false,
              user: null
            });
          }
        }),
        catchError((error: HttpErrorResponse) => {
          this.updateAuthState({
            isAuthenticated: false,
            isLoading: false,
            user: null
          });
          // Don't throw error during development to prevent build issues
          return throwError(() => error);
        })
      )
      .subscribe({
        error: (err) => {
          // Silently handle connection errors during build/development
          console.warn('Auth check failed:', err.message);
        }
      });
  }

  login(): void {
    this.oauthService.login();
  }

  logout(): void {
    this.updateAuthState({
      isAuthenticated: false,
      isLoading: false,
      user: null
    });
    
    this.oauthService.logout();
  }

  refreshAuthStatus(): void {
    this.checkAuthenticationStatus();
  }

  forceLogout(): void {
    this.updateAuthState({
      isAuthenticated: false,
      isLoading: false,
      user: null
    });
  }

  private updateAuthState(newState: Partial<AuthState>): void {
    const currentState = this.authStateSubject.value;
    const updatedState = { ...currentState, ...newState };
    this.authStateSubject.next(updatedState);
  }

  private setLoading(isLoading: boolean): void {
    this.updateAuthState({ isLoading });
  }

  private redirectToLogin(): void {
    this.router.navigate(['/login']);
  }

  get isAuthenticated(): boolean {
    return this.authStateSubject.value.isAuthenticated;
  }

  get isLoading(): boolean {
    return this.authStateSubject.value.isLoading;
  }

  get currentUser(): User | null {
    return this.authStateSubject.value.user;
  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
