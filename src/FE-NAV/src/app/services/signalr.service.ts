// src/app/services/signalr.service.ts
import { Injectable, OnDestroy } from '@angular/core';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  HttpTransportType,
  LogLevel
} from '@microsoft/signalr';
import { BehaviorSubject, Subject } from 'rxjs';
import { environment } from '../../environments/environment';

export interface DailyGoalAchievedEvent {
  journeyId: number;
  ownerUserId: string;
  day: string;
  totalKm: number;
  occurredOn: string;
  journeyInfo?: {
    id: number;
    startLocation: string;
    arrivalLocation: string;
    transportType: string;
    distanceKm: number;
    startTime: string;
  };
  userInfo?: {
    id: string;
    name: string;
    email: string;
  };
  message: string;
  isOwnAchievement?: boolean;
}
export interface JourneySharedEvent {
  journeyId: string;
  sharedByUserId: string;
  sharedWithemails: string[];
  timestamp: string;
}
export interface FavoriteToggleEvent {
  journeyId: string;
  userId: string;
  isFavorited: boolean;
  timestamp: string;
}
export interface NotificationEvent {
  id: string;
  userId: string;
  message: string;
  timestamp: string;
  type: 'achievement' | 'share' | 'favorite' | 'general';
}
export interface JourneyUpdatedEvent { 
  journeyId: string;
  ownerUserId: string;
  occurredOn: string;
  journeyInfo?: {
    id: number;
    startLocation: string;
    arrivalLocation: string;
    transportType: string;
    distanceKm: number;
    updatedAt: string;
  };
  userInfo?: {
    id: string;
    name: string;
    email: string;
  };
  message: string;
}
export interface JourneyDeletedEvent { journeyId: string; }

type ConnState = 'disconnected' | 'connecting' | 'connected';

@Injectable({ providedIn: 'root' })
export class SignalRService implements OnDestroy {
  private hub?: HubConnection;

  private connectionStateSubject = new BehaviorSubject<ConnState>('disconnected');
  private dailyGoalAchievedSubject = new Subject<DailyGoalAchievedEvent>();
  private journeySharedSubject = new Subject<JourneySharedEvent>();
  private favoriteToggleSubject = new Subject<FavoriteToggleEvent>();
  private notificationSubject = new Subject<NotificationEvent>();
  private journeyUpdatedSubject = new Subject<JourneyUpdatedEvent>();
  private journeyDeletedSubject = new Subject<JourneyDeletedEvent>();

  connectionState$ = this.connectionStateSubject.asObservable();
  dailyGoalAchieved$ = this.dailyGoalAchievedSubject.asObservable();
  journeyShared$ = this.journeySharedSubject.asObservable();
  favoriteToggle$ = this.favoriteToggleSubject.asObservable();
  notifications$ = this.notificationSubject.asObservable();
  journeyUpdated$ = this.journeyUpdatedSubject.asObservable();
  journeyDeleted$ = this.journeyDeletedSubject.asObservable();

  constructor() {}

  async startConnection(): Promise<void> {
    if (this.hub && this.hub.state === HubConnectionState.Connected) return;

    this.connectionStateSubject.next('connecting');
    
    // Debug: Log the environment configuration
    console.log('SignalR Environment:', {
      gatewayBaseUrl: environment.gatewayBaseUrl,
      signalRUrl: `${environment.gatewayBaseUrl}/hubs/journeys`,
      environment: environment
    });

    // Build the connection. Cookie-based auth → withCredentials: true
    this.hub = new HubConnectionBuilder()
      .withUrl(`${environment.gatewayBaseUrl}/hubs/journeys`, {
         accessTokenFactory: async () => {
      try {
        console.log('Fetching access token for SignalR...');
        console.log('Token URL:', `${environment.gatewayBaseUrl}/auth/token`);
        const res = await fetch(`${environment.gatewayBaseUrl}/auth/token`, {
          credentials: 'include'
        });
        console.log('Token response status:', res.status);
        if (!res.ok) {
          console.warn('Token request failed:', res.status, res.statusText);
          return '';
        }
        const data = await res.json();
        const token = data.access_token ?? '';
        console.log('Token received:', token ? 'Yes' : 'No');
        return token;
      } catch (error) {
        console.error('Token fetch error:', error);
        return '';
      }
    },
    // Let SignalR negotiate the best transport automatically
    // transport: HttpTransportType.WebSockets, // optional
    // skipNegotiation: true,                // only if your server supports WS directly
  })

      .withAutomaticReconnect([0, 2000, 10000, 30000])
      .configureLogging(LogLevel.Information)
      .build();

    this.setupEventHandlers();

    try {
      console.log('Starting SignalR connection...');
      await this.hub.start();
      this.connectionStateSubject.next('connected');
      console.log('SignalR connected successfully');
      await this.joinUserGroups();
    } catch (err) {
      this.connectionStateSubject.next('disconnected');
      console.error('SignalR start failed:', err);
      
      // Log more detailed error information
      if (err instanceof Error) {
        console.error('Error message:', err.message);
        console.error('Error stack:', err.stack);
      }
      
      throw err;
    }
  }

