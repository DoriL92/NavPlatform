import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, BehaviorSubject, of } from 'rxjs';
import { catchError, tap, map } from 'rxjs/operators';
import { environment } from '../../environments/environment';

import { 
  Journey, 
  JourneyCreateRequest, 
  JourneyUpdateRequest, 
  PagedJourneyResponse, 
  JourneyQueryParams,
  JourneyShareRequest,
  JourneyShareResponse,
  JourneyPublicLinkResponse,
  JourneyAuditLog,
  TransportType
} from '../models';
import { SignalRService } from './signalr.service';

@Injectable({
  providedIn: 'root'
})
export class JourneyService {
  private baseUrl = `${environment.gatewayBaseUrl}/api/journeys`;
  private journeysSubject = new BehaviorSubject<Journey[]>([]);
  private totalCountSubject = new BehaviorSubject<number>(0);
  private currentPageSubject = new BehaviorSubject<number>(1);
  private pageSizeSubject = new BehaviorSubject<number>(20);


  public journeys$ = this.journeysSubject.asObservable();
  public totalCount$ = this.totalCountSubject.asObservable();
  public currentPage$ = this.currentPageSubject.asObservable();
  public pageSize$ = this.pageSizeSubject.asObservable();

  private notificationSubject = new BehaviorSubject<string | null>(null);
  notification$ = this.notificationSubject.asObservable();

  showNotification(message: string): void {
    this.notificationSubject.next(message);
    setTimeout(() => {
      this.notificationSubject.next(null);
    }, 3000);
  }

  constructor(
    private http: HttpClient,
    private signalR: SignalRService
  ) {}

  // Get a single journey by ID
  getJourney(id: string): Observable<Journey> {
    return this.http.get<Journey>(`${this.baseUrl}/${id}`, {
      withCredentials: true
    }).pipe(
      catchError(this.handleError)
    );
  }

  // Get paged list of journeys
  getJourneys(params: JourneyQueryParams = {}): Observable<PagedJourneyResponse> {
    let httpParams = new HttpParams();
    
    if (params.page) httpParams = httpParams.set('page', params.page.toString());
    if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
    if (params.transportType) httpParams = httpParams.set('transportType', params.transportType);
    if (params.startDate) httpParams = httpParams.set('startDate', params.startDate);
    if (params.endDate) httpParams = httpParams.set('endDate', params.endDate);

    return this.http.get<PagedJourneyResponse>(`${this.baseUrl}`, {
      params: httpParams,
      withCredentials: true
    }).pipe(
        map(r => r ?? { items: [], totalCount: 0, page: params.page ?? 1, pageSize: params.pageSize ?? 20 }),
        tap(response => {
        this.journeysSubject.next(response.items);
        this.totalCountSubject.next(response.totalCount);
        this.currentPageSubject.next(response.page);
        this.pageSizeSubject.next(response.pageSize);
      }),
      catchError(this.handleError)
    );
  }

  createJourney(journey: JourneyCreateRequest): Observable<Journey> {
    const newJourney = {
      ...journey,
      isFavorite: false
    } as JourneyCreateRequest;
    return this.http.post<Journey>(`${this.baseUrl}`, newJourney, {
      withCredentials: true
    }).pipe(
      tap(newJourney => {
        const currentJourneys = this.journeysSubject.value;
        this.journeysSubject.next([newJourney, ...currentJourneys]);
        this.totalCountSubject.next(this.totalCountSubject.value + 1);
      }),
      catchError(this.handleError)
    );
  }

