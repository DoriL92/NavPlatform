import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Journey, TransportType } from '../../models';

@Component({
  selector: 'app-journey-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './journey-card.component.html',
  styleUrls: ['./journey-card.component.scss']
})
export class JourneyCardComponent {
  @Input() journey!: Journey;
  @Input() showActions = true;
  @Output() edit = new EventEmitter<Journey>();
  @Output() delete = new EventEmitter<Journey>();
  @Output() favorite = new EventEmitter<Journey>();
  @Output() share = new EventEmitter<Journey>();

  TransportType = TransportType;

  constructor(private router: Router) {}

  onEdit(event: Event): void {
    event.stopPropagation();
    this.edit.emit(this.journey);
  }

  onDelete(event: Event): void {
    event.stopPropagation();
    this.delete.emit(this.journey);
  }

  onFavorite(event: Event): void {
    event.stopPropagation();
    this.favorite.emit(this.journey);
  }

  onShare(event: Event): void {
    event.stopPropagation();
    this.share.emit(this.journey);
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

  onViewDetails(): void {
    this.router.navigate(['/journey', this.journey.id]);
  }

  formatDateTime(dateTime: string): string {
    const date = new Date(dateTime);
    return date.toLocaleString('en-US', {
      year: 'numeric',
      month: 'short',
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
    return icons[transportType] || 'commute';
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
} 