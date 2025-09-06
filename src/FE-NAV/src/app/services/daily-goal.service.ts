import { Injectable, OnDestroy } from '@angular/core';
import { BehaviorSubject, Observable, map, takeUntil, Subject } from 'rxjs';
import { Journey } from '../models/journey.model';
import { SignalRService, DailyGoalAchievedEvent } from './signalr.service';

export interface DailyGoalProgress {
  currentDistance: number;
  targetDistance: number;
  isAchieved: boolean;
  progressPercentage: number;
  remainingDistance: number;
  achievedAt?: string;
  isNewAchievement?: boolean; // Flag to indicate if this is a new achievement
}

@Injectable({
  providedIn: 'root'
})
export class DailyGoalService implements OnDestroy {
  private readonly DAILY_GOAL_KM = 20;
  private dailyGoalSubject = new BehaviorSubject<DailyGoalProgress>({
    currentDistance: 0,
    targetDistance: this.DAILY_GOAL_KM,
    isAchieved: false,
    progressPercentage: 0,
    remainingDistance: this.DAILY_GOAL_KM
  });

  public dailyGoal$ = this.dailyGoalSubject.asObservable();
  private destroy$ = new Subject<void>();
  
  // Track when we last showed a notification to prevent duplicates
  private lastNotificationDate: string | null = null;

  constructor(private signalRService: SignalRService) {
    this.setupSignalRListeners();
  }

  /**
   * Set up SignalR listeners for real-time updates
   */
  private setupSignalRListeners(): void {
    // Listen for real-time daily goal achievements
    this.signalRService.dailyGoalAchieved$
      .pipe(takeUntil(this.destroy$))
      .subscribe(event => {
        console.log('Real-time daily goal achievement received:', event);
        this.handleRealTimeAchievement(event);
      });

    // Listen for connection state changes
    this.signalRService.connectionState$
      .pipe(takeUntil(this.destroy$))
      .subscribe(state => {
        console.log('SignalR connection state:', state);
        if (state === 'connected') {
          // Connection established, can now receive real-time updates
          console.log('Ready to receive real-time daily goal updates');
        }
      });
  }

  /**
   * Handle real-time achievement events from backend
   */
  private handleRealTimeAchievement(event: DailyGoalAchievedEvent): void {
    console.log('Enhanced daily goal achievement received:', event);
    
    const currentProgress = this.dailyGoalSubject.value;
    
    // Update progress with real-time data from SignalR
    const updatedProgress: DailyGoalProgress = {
      ...currentProgress,
      isAchieved: true,
      currentDistance: event.totalKm, // Use the total distance from the achievement event
      progressPercentage: Math.min(100, (event.totalKm / this.DAILY_GOAL_KM) * 100),
      remainingDistance: Math.max(0, this.DAILY_GOAL_KM - event.totalKm),
      achievedAt: event.occurredOn
    };

    this.dailyGoalSubject.next(updatedProgress);
    
    // Show achievement notification (this will trigger the UI)
    this.showAchievementNotification(updatedProgress);
  }

  /**
   * Calculate daily goal progress from a list of journeys
   * @param journeys List of journeys to calculate from
   * @param targetDate Optional target date (defaults to today)
   */
  calculateDailyProgress(journeys: Journey[], targetDate?: Date): DailyGoalProgress {
    const date = targetDate || new Date();
    const today = date.toISOString().split('T')[0]; // YYYY-MM-DD format
    
    const startOfDay = new Date(date.getFullYear(), date.getMonth(), date.getDate());
    const endOfDay = new Date(startOfDay.getTime() + 24 * 60 * 60 * 1000 - 1);

    // Filter journeys for the target date
    const dailyJourneys = journeys.filter(journey => {
      const journeyDate = new Date(journey?.startTime);
      return journeyDate >= startOfDay && journeyDate <= endOfDay;
    });

    // Calculate total distance
    const currentDistance = dailyJourneys.reduce((total, journey) => total + journey.distanceKm, 0);
    
    // Check if goal is achieved
    const isAchieved = currentDistance >= this.DAILY_GOAL_KM;
    
    // Get current progress state
    const currentProgress = this.dailyGoalSubject.value;
    
    // Find when goal was first achieved (only if this is a new calculation)
    let achievedAt: string | undefined;
    let shouldNotifyBackend = false;
    let isNewAchievement = false;
    
    if (isAchieved) {
      if (!currentProgress.isAchieved) {
        // This is a new achievement - find when it happened
        const sortedJourneys = [...dailyJourneys].sort((a, b) => 
          new Date(a.startTime).getTime() - new Date(b.startTime).getTime()
        );
        
        let runningTotal = 0;
        for (const journey of sortedJourneys) {
          runningTotal += journey.distanceKm;
          if (runningTotal >= this.DAILY_GOAL_KM) {
            achievedAt = journey.startTime;
            shouldNotifyBackend = true;
            // Only mark as new achievement if we haven't shown notification today
            isNewAchievement = this.shouldShowNotification();
            if (isNewAchievement) {
              this.markNotificationShown();
            }
            break;
          }
        }
      } else {
        // Goal was already achieved, keep the existing achievedAt
        achievedAt = currentProgress.achievedAt;
        // Don't show notification again for the same day
        isNewAchievement = false;
      }
    }

    const progress: DailyGoalProgress = {
      currentDistance: Math.round(currentDistance * 100) / 100,
      targetDistance: this.DAILY_GOAL_KM,
      isAchieved,
      progressPercentage: Math.min((currentDistance / this.DAILY_GOAL_KM) * 100, 100),
      remainingDistance: Math.max(this.DAILY_GOAL_KM - currentDistance, 0),
      achievedAt: achievedAt || currentProgress.achievedAt,
      // Add a flag to indicate if this is a new achievement
      isNewAchievement: isNewAchievement
    };

    // Only notify backend if this is a genuine new achievement
    if (shouldNotifyBackend && this.signalRService.isConnected()) {
      this.notifyBackendOfAchievement(achievedAt!, currentDistance, dailyJourneys);
    }

    this.dailyGoalSubject.next(progress);
    return progress;
  }

