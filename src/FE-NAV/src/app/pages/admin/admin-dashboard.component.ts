import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { AdminService } from '../../services/admin.service';
import { AuthService } from '../../services/auth.service';
import { AdminJourneyFilter, MonthlyDistanceStat, AdminUser, AdminUserResponse } from '../../models/admin.model';
import { TransportType } from '../../models';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  template: `
    <div class="admin-dashboard">
      <div class="dashboard-header">
        <div class="header-content">
          <div class="header-left">
            <h1>Admin Dashboard</h1>
            <p>Monitor journeys, view statistics, and manage users</p>
          </div>
        </div>
      </div>

      <div class="dashboard-tabs">
        <button 
          [class.active]="activeTab === 'journeys'" 
          (click)="setActiveTab('journeys')"
          class="tab-button">
          <span class="material-icons">directions_car</span>
          Journey Monitoring
        </button>
        <button 
          [class.active]="activeTab === 'statistics'" 
          (click)="setActiveTab('statistics')"
          class="tab-button">
          <span class="material-icons">analytics</span>
          Monthly Statistics
        </button>
        <button 
          [class.active]="activeTab === 'users'" 
          (click)="setActiveTab('users')"
          class="tab-button">
          <span class="material-icons">people</span>
          User Management
        </button>
      </div>

      <!-- Journey Monitoring Tab -->
      <div *ngIf="activeTab === 'journeys'" class="tab-content">
        <div class="filter-section">
          <h3>Journey Filters</h3>
          <form [formGroup]="journeyFilterForm" (ngSubmit)="applyJourneyFilters()" class="filter-form">
            <div class="form-row">
              <div class="form-group">
                <label>User ID</label>
                <input type="text" formControlName="userId" placeholder="Enter user ID">
              </div>
              <div class="form-group">
                <label>Transport Type</label>
                <select formControlName="transportType">
                  <option value="">All Types</option>
                  <option [value]="TransportType.Car">Car</option>
                  <option [value]="TransportType.Bus">Bus</option>
                  <option [value]="TransportType.Train">Train</option>
                  <option [value]="TransportType.Ferry">Ferry</option>
                  <option [value]="TransportType.Plane">Plane</option>
                  <option [value]="TransportType.Bike">Bike</option>
                  <option [value]="TransportType.Walk">Walk</option>
                </select>
              </div>
            </div>
            
            <div class="form-row">
              <div class="form-group">
                <label>Start Date From</label>
                <input type="date" formControlName="startDateFrom">
              </div>
              <div class="form-group">
                <label>Start Date To</label>
                <input type="date" formControlName="startDateTo">
              </div>
            </div>

            <div class="form-row">
              <div class="form-group">
                <label>Arrival Date From</label>
                <input type="date" formControlName="arrivalDateFrom">
              </div>
              <div class="form-group">
                <label>Arrival Date To</label>
                <input type="date" formControlName="arrivalDateTo">
              </div>
            </div>

            <div class="form-row">
              <div class="form-group">
                <label>Min Distance (km)</label>
                <input type="number" formControlName="minDistance" min="0">
              </div>
              <div class="form-group">
                <label>Max Distance (km)</label>
                <input type="number" formControlName="maxDistance" min="0">
              </div>
            </div>

            <div class="form-row">
              <div class="form-group">
                <label>Order By</label>
                <select formControlName="orderBy">
                  <option value="startTime">Start Time</option>
                  <option value="arrivalTime">Arrival Time</option>
                  <option value="distanceKm">Distance</option>
                  <option value="createdAt">Created At</option>
                </select>
              </div>
              <div class="form-group">
                <label>Direction</label>
                <select formControlName="direction">
                  <option value="desc">Descending</option>
                  <option value="asc">Ascending</option>
                </select>
              </div>
            </div>

            <div class="form-actions">
              <button type="submit" class="md-button md-button-primary">
                <span class="material-icons">search</span>
                Apply Filters
              </button>
              <button type="button" (click)="clearJourneyFilters()" class="md-button md-button-secondary">
                <span class="material-icons">clear</span>
                Clear
              </button>
            </div>
          </form>
        </div>

        <div class="results-section">
          <h3>Journey Results ({{ totalJourneyCount }} total)</h3>
          <div class="pagination-controls">
            <button 
              [disabled]="currentJourneyPage <= 1" 
              (click)="setJourneyPage(currentJourneyPage - 1)"
              class="md-button md-button-secondary">
              <span class="material-icons">chevron_left</span>
              Previous
            </button>
            <span class="page-info">Page {{ currentJourneyPage }} of {{ totalJourneyPages }}</span>
            <button 
              [disabled]="currentJourneyPage >= totalJourneyPages" 
              (click)="setJourneyPage(currentJourneyPage + 1)"
              class="md-button md-button-secondary">
              Next
              <span class="material-icons">chevron_right</span>
            </button>
          </div>

          <div class="journey-list">
            <div *ngFor="let journey of filteredJourneys" class="journey-item">
              <div class="journey-header">
                <span class="user-id">User: {{ journey.ownerUserId }}</span>
                <span class="transport-type">{{ journey.transportType }}</span>
                <span class="distance">{{ journey.distanceKm }} km</span>
              </div>
              <div class="journey-details">
                <div class="route">
                  <span class="start">{{ journey.startLocation }}</span>
                  <span class="material-icons">arrow_forward</span>
                  <span class="arrival">{{ journey.arrivalLocation }}</span>
                </div>
                <div class="timing">
                  <span class="start-time">{{ journey.startTime | date:'short' }}</span>
                  <span class="material-icons">schedule</span>
                  <span class="arrival-time">{{ journey.arrivalTime | date:'short' }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Monthly Statistics Tab -->
      <div *ngIf="activeTab === 'statistics'" class="tab-content">
        <div class="stats-controls">
          <h3>Monthly Distance Statistics</h3>
          <form class="stats-filter-form">
            <div class="filter-row">
              <div class="form-group">
                <label for="orderBy">Sort By</label>
                <div class="select-wrapper">
                  <select 
                    id="orderBy"
                    [(ngModel)]="statsOrderBy" 
                    name="orderBy"
                    (change)="loadMonthlyStats()"
                    class="md-select">
                    <option value="TotalDistanceKm">Total Distance</option>
                    <option value="UserId">User ID</option>
                  </select>
                  <span class="material-icons select-arrow">expand_more</span>
                </div>
              </div>
              <div class="form-group">
                <label for="pageSize">Page Size</label>
                <div class="select-wrapper">
                  <select 
                    id="pageSize"
                    [(ngModel)]="statsPageSize" 
                    name="pageSize"
                    (change)="loadMonthlyStats()"
                    class="md-select">
                    <option value="10">10 per page</option>
                    <option value="20">20 per page</option>
                    <option value="50">50 per page</option>
                  </select>
                  <span class="material-icons select-arrow">expand_more</span>
                </div>
              </div>
            </div>
          </form>
        </div>

        <div class="stats-results">
          <div class="pagination-controls">
            <button 
              [disabled]="currentStatsPage <= 1" 
              (click)="setStatsPage(currentStatsPage - 1)"
              class="md-button md-button-secondary">
              <span class="material-icons">chevron_left</span>
              Previous
            </button>
            <span class="page-info">Page {{ currentStatsPage }} of {{ totalStatsPages }}</span>
            <button 
              [disabled]="currentStatsPage >= totalStatsPages" 
              (click)="setStatsPage(currentStatsPage + 1)"
              class="md-button md-button-secondary">
              Next
              <span class="material-icons">chevron_right</span>
            </button>
          </div>

          <div class="stats-list">
            <div *ngFor="let stat of monthlyStats" class="stat-item">
              <div class="stat-header">
                <span class="user-id">{{ stat.userId }}</span>
                <span class="period">{{ stat.year }}-{{ stat.month.toString().padStart(2, '0') }}</span>
              </div>
              <div class="stat-value">
                <span class="material-icons">straighten</span>
                <span class="distance">{{ stat.totalDistanceKm }} km</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- User Management Tab -->
      <div *ngIf="activeTab === 'users'" class="tab-content">
        <div class="users-section">
          <h3>User Management</h3>
          <div class="user-list">
            <div *ngFor="let user of users" class="user-item">
              <div class="user-info">
                <div class="user-details">
                  <span class="user-name">{{ user.name }}</span>
                  <span class="user-email">{{ user.email }}</span>
                  <span class="user-created">Created: {{ user.createdAt | date:'short' }}</span>
                  <span *ngIf="user.lastLoginAt" class="user-last-login">Last login: {{ user.lastLoginAt | date:'short' }}</span>
                </div>
                <div class="user-status">
                  <span class="status-badge" [class]="'status-' + user.status.toLowerCase()">
                    {{ user.status }}
                  </span>
                  <select 
                    [value]="user.status" 
                    (change)="onUserStatusChange(user.id, $event)"
                    class="status-select">
                    <option value="Active">Active</option>
                    <option value="Suspended">Suspended</option>
                    <option value="Deactivated">Deactivated</option>
                  </select>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styleUrls: ['./admin-dashboard.component.scss']
})
export class AdminDashboardComponent implements OnInit {
  activeTab: 'journeys' | 'statistics' | 'users' = 'journeys';
  
  TransportType = TransportType;
  
  journeyFilterForm: FormGroup;
  filteredJourneys: any[] = [];
  totalJourneyCount = 0;
  currentJourneyPage = 1;
  journeyPageSize = 20;
  totalJourneyPages = 1;

  monthlyStats: MonthlyDistanceStat[] = [];
  currentStatsPage = 1;
  statsPageSize = 20;
  totalStatsPages = 1;
  statsOrderBy: 'UserId' | 'TotalDistanceKm' = 'TotalDistanceKm';

  users: AdminUser[] = [];

  constructor(
    private adminService: AdminService,
    private fb: FormBuilder,
    private authService: AuthService
  ) {
    this.journeyFilterForm = this.fb.group({
      userId: [''],
      transportType: [''],
      startDateFrom: [''],
      startDateTo: [''],
      arrivalDateFrom: [''],
      arrivalDateTo: [''],
      minDistance: [''],
      maxDistance: [''],
      orderBy: ['startTime'],
      direction: ['desc']
    });
  }

  ngOnInit(): void {
    this.loadMonthlyStats();
    this.loadUsers();
    this.loadFilteredJourneys({});
  }

  setActiveTab(tab: 'journeys' | 'statistics' | 'users'): void {
    this.activeTab = tab;
  }

  applyJourneyFilters(): void {
    const filter: AdminJourneyFilter = {
      ...this.journeyFilterForm.value,
      page: 1,
      pageSize: this.journeyPageSize
    };
    
    this.currentJourneyPage = 1;
    this.loadFilteredJourneys(filter);
  }

  clearJourneyFilters(): void {
    this.journeyFilterForm.reset({
      orderBy: 'startTime',
      direction: 'desc'
    });
    this.applyJourneyFilters();
  }

  loadFilteredJourneys(filter: AdminJourneyFilter): void {
    this.adminService.getAdminJourneys(filter).subscribe({
      next: (response) => {
        this.filteredJourneys = response.items;
        this.totalJourneyCount = response.totalCount;
        this.totalJourneyPages = Math.ceil(response.totalCount / response.pageSize);
      },
      error: (error) => {
        console.error('Failed to load filtered journeys:', error);
      }
    });
  }

  setJourneyPage(page: number): void {
    this.currentJourneyPage = page;
    const filter: AdminJourneyFilter = {
      ...this.journeyFilterForm.value,
      page: this.currentJourneyPage,
      pageSize: this.journeyPageSize
    };
    this.loadFilteredJourneys(filter);
  }

  loadMonthlyStats(): void {
    this.adminService.getMonthlyDistanceStats(this.currentStatsPage, this.statsPageSize, this.statsOrderBy).subscribe({
      next: (response) => {
        this.monthlyStats = response.items;
        this.totalStatsPages = Math.ceil(response.totalCount / response.pageSize);
      },
      error: (error) => {
        console.error('Failed to load monthly stats:', error);
      }
    });
  }

  setStatsPage(page: number): void {
    this.currentStatsPage = page;
    this.loadMonthlyStats();
  }

  loadUsers(): void {
    this.adminService.getUsers().subscribe({
      next: (response: AdminUserResponse) => {
        this.users = response.items;
      },
      error: (error) => {
        console.error('Failed to load users:', error);
      }
    });
  }

  updateUserStatus(userId: string, newStatus: string): void {
    this.adminService.updateUserStatus(userId, { status: newStatus as any }).subscribe({
      next: () => {
        this.loadUsers();
      },
      error: (error) => {
        console.error('Failed to update user status:', error);
      }
    });
  }

  onUserStatusChange(userId: string, event: Event): void {
    const target = event.target as HTMLSelectElement;
    if (target) {
      const newStatus = target.value;
      
      const userIndex = this.users.findIndex(u => u.id === userId);
      if (userIndex !== -1) {
        this.users[userIndex] = { ...this.users[userIndex], status: newStatus as any };
      }
      
      this.updateUserStatus(userId, newStatus);
    }
  }

} 