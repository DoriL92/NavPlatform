import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { JourneyService } from '../../services/journey.service';
import { Journey, TransportType } from '../../models/journey.model';

@Component({
  selector: 'app-public-journey',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="public-journey-container">
      <div class="public-header">
        <div class="header-content">
          <h1>Public Journey</h1>
          <p class="subtitle">Shared journey from our navigation platform</p>
        </div>
        <button class="md-button md-button-primary" (click)="goToMainApp()">
          <span class="material-icons">home</span>
          Go to Main App
        </button>
      </div>

      <div *ngIf="journey" class="journey-details">
        <div class="transport-section">
          <div class="transport-icon" [style.background-color]="getTransportColor(journey.transportType)">
            <span class="material-icons">{{ getTransportIcon(journey.transportType) }}</span>
          </div>
          <div class="transport-info">
            <h2>{{ journey.transportType }} Journey</h2>
            <p class="journey-date">{{ formatDateTime(journey.startTime) }}</p>
          </div>
          
          <!-- Daily Goal Achievement Badge -->
          <div class="daily-goal-badge" *ngIf="journey.isDailyGoalAchieved">
            <span class="material-icons">emoji_events</span>
            <span class="badge-text">Daily Goal</span>
          </div>
        </div>

        <div class="route-section">
          <div class="route-point start">
            <div class="point-marker start-marker"></div>
            <div class="point-details">
              <h3>Start</h3>
              <p class="location">{{ journey.startLocation }}</p>
              <p class="time">{{ formatDateTime(journey.startTime) }}</p>
            </div>
          </div>

          <div class="route-line">
            <div class="route-duration">{{ formatDuration(journey.startTime, journey.arrivalTime) }}</div>
          </div>

          <div class="route-point arrival">
            <div class="point-marker arrival-marker"></div>
            <div class="point-details">
              <h3>Destination</h3>
              <p class="location">{{ journey.arrivalLocation }}</p>
              <p class="time">{{ formatDateTime(journey.arrivalTime) }}</p>
            </div>
          </div>
        </div>

        <div class="journey-stats">
          <div class="stat-card">
            <div class="stat-icon">
              <span class="material-icons">straighten</span>
            </div>
            <div class="stat-content">
              <h4>Distance</h4>
              <p class="stat-value">{{ journey.distanceKm }} km</p>
            </div>
          </div>

          <div class="stat-card">
            <div class="stat-icon">
              <span class="material-icons">schedule</span>
            </div>
            <div class="stat-content">
              <h4>Duration</h4>
              <p class="stat-value">{{ formatDuration(journey.startTime, journey.arrivalTime) }}</p>
            </div>
          </div>
        </div>

        <div class="public-info">
          <div class="info-card">
            <span class="material-icons">public</span>
            <div class="info-content">
              <h4>Public Journey</h4>
              <p>This journey has been shared publicly. Anyone with this link can view it.</p>
            </div>
          </div>
        </div>
      </div>

      <div *ngIf="!journey && !loading && !error" class="not-found">
        <h2>Journey Not Found</h2>
        <p>The journey you're looking for doesn't exist or the link has been revoked.</p>
        <button class="md-button md-button-primary" (click)="goToMainApp()">Go to Main App</button>
      </div>

      <div *ngIf="error" class="error">
        <h2>Error Loading Journey</h2>
        <p>{{ error }}</p>
        <button class="md-button md-button-primary" (click)="goToMainApp()">Go to Main App</button>
      </div>

      <div *ngIf="loading" class="loading">
        <div class="spinner"></div>
        <p>Loading journey details...</p>
      </div>
    </div>
  `,
  styleUrls: ['./public-journey.component.scss']
})
export class PublicJourneyComponent implements OnInit, OnDestroy {
  journey: Journey | null = null;
  loading = true;
  error: string | null = null;
  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private journeyService: JourneyService
  ) {}

  ngOnInit(): void {
    const token = this.route.snapshot.paramMap.get('token');
    console.log('PublicJourneyComponent initialized');
    console.log('Token from route:', token);
    console.log('Current URL:', window.location.href);
    
    if (token) {
      this.loadPublicJourney(token);
    } else {
      this.loading = false;
      this.error = 'Invalid journey link';
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadPublicJourney(token: string): void {
    console.log('Loading public journey with token:', token);
    this.journeyService.getPublicJourney(token)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (journey) => {
          console.log('Public journey loaded successfully:', journey);
          this.journey = journey;
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading public journey:', error);
          this.error = 'Failed to load journey. The link may be invalid or expired.';
          this.loading = false;
        }
      });
  }

  goToMainApp(): void {
    window.location.href = 'http://localhost:4200/login';
  }

  formatDateTime(dateTime: string): string {
    const date = new Date(dateTime);
    return date.toLocaleString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  formatDuration(startTime: string, arrivalTime: string): string {
    const start = new Date(startTime);
    const arrival = new Date(arrivalTime);
    const diffMs = arrival.getTime() - start.getTime();
    const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
    const diffMinutes = Math.floor((diffMs % (1000 * 60 * 60)) / (1000 * 60));
    
    if (diffHours > 0) {
      return `${diffHours}h ${diffMinutes}m`;
    }
    return `${diffMinutes}m`;
  }

  getTransportIcon(transportType: TransportType): string {
    const icons: { [key in TransportType]: string } = {
      [TransportType.Car]: 'directions_car',
      [TransportType.Bus]: 'directions_bus',
      [TransportType.Train]: 'train',
      [TransportType.Ferry]: 'flight',
      [TransportType.Plane]: 'directions_bike',
      [TransportType.Bike]: 'directions_walk',
      [TransportType.Walk]: 'motorcycle'
    };
    return icons[transportType] || 'commute';
  }

  getTransportColor(transportType: TransportType): string {
    const colors: { [key in TransportType]: string } = {
      [TransportType.Car]: '#3498db',
      [TransportType.Bus]: '#e67e22',
      [TransportType.Train]: '#9b59b6',
      [TransportType.Ferry]: '#1abc9c',
      [TransportType.Plane]: '#27ae60',
      [TransportType.Bike]: '#95a5a6',
      [TransportType.Walk]: '#e74c3c'
    };
    return colors[transportType] || '#34495e';
  }
}
