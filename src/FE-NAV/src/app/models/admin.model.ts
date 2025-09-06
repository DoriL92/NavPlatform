export interface AdminJourneyFilter {
  userId?: string;
  transportType?: string;
  startDateFrom?: string;
  startDateTo?: string;
  arrivalDateFrom?: string;
  arrivalDateTo?: string;
  minDistance?: number;
  maxDistance?: number;
  page?: number;
  pageSize?: number;
  orderBy?: string;
  direction?: 'asc' | 'desc';
}

export interface AdminJourneyResponse {
  items: any[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface MonthlyDistanceStat {
  userId: string;
  year: number;
  month: number;
  totalDistanceKm: number;
}

export interface MonthlyDistanceResponse {
  items: MonthlyDistanceStat[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface UserStatusUpdate {
  status: 'Active' | 'Suspended' | 'Deactivated';
}

export interface AdminUser {
  id: string;
  email: string;
  name: string;
  status: 'Active' | 'Suspended' | 'Deactivated';
  createdAt: string;
  lastLoginAt?: string;
  lastSeenAt?: string;
  pictureUrl?: string;
}

export interface AdminUserResponse {
  items: AdminUser[];
  totalCount: number;
  page: number;
  pageSize: number;
} 