import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { JourneyService } from '../../services/journey.service';
import { SignalRService, JourneyUpdatedEvent, JourneyDeletedEvent } from '../../services/signalr.service';
import { Journey, TransportType } from '../../models/journey.model';

@Component({
  selector: 'app-journey-details',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="journey-details-container">
      <div class="details-header">
        <button class="md-button md-button-secondary back-button" (click)="goBack()">
          <span class="material-icons">arrow_back</span>
          Back to Journeys
        </button>
        <h1>Journey Details</h1>
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

        <!-- Sharing Information -->
        <div *ngIf="journey.isPublic || journey.shareCount" class="sharing-section">
          <h3>Sharing Information</h3>
          <div class="sharing-details">
            <div *ngIf="journey.isPublic" class="share-status public">
              <span class="material-icons">public</span>
              <span class="status-text">This journey is public and can be viewed by anyone</span>
            </div>
            <div *ngIf="journey.shareCount" class="share-count">
              <span class="material-icons">share</span>
              <span class="count-text">Shared {{ journey.shareCount }} time{{ journey.shareCount !== 1 ? 's' : '' }}</span>
            </div>
            <div *ngIf="journey.lastSharedAt" class="last-shared">
              <span class="material-icons">schedule</span>
              <span class="time-text">Last shared {{ getTimeAgo(journey.lastSharedAt) }}</span>
            </div>
          </div>
        </div>

        <div class="journey-actions">
          <button class="md-button md-button-primary" (click)="editJourney()">
            <span class="material-icons">edit</span>
            Edit Journey
          </button>
          <button class="md-button md-button-secondary" (click)="goBack()">
            <span class="material-icons">arrow_back</span>
            Back to List
          </button>
        </div>
      </div>

      <div *ngIf="!journey && !loading" class="not-found">
        <h2>Journey Not Found</h2>
        <p>The journey you're looking for doesn't exist.</p>
        <button class="md-button md-button-primary" (click)="goBack()">Go Back</button>
      </div>

      <div *ngIf="loading" class="loading">
        <div class="spinner"></div>
        <p>Loading journey details...</p>
      </div>
    </div>
  `,
  styleUrls: ['./journey-details.component.scss']
})
export class JourneyDetailsComponent implements OnInit, OnDestroy {
  journey: Journey | null = null;
  loading = true;
  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private journeyService: JourneyService,
    private signalRService: SignalRService
  ) {}

  ngOnInit(): void {
    const journeyId = this.route.snapshot.paramMap.get('id');
    if (journeyId) {
      this.loadJourney(journeyId);
      this.setupSignalRSubscriptions(journeyId);
    } else {
      this.loading = false;
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadJourney(id: string): void {
    this.journeyService.getJourney(id).subscribe({
      next: (journey) => {
        this.journey = journey;
        this.loading = false;
      },
      error: (error) => {
        console.error('Failed to load journey:', error);
        this.loading = false;
      }
    });
  }

  private setupSignalRSubscriptions(journeyId: string): void {
    this.signalRService.journeyUpdated$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(event => {
      if (event.journeyId === journeyId) {
        console.log('Real-time journey update received for current journey:', event);
        this.handleJourneyUpdate();
      }
    });

    // Listen for journey deletions
    this.signalRService.journeyDeleted$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(event => {
      if (event.journeyId === journeyId) {
        console.log('Real-time journey deletion received for current journey:', event);
        this.handleJourneyDeletion();
      }
    });
  }

  private handleJourneyUpdate(): void {
    // Reload the journey to get updated data
    if (this.journey) {
      this.loadJourney(this.journey.id);
    }
    
    // Show notification to user
    this.journeyService.showNotification(`This journey has been updated!`);
    
    // Show additional toast notification
    this.showJourneyUpdateToast();
  }

  private handleJourneyDeletion(): void {
    // Show notification and redirect to listing
    this.journeyService.showNotification(`This journey has been deleted.`);
    this.router.navigate(['/listing']);
  }

  goBack(): void {
    this.router.navigate(['/listing']);
  }

  editJourney(): void {
    if (this.journey) {
      this.router.navigate(['/listing'], { 
        queryParams: { edit: this.journey.id } 
      });
    }
  }

  formatDateTime(dateTime: string): string {
    const date = new Date(dateTime);
    return date.toLocaleString('en-US', {
      year: 'numeric',
      month: 'long',
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
    return icons[transportType] || 'rocket';
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

  getTimeAgo(timestamp: string): string {
    const now = new Date();
    const time = new Date(timestamp);
    const diffInMs = now.getTime() - time.getTime();
    const diffInMinutes = Math.floor(diffInMs / (1000 * 60));
    const diffInHours = Math.floor(diffInMs / (1000 * 60 * 60));
    const diffInDays = Math.floor(diffInMs / (1000 * 60 * 60 * 24));

    if (diffInMinutes < 1) return 'Just now';
    if (diffInMinutes < 60) return `${diffInMinutes}m ago`;
    if (diffInHours < 24) return `${diffInHours}h ago`;
    if (diffInDays < 7) return `${diffInDays}d ago`;
    return time.toLocaleDateString();
  }

  private showJourneyUpdateToast(): void {
    const toast = document.createElement('div');
    toast.style.cssText = `
      position: fixed;
      top: 20px;
      right: 20px;
      background: #28a745;
      color: white;
      padding: 15px;
      border-radius: 6px;
      z-index: 10000;
      font-size: 14px;
    `;

    const message = document.createElement('div');
    message.textContent = 'Journey updated!';
    message.style.cssText = `
      font-weight: 600;
    `;

    toast.appendChild(message);
    document.body.appendChild(toast);

    setTimeout(() => {
      if (toast.parentNode) {
        toast.remove();
      }
    }, 3000);
  }
} 