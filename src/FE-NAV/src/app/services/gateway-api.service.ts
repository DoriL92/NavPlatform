import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class GatewayApiService {
  private base = environment.gatewayBaseUrl;

  constructor(private http: HttpClient) {}

  // getJourneys(): Observable<any> {
  //   return this.http.get(`${this.base}/bff/journeys`, { withCredentials: true });
  // }

  // Add more BFF endpoints as you expose them:
  // createJourney(dto: any) { return this.http.post(`${this.base}/bff/journeys`, dto, { withCredentials: true }); }
}
