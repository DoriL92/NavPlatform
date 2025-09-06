import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { 
  AdminJourneyFilter, 
  AdminJourneyResponse, 
  MonthlyDistanceResponse, 
  UserStatusUpdate,
  AdminUser,
  AdminUserResponse
} from '../models/admin.model';
import { 
  Journey, 
  JourneyCreateRequest, 
  PagedJourneyResponse, 
  JourneyQueryParams
} from '../models/journey.model';

@Injectable({
  providedIn: 'root'
})
export class AdminService {

  constructor(private http: HttpClient) {}

  getAdminJourneys(filter: AdminJourneyFilter): Observable<AdminJourneyResponse> {
    let params = new HttpParams();
    
    if (filter.userId) params = params.set('UserId', filter.userId);
    if (filter.transportType) params = params.set('TransportType', filter.transportType);
    if (filter.startDateFrom) params = params.set('StartDateFrom', filter.startDateFrom);
    if (filter.startDateTo) params = params.set('StartDateTo', filter.startDateTo);
    if (filter.arrivalDateFrom) params = params.set('ArrivalDateFrom', filter.arrivalDateFrom);
    if (filter.arrivalDateTo) params = params.set('ArrivalDateTo', filter.arrivalDateTo);
    if (filter.minDistance) params = params.set('MinDistance', filter.minDistance.toString());
    if (filter.maxDistance) params = params.set('MaxDistance', filter.maxDistance.toString());
    if (filter.page) params = params.set('Page', filter.page.toString());
    if (filter.pageSize) params = params.set('PageSize', filter.pageSize.toString());
    if (filter.orderBy) params = params.set('OrderBy', filter.orderBy);
    if (filter.direction) params = params.set('Direction', filter.direction);

    return this.http.get<AdminJourneyResponse>(`${environment.gatewayBaseUrl}/api/admin/journeys`, { 
      params,
      withCredentials: true 
    });
  }

  getMonthlyDistanceStats(
    page: number = 1, 
    pageSize: number = 20, 
    orderBy: 'UserId' | 'TotalDistanceKm' = 'TotalDistanceKm'
  ): Observable<MonthlyDistanceResponse> {
    const params = new HttpParams()
      .set('Page', page.toString())
      .set('PageSize', pageSize.toString())
      .set('OrderBy', orderBy);

    return this.http.get<MonthlyDistanceResponse>(`${environment.gatewayBaseUrl}/api/admin/statistics/monthly-distance`, { 
      params,
      withCredentials: true 
    });
  }

  updateUserStatus(userId: string, statusUpdate: UserStatusUpdate): Observable<any> {
    return this.http.patch(`${environment.gatewayBaseUrl}/api/admin/users/${userId}/status`, statusUpdate, {
      withCredentials: true
    });
  }

  getUsers(): Observable<AdminUserResponse> {
    return this.http.get<AdminUserResponse>(`${environment.gatewayBaseUrl}/api/admin/users`, {
      withCredentials: true
    });
  }

  // Regular user journey endpoints for admin users
  getJourneys(params: JourneyQueryParams = {}): Observable<PagedJourneyResponse> {
    let httpParams = new HttpParams();
    
    if (params.page) httpParams = httpParams.set('page', params.page.toString());
    if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
    if (params.transportType) httpParams = httpParams.set('transportType', params.transportType);
    if (params.startDate) httpParams = httpParams.set('startDate', params.startDate);
    if (params.endDate) httpParams = httpParams.set('endDate', params.endDate);
    if (params.userId) httpParams = httpParams.set('userId', params.userId);
    if (params.isPublic !== undefined) httpParams = httpParams.set('isPublic', params.isPublic.toString());

    return this.http.get<PagedJourneyResponse>(`${environment.gatewayBaseUrl}/api/journeys`, { 
      params: httpParams,
      withCredentials: true 
    });
  }

  createJourney(journey: JourneyCreateRequest): Observable<Journey> {
    const newJourney = {
      ...journey,
      isFavorite: false
    } as JourneyCreateRequest;
    return this.http.post<Journey>(`${environment.gatewayBaseUrl}/api/journeys`, newJourney, {
      withCredentials: true
    });
  }
} 