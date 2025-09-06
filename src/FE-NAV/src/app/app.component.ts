import { Component, OnInit, OnDestroy } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthService } from './services/auth.service';
import { SignalRService } from './services/signalr.service';
import { AsyncPipe, NgIf } from '@angular/common';
import { Subject, takeUntil, take } from 'rxjs';
import { NavbarComponent } from './components/navbar/navbar.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, AsyncPipe, NgIf, NavbarComponent],
  template: `
    <div class="app-container">
      <div *ngIf="authService.isLoading" class="loading">
        Loading...
      </div>
      <app-navbar></app-navbar>
      <main class="main-content">
        <router-outlet></router-outlet>
      </main>
    </div>
  `,
  styles: [`
    .app-container {
      min-height: 100vh;
      background-color: var(--md-background);
      display: flex;
      flex-direction: column;
    }

    .main-content {
      flex: 1;
      background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
      min-height: calc(100vh - 70px);
    }

    .loading {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(255, 255, 255, 0.9);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 9999;
      font-size: var(--md-typography-h6);
      color: var(--md-on-surface-variant);
    }
  `]
})
export class AppComponent implements OnInit, OnDestroy {
  connectionState: 'connected' | 'connecting' | 'disconnected' = 'disconnected';
  showConnectionStatus = false;
  private destroy$ = new Subject<void>();

  constructor(
    public authService: AuthService,
    private signalRService: SignalRService
  ) {}

  async ngOnInit(): Promise<void> {
    this.authService.authState$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(authState => {
      if (authState.isAuthenticated && !authState.isLoading) {
        this.initializeSignalR();
      }
    });

    this.signalRService.connectionState$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(state => {
      this.connectionState = state;
    });
  }

  private async initializeSignalR(): Promise<void> {
    try {
      console.log('Initializing SignalR connection...');
      await this.signalRService.startConnection();
      console.log('SignalR connection established successfully');
    } catch (error) {
      console.error('Failed to initialize SignalR connection:', error);
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
