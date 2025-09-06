import { Component, Input, OnDestroy, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil } from 'rxjs';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { DailyGoalService, DailyGoalProgress } from '../../services/daily-goal.service';
import { SignalRService, DailyGoalAchievedEvent } from '../../services/signalr.service';

@Component({
  selector: 'app-daily-goal-progress',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './daily-goal-progress.component.html',
  styleUrls: ['./daily-goal-progress.component.scss'],
  animations: [
    trigger('slideInOut', [
      transition(':enter', [
        style({ transform: 'translateX(-50%) translateY(-20px)', opacity: 0 }),
        animate('300ms ease-out', style({ transform: 'translateX(-50%) translateY(0)', opacity: 1 }))
      ]),
      transition(':leave', [
        animate('200ms ease-in', style({ transform: 'translateX(-50%) translateY(-20px)', opacity: 0 }))
      ])
    ]),
    trigger('progressAnimation', [
      transition('* => *', [
        animate('800ms ease-out')
      ])
    ])
  ]
})
export class DailyGoalProgressComponent implements OnInit, OnChanges, OnDestroy {
  @Input() journeys: any[] = [];
  
  dailyProgress: DailyGoalProgress | null = null;
  showAchievement = false;
  achievementMessage = '';
  showRealTimeNotification = false;
  realTimeNotificationMessage = '';
  
  private destroy$ = new Subject<void>();
  private previousJourneyCount = 0;
  private hasShownNotificationToday = false;
  private currentDay = '';

  constructor(
    private dailyGoalService: DailyGoalService,
    private signalRService: SignalRService
  ) {}

  ngOnInit(): void {
    this.currentDay = new Date().toISOString().split('T')[0];
    
    this.dailyGoalService.dailyGoal$
      .pipe(takeUntil(this.destroy$))
      .subscribe(progress => {
        this.dailyProgress = progress;
        
        if (progress.isNewAchievement && progress.achievedAt && !this.hasShownNotificationToday) {
          this.showAchievementNotification();
          this.hasShownNotificationToday = true;
        }
      });

    this.setupRealTimeNotifications();

    if (this.journeys.length > 0) {
      this.dailyGoalService.calculateDailyProgress(this.journeys);
      this.previousJourneyCount = this.journeys.length;
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['journeys'] && !changes['journeys'].firstChange) {
      const currentCount = this.journeys.length;
      const previousCount = this.previousJourneyCount;
      
      const newDay = new Date().toISOString().split('T')[0];
      if (newDay !== this.currentDay) {
        this.currentDay = newDay;
        this.hasShownNotificationToday = false;
      }
      
      if (currentCount !== previousCount) {
        this.dailyGoalService.calculateDailyProgress(this.journeys);
        this.previousJourneyCount = currentCount;
      }
    }
  }

  private setupRealTimeNotifications(): void {
    this.signalRService.dailyGoalAchieved$
      .pipe(takeUntil(this.destroy$))
      .subscribe(event => {
        this.showRealTimeAchievementNotification(event);
      });

    this.signalRService.connectionState$
      .pipe(takeUntil(this.destroy$))
      .subscribe(state => {
        if (state === 'connected') {
          console.log('SignalR connected - ready for real-time updates');
        } else if (state === 'disconnected') {
          console.log('SignalR disconnected - falling back to local calculations');
        }
      });
  }

  private showRealTimeAchievementNotification(event: DailyGoalAchievedEvent): void {
    console.log('Enhanced daily goal achievement event:', event);
    
    // Update the daily progress with the achievement data
    if (this.dailyProgress) {
      this.dailyProgress = {
        ...this.dailyProgress,
        isAchieved: true,
        currentDistance: event.totalKm,
        progressPercentage: Math.min(100, (event.totalKm / 20) * 100),
        remainingDistance: Math.max(0, 20 - event.totalKm),
        achievedAt: event.occurredOn
      };
    }
    
    this.realTimeNotificationMessage = `Real-time Achievement: ${event.message}`;
    this.showRealTimeNotification = true;
    
    this.showEnhancedAchievementToast(event);
    
    setTimeout(() => {
      this.showRealTimeNotification = false;
    }, 8000);
  }

  private showEnhancedAchievementToast(event: DailyGoalAchievedEvent): void {
    const toast = document.createElement('div');
    toast.style.cssText = `
      position: fixed;
      top: 20px;
      right: 20px;
      background: #28a745;
      color: white;
      padding: 15px;
      border-radius: 6px;
      max-width: 300px;
      z-index: 10000;
      font-size: 14px;
    `;

    const message = document.createElement('div');
    message.textContent = event.isOwnAchievement === false ? 'Someone achieved their daily goal!' : 'Daily goal achieved!';
    message.style.cssText = `
      margin-bottom: 10px;
      font-weight: 600;
    `;

    const details = document.createElement('div');
    details.textContent = `${event.totalKm}km total`;
    details.style.cssText = `
      font-size: 12px;
      opacity: 0.9;
    `;

    toast.appendChild(message);
    toast.appendChild(details);

    document.body.appendChild(toast);

    setTimeout(() => {
      if (toast.parentNode) {
        toast.remove();
      }
    }, 4000);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.resetNotifications();
  }

  private resetNotifications(): void {
    this.showAchievement = false;
    this.showRealTimeNotification = false;
    this.achievementMessage = '';
    this.realTimeNotificationMessage = '';
  }

  updateProgress(): void {
    if (this.journeys.length > 0) {
      this.dailyGoalService.calculateDailyProgress(this.journeys);
    }
  }

  private showAchievementNotification(): void {
    if (this.dailyProgress) {
      this.achievementMessage = this.dailyGoalService.getAchievementMessage(this.dailyProgress);
      this.showAchievement = true;
      
      setTimeout(() => {
        this.showAchievement = false;
      }, 5000);
    }
  }

  getProgressBarColor(): string {
    if (!this.dailyProgress) return '#e0e0e0';
    
    const percentage = this.dailyProgress.progressPercentage;
    if (percentage >= 100) return '#27ae60';
    if (percentage >= 80) return '#f39c12';
    if (percentage >= 50) return '#3498db';
    return '#e74c3c';
  }

  getProgressBarWidth(): string {
    if (!this.dailyProgress) return '0%';
    return `${Math.min(this.dailyProgress.progressPercentage, 100)}%`;
  }

  closeAchievement(): void {
    this.showAchievement = false;
  }

  closeRealTimeNotification(): void {
    this.showRealTimeNotification = false;
  }

  getConnectionStatus(): string {
    const state = this.signalRService.getConnectionState();
    switch (state) {
      case 'Connected':
        return '🟢 Real-time updates active';
      case 'Connecting':
        return '🟡 Connecting to real-time service...';
      case 'Disconnected':
        return '🔴 Real-time updates offline';
      default:
        return '⚪ Connection status unknown';
    }
  }
} 