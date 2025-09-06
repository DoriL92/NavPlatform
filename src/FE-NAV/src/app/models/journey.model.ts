export enum TransportType {
  Car = 'Car',
  Bus = 'Bus',
  Train = 'Train',
  Ferry = 'Ferry',
  Plane = 'Plane',
  Bike = 'Bike',
  Walk = 'Walk',
}

export interface Journey {
  id: string;
  startLocation: string;
  startTime: string;
  arrivalLocation: string;
  arrivalTime: string;
  transportType: TransportType;
  distanceKm: number;
  userId: string;
  createdAt: string;
  updatedAt: string;

  // Sharing properties
  isPublic?: boolean;
  shareId?: string;
  shareCount?: number;
  lastSharedAt?: string;
  // Favorite properties
  isFavorite?: boolean;
  // Daily goal achievement
  isDailyGoalAchieved?: boolean;
}

export interface JourneyCreateRequest {
  startLocation: string;
  startTime: string;
  arrivalLocation: string;
  arrivalTime: string;
  transportType: TransportType;
  distanceKm: number;
  isFavorite: boolean;
}

export interface JourneyUpdateRequest {
  startLocation?: string;
  startTime?: string;
  arrivalLocation?: string;
  arrivalTime?: string;
  transportType?: TransportType;
  distanceKm?: number;
}

export interface PagedJourneyResponse {
  items: Journey[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface JourneyQueryParams {
  page?: number;
  pageSize?: number;
  userId?: string;
  transportType?: TransportType;
  startDate?: string;
  endDate?: string;
  isPublic?: boolean;
}

// Sharing interfaces
export interface JourneyShareRequest {
  emails: string[];
  shareMessage?: string;
}

export interface JourneyShareResponse {
  success: boolean;
  message: string;
  shareCount: number;
}

export interface JourneyPublicLinkResponse {
  url: string;
}

export interface JourneyAuditLog {
  id: string;
  journeyId: string;
  userId: string;
  action: 'shared' | 'viewed' | 'favorited' | 'unfavorited';
  timestamp: string;
  metadata?: Record<string, any>;
} 