  private setupEventHandlers(): void {
    if (!this.hub) return;

    this.hub.on('DailyGoalAchieved', (e: DailyGoalAchievedEvent) => {
      this.dailyGoalAchievedSubject.next(e);
    });
    this.hub.on('JourneyShared', (e: JourneySharedEvent) => {
      this.journeySharedSubject.next(e);
    });
    this.hub.on('FavoriteToggled', (e: FavoriteToggleEvent) => {
      this.favoriteToggleSubject.next(e);
    });
    this.hub.on('Notification', (e: NotificationEvent) => {
      this.notificationSubject.next(e);
    });
    this.hub.on('JourneyUpdated', (e: JourneyUpdatedEvent) => {
      this.journeyUpdatedSubject.next(e);
    });
    this.hub.on('JourneyDeleted', (e: JourneyDeletedEvent) => {
      this.journeyDeletedSubject.next(e);
    });

    this.hub.onreconnecting(err => {
      console.warn('SignalR reconnecting', err);
      this.connectionStateSubject.next('connecting');
    });

    this.hub.onreconnected(_id => {
      console.info('SignalR reconnected');
      this.connectionStateSubject.next('connected');
      this.joinUserGroups().catch(console.error);
    });

    this.hub.onclose(err => {
      console.warn('SignalR closed', err);
      this.connectionStateSubject.next('disconnected');
    });
  }

  private async joinUserGroups(): Promise<void> {
    if (!this.hub || this.hub.state !== HubConnectionState.Connected) return;
    try {
      const userId = this.getCurrentUserId();
      if (userId) {
        await this.hub.invoke('JoinUserGroup', userId);
        await this.hub.invoke('JoinDailyGoalGroup', userId);
        console.log('Joined groups for', userId);
      }
    } catch (err) {
      console.error('joinUserGroups failed', err);
    }
  }

  async sendDailyGoalAchieved(payload: DailyGoalAchievedEvent): Promise<void> {
    if (this.hub?.state === HubConnectionState.Connected) {
      await this.hub.invoke('DailyGoalAchieved', payload);
    }
  }

  async shareJourney(journeyId: string, userIds: string[]): Promise<void> {
    if (this.hub?.state === HubConnectionState.Connected) {
      await this.hub.invoke('ShareJourney', journeyId, userIds);
    }
  }

  async toggleFavorite(journeyId: string, isFavorited: boolean): Promise<void> {
    if (this.hub?.state === HubConnectionState.Connected) {
      await this.hub.invoke('ToggleFavorite', journeyId, isFavorited);
    }
  }

  async subscribeToJourney(journeyId: string): Promise<void> {
    if (this.hub?.state === HubConnectionState.Connected) {
      await this.hub.invoke('SubscribeToJourney', journeyId);
    }
  }

  async unsubscribeFromJourney(journeyId: string): Promise<void> {
    if (this.hub?.state === HubConnectionState.Connected) {
      await this.hub.invoke('UnsubscribeFromJourney', journeyId);
    }
  }

  isConnected(): boolean {
    return this.hub?.state === HubConnectionState.Connected;
  }

  getConnectionState(): string {
    return this.hub ? HubConnectionState[this.hub.state] : 'Disconnected';
  }

  async stopConnection(): Promise<void> {
    if (this.hub) {
      await this.hub.stop();
      this.connectionStateSubject.next('disconnected');
      console.log('SignalR disconnected');
    }
  }

  // TODO: replace with your real auth service if you have one
  private getCurrentUserId(): string | null {
    try {
      const raw = localStorage.getItem('current_user');
      return raw ? JSON.parse(raw).id ?? null : null;
    } catch {
      return null;
    }
  }

  ngOnDestroy(): void {
    void this.stopConnection();
  }
}