  /**
   * Notify backend of daily goal achievement via SignalR
   */
  private async notifyBackendOfAchievement(achievedAt: string, totalDistance: number, journeys: Journey[]): Promise<void> {
    try {
      // Find the journey that triggered the achievement
      const achievementTrigger = this.findAchievementTrigger(journeys);
      
      const achievementEvent: DailyGoalAchievedEvent = {
        journeyId: achievementTrigger?.id ? parseInt(achievementTrigger.id.toString()) : 0,
        ownerUserId: this.getCurrentUserId() || 'unknown',
        day: new Date().toISOString().split('T')[0],
        totalKm: totalDistance,
        occurredOn: achievedAt,
        message: `Daily goal of ${this.DAILY_GOAL_KM}km achieved!`,
        isOwnAchievement: true
      };

      // await this.signalRService.sendDailyGoalAchieved(achievementEvent);
      
      console.log('Notified backend of daily goal achievement:', achievementEvent);
    } catch (error) {
      console.error('Error notifying backend of achievement:', error);
    }
  }

  /**
   * Find the journey that triggered the 20km achievement
   */
  private findAchievementTrigger(journeys: Journey[]): Journey | undefined {
    const sortedJourneys = [...journeys].sort((a, b) => 
      new Date(a.startTime).getTime() - new Date(b.startTime).getTime()
    );
    
    let runningTotal = 0;
    for (const journey of sortedJourneys) {
      runningTotal += journey.distanceKm;
      if (runningTotal >= this.DAILY_GOAL_KM) {
        return journey;
      }
    }
    return undefined;
  }

  /**
   * Check if a specific journey would trigger the daily goal achievement
   * @param existingJourneys Current journeys for the day
   * @param newJourney New journey to be added
   * @returns True if this journey would trigger the achievement
   */
  wouldTriggerAchievement(existingJourneys: Journey[], newJourney: Journey): boolean {
    const currentTotal = existingJourneys.reduce((total, journey) => total + journey.distanceKm, 0);
    const newTotal = currentTotal + newJourney.distanceKm;
    
    return currentTotal < this.DAILY_GOAL_KM && newTotal >= this.DAILY_GOAL_KM;
  }

  /**
   * Get achievement message based on progress
   */
  getAchievementMessage(progress: DailyGoalProgress): string {
    if (progress.isAchieved) {
      return `🎉 Congratulations! You've achieved your daily goal of ${progress.targetDistance}km!`;
    } else if (progress.progressPercentage >= 80) {
      return `🔥 Almost there! Just ${progress.remainingDistance.toFixed(2)}km more to reach your daily goal!`;
    } else if (progress.progressPercentage >= 50) {
      return `💪 Great progress! You're halfway to your ${progress.targetDistance}km daily goal!`;
    } else if (progress.progressPercentage >= 25) {
      return `🚀 Good start! Keep going to reach your ${progress.targetDistance}km daily goal!`;
    } else {
      return `🎯 Start your journey! Your daily goal is ${progress.targetDistance}km.`;
    }
  }

  /**
   * Show achievement notification (this will be called by the component)
   */
  showAchievementNotification(progress: DailyGoalProgress): void {
    // This method will be called by the component to show the notification
    // The component will handle the actual UI display
    console.log('Achievement notification should be shown:', progress);
  }

  /**
   * Reset daily progress (useful for testing or when switching days)
   */
  resetDailyProgress(): void {
    this.lastNotificationDate = null;
    this.dailyGoalSubject.next({
      currentDistance: 0,
      targetDistance: this.DAILY_GOAL_KM,
      isAchieved: false,
      progressPercentage: 0,
      remainingDistance: this.DAILY_GOAL_KM
    });
  }

  /**
   * Check if we should show a notification for today
   */
  private shouldShowNotification(): boolean {
    const today = new Date().toISOString().split('T')[0];
    return this.lastNotificationDate !== today;
  }

  /**
   * Mark notification as shown for today
   */
  private markNotificationShown(): void {
    this.lastNotificationDate = new Date().toISOString().split('T')[0];
  }

  /**
   * Check if we're still in the same day (useful for preventing false notifications)
   */
  isSameDay(date1: Date, date2: Date): boolean {
    return date1.toISOString().split('T')[0] === date2.toISOString().split('T')[0];
  }

  /**
   * Get current day string
   */
  getCurrentDay(): string {
    return new Date().toISOString().split('T')[0];
  }

  /**
   * Get current user ID (implement based on your auth system)
   */
  private getCurrentUserId(): string | null {
    const userStr = localStorage.getItem('current_user');
    if (userStr) {
      try {
        const user = JSON.parse(userStr);
        return user.id;
      } catch {
        return null;
      }
    }
    return null;
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
} 