  // Update an existing journey
  updateJourney(id: string, journey: JourneyUpdateRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, journey, {
      withCredentials: true
    }).pipe(
      tap(() => {
        const currentJourneys = this.journeysSubject.value;
        const updatedJourneys = currentJourneys.map(j => 
          j.id === id ? { ...j, ...journey, updatedAt: new Date().toISOString() } : j
        );
        this.journeysSubject.next(updatedJourneys);
      }),
      catchError(this.handleError)
    );
  }

  // Delete a journey
  deleteJourney(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`, {
      withCredentials: true
    }).pipe(
      tap(() => {
        const currentJourneys = this.journeysSubject.value;
        const filteredJourneys = currentJourneys.filter(j => j.id !== id);
        this.journeysSubject.next(filteredJourneys);
        this.totalCountSubject.next(this.totalCountSubject.value - 1);
      }),
      catchError(this.handleError)
    );
  }

  // Share a journey directly with specific users
  shareJourney(id: string, shareRequest: JourneyShareRequest): Observable<JourneyShareResponse> {
    return this.http.post<JourneyShareResponse>(`${this.baseUrl}/${id}/share`, shareRequest, {
      withCredentials: true
    }).pipe(
      tap(response => {
        if (response.success) {
          const currentJourneys = this.journeysSubject.value;
          const updatedJourneys = currentJourneys.map(j => 
            j.id === id ? { 
              ...j, 
              shareCount: response.shareCount,
              lastSharedAt: new Date().toISOString(),
              updatedAt: new Date().toISOString() 
            } : j
          );
          this.journeysSubject.next(updatedJourneys);
          this.showNotification(`Journey shared with ${shareRequest.emails.length} user(s)!`);
        }
      }),
      catchError(this.handleError)
    );
  }

  // Get a public journey by token
  getPublicJourney(token: string): Observable<Journey> {
    return this.http.get<Journey>(`${environment.gatewayBaseUrl}/api/journeys/${token}`, {
      withCredentials: true
    }).pipe(
      catchError(this.handleError)
    );
  }

  // Generate a public link for a journey
  generatePublicLink(id: string): Observable<JourneyPublicLinkResponse> {
    console.log('Making request to:', `${this.baseUrl}/${id}/public-link`);
    return this.http.post<JourneyPublicLinkResponse>(`${this.baseUrl}/${id}/public-link`, {}, {
      withCredentials: true
    }).pipe(
      tap(response => {
        console.log('Service received response:', response);
        console.log('Response type:', typeof response);
        console.log('Response constructor:', response.constructor.name);
        console.log('Response keys:', Object.keys(response));
        console.log('Response.url value:', response.url);
        console.log('Response.url type:', typeof response.url);
        
        // Handle potential response wrapping
        let actualResponse = response;
        if (response && typeof response === 'object' && 'data' in response) {
          actualResponse = (response as any).data;
          console.log('Response was wrapped, unwrapped data:', actualResponse);
        }
        
        const currentJourneys = this.journeysSubject.value;
        const updatedJourneys = currentJourneys.map(j => 
          j.id === id ? { 
            ...j, 
            isPublic: true,
            lastSharedAt: new Date().toISOString(),
            updatedAt: new Date().toISOString() 
          } : j
        );
        this.journeysSubject.next(updatedJourneys);
      }),
      catchError(this.handleError)
    );
  }

  // Unshare a journey
  unshareJourney(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}/share`, {
      withCredentials: true
    }).pipe(
      tap(() => {
        const currentJourneys = this.journeysSubject.value;
        const updatedJourneys = currentJourneys.map(j => 
          j.id === id ? { 
            ...j, 
            isPublic: false, 
            shareId: undefined,
            shareCount: 0,
            lastSharedAt: undefined,
            updatedAt: new Date().toISOString() 
          } : j
        );
        this.journeysSubject.next(updatedJourneys);
      }),
      catchError(this.handleError)
    );
  }

  // Add journey to favorites (idempotent)
  addToFavorites(journeyId: string): Observable<boolean> {
    return this.http.post<void>(`${this.baseUrl}/${journeyId}/favorite`, {}, {
      withCredentials: true
    }).pipe(
      tap(async () => {
        const journeys = this.journeysSubject.value;
        const updatedJourneys = journeys.map(journey => {
          if (journey.id === journeyId) {
            return { ...journey, isFavorite: true };
          }
          return journey;
        });
        this.journeysSubject.next([...updatedJourneys]);
        this.showNotification('Journey added to favorites!');
        try { await this.signalR.subscribeToJourney(journeyId); } catch {}
      }),
      map(() => true),
      catchError(this.handleError)
    );
  }

  // Remove journey from favorites
  removeFromFavorites(journeyId: string): Observable<boolean> {
    return this.http.delete<void>(`${this.baseUrl}/${journeyId}/favorite`, {
      withCredentials: true
    }).pipe(
      tap(async () => {
        const journeys = this.journeysSubject.value;
        const updatedJourneys = journeys.map(journey => {
          if (journey.id === journeyId) {
            return { ...journey, isFavorite: false };
          }
          return journey;
        });
        this.journeysSubject.next([...updatedJourneys]);
        this.showNotification('Journey removed from favorites!');
        try { await this.signalR.unsubscribeFromJourney(journeyId); } catch {}
      }),
      map(() => true),
      catchError(this.handleError)
    );
  }

  // Toggle favorite status 
  toggleFavorite(journeyId: string): Observable<boolean> {
    const journeys = this.journeysSubject.value;
    const journey = journeys.find(j => j.id === journeyId);
    
    if (journey?.isFavorite) {
      return this.removeFromFavorites(journeyId);
    } else {
      return this.addToFavorites(journeyId);
    }
  }

  // Get favorite journeys
  getFavoriteJourneys(): Observable<Journey[]> {
    const journeys = this.journeysSubject.value;
    const favorites = journeys.filter(journey => journey.isFavorite);
    return of(favorites);
  }

  // Refresh journeys for current page
  refreshJourneys(): void {
    const currentParams: JourneyQueryParams = {
      page: this.currentPageSubject.value,
      pageSize: this.pageSizeSubject.value
    };
    this.getJourneys(currentParams).subscribe();
  }

  // Set current page and load journeys
  setPage(page: number): void {
    this.currentPageSubject.next(page);
    const params: JourneyQueryParams = {
      page: page,
      pageSize: this.pageSizeSubject.value
    };
    this.getJourneys(params).subscribe();
  }

  // Set page size and reload journeys
  setPageSize(pageSize: number): void {
    this.pageSizeSubject.next(pageSize);
    this.currentPageSubject.next(1); 
    const params: JourneyQueryParams = {
      page: 1,
      pageSize: pageSize
    };
    this.getJourneys(params).subscribe();
  }

  clearData(): void {
    this.journeysSubject.next([]);
    this.totalCountSubject.next(0);
    this.currentPageSubject.next(1);
    this.pageSizeSubject.next(20);
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An error occurred';
    
    if (error.error instanceof ErrorEvent) {
      errorMessage = error.error.message;
    } else {
      if (error.status === 401) {
        errorMessage = 'Unauthorized - Please log in again';
      } else if (error.status === 400) {
        errorMessage = 'Invalid request - Please check your input';
      } else if (error.status === 404) {
        errorMessage = 'Journey not found';
      } else if (error.status === 500) {
        errorMessage = 'Server error - Please try again later';
      } else {
        errorMessage = `Error ${error.status}: ${error.message}`;
      }
    }

    console.error('Journey service error:', error);
    return throwError(() => new Error(errorMessage));
  }
} 
