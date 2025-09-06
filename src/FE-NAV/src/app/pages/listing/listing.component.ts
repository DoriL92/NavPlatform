import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { JourneyService } from '../../services/journey.service';
import { AuthService } from '../../services/auth.service';
import { DailyGoalService } from '../../services/daily-goal.service';
import { SignalRService, JourneyUpdatedEvent, JourneyDeletedEvent } from '../../services/signalr.service';
import { 
  JourneyFormComponent, 
  JourneyCardComponent, 
  DailyGoalProgressComponent 
} from '../../components';
import { Journey, JourneyCreateRequest, JourneyUpdateRequest } from '../../models';

@Component({
  selector: 'app-listing',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    JourneyFormComponent,
    JourneyCardComponent,
    DailyGoalProgressComponent
  ],
  templateUrl: './listing.component.html',
  styleUrls: ['./listing.component.scss']
})
export class ListingComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  
  journeys: Journey[] = [];
  totalCount = 0;
  currentPage = 1;
  pageSize = 20;
  showModal = false;
  editingJourney: Journey | undefined = undefined;
  error: string | null = null;
  loading = false;

  activeTab: 'all' | 'favorites' = 'all';
  
  get filteredJourneys(): Journey[] {
    if (this.activeTab === 'favorites') {
      return this.journeys.filter(journey => journey.isFavorite);
    }
    return this.journeys;
  }

  switchTab(tab: 'all' | 'favorites'): void {
    this.activeTab = tab;
    this.currentPage = 1;
  }

  getFavoriteCount(): number {
    return this.journeys.filter(journey => journey?.isFavorite)?.length?? 0;
  }

  constructor(
    public journeyService: JourneyService,
    private authService: AuthService,
    private dailyGoalService: DailyGoalService,
    private signalRService: SignalRService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.loadJourneys();
    
    this.journeyService.journeys$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(journeys => {
      this.journeys = journeys;
    });
    
    this.route.queryParams.subscribe(params => {
      if (params['edit'] && params['edit'] !== '') {
        const journeyId = params['edit'];
        this.openEditFormForJourney(journeyId);
      }
    });

    this.setupSignalRSubscriptions();
  }

  private openEditFormForJourney(journeyId: string): void {
    const journey = this.journeys.find(j => j.id === journeyId);
    if (journey) {
      this.onEditJourney(journey);
    } else {
      this.journeyService.getJourney(journeyId).subscribe({
        next: (journey) => {
          this.onEditJourney(journey);
        },
        error: (error) => {
          console.error('Failed to load journey for editing:', error);
        }
      });
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadJourneys(): void {
    this.loading = true;
    this.journeyService.getJourneys({
      page: this.currentPage,
      pageSize: this.pageSize
    }).subscribe({
      next: (response) => {
        this.journeys = response.items;
        this.totalCount = response.totalCount;
        this.loading = false;
        this.updateDailyGoalProgress();
      },
      error: (error) => {
        console.error('Failed to load journeys:', error);
        this.error = 'Failed to load journeys';
        this.loading = false;
      }
    });
  }
  private updateDailyGoalProgress(): void {
    if (this.journeys.length > 0) {
      this.dailyGoalService.calculateDailyProgress(this.journeys);
    }
  }

  private setupSignalRSubscriptions(): void {
    this.signalRService.journeyUpdated$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(event => {
      console.log('Real-time journey update received:', event);
      this.handleJourneyUpdate(event);
    });

    this.signalRService.journeyDeleted$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(event => {
      console.log('Real-time journey deletion received:', event);
      this.handleJourneyDeletion(event);
    });
  }

  private handleJourneyUpdate(event: JourneyUpdatedEvent): void {
    console.log('Enhanced journey update event:', event);
    
    if (event.journeyInfo && event.userInfo) {
      const journeyName = `${event.journeyInfo.startLocation} → ${event.journeyInfo.arrivalLocation}`;
      const updateMessage = `Journey "${journeyName}" was updated by ${event.userInfo.name}`;
      
      this.journeyService.showNotification(updateMessage);
      
      this.showJourneyUpdateNotification(event);
    } else {
      this.journeyService.showNotification(`Journey has been updated!`);
    }
    
    this.loadJourneys();
  }

  private handleJourneyDeletion(event: JourneyDeletedEvent): void {
    this.journeys = this.journeys.filter(journey => journey.id !== event.journeyId);
    
    this.journeyService.showNotification(`A favorited journey has been deleted.`);
    
    this.updateDailyGoalProgress();
  }

  openNewJourneyModal(): void {
    this.editingJourney = undefined;
    this.showModal = true;
    this.clearEditQueryParam();
    this.scrollToModal();
  }

  onEditJourney(journey: Journey): void {
    this.editingJourney = journey;
    this.showModal = true;
    this.clearEditQueryParam();
    this.scrollToModal();
  }

  private clearEditQueryParam(): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { edit: null },
      queryParamsHandling: 'merge'
    });
  }

  private scrollToModal(): void {
    setTimeout(() => {
      const modal = document.querySelector('.modal');
      if (modal) {
        modal.scrollIntoView({ behavior: 'smooth', block: 'start' });
      }
    }, 100);
  }

  closeModal(event?: Event): void {
    if (event) {
      event.stopPropagation();
    }
    this.showModal = false;
    this.editingJourney = undefined;
  }

  onJourneySaved(journeyData: JourneyCreateRequest | JourneyUpdateRequest): void {
    if (this.editingJourney) {
      this.journeyService.updateJourney(this.editingJourney.id, journeyData as JourneyUpdateRequest).subscribe({
        next: () => {
          this.loadJourneys();
          this.updateDailyGoalProgress();
          this.closeModal();
        },
        error: (error) => {
          console.error('Error updating journey:', error);
          alert('Failed to update journey');
        }
      });
    } else {
      this.journeyService.createJourney(journeyData as JourneyCreateRequest).subscribe({
        next: () => {
          this.loadJourneys();
          this.updateDailyGoalProgress();
          this.closeModal();
        },
        error: (error) => {
          console.error('Error creating journey:', error);
          alert('Failed to create journey');
        }
      });
    }
  }

  onDeleteJourney(journey: Journey): void {
    if (confirm(`Are you sure you want to delete the journey from ${journey.startLocation} to ${journey.arrivalLocation}?`)) {
      this.journeyService.deleteJourney(journey.id).subscribe({
        next: () => {
          this.loadJourneys();
          this.updateDailyGoalProgress();
        },
        error: (error) => {
          console.error('Error deleting journey:', error);
          alert('Failed to delete journey');
        }
      });
    }
  }

  onShareJourney(journey: Journey): void {
    this.openShareModal(journey);
  }

  onFavoriteJourney(journey: Journey): void {
    this.journeyService.toggleFavorite(journey.id).subscribe({
      next: (success) => {
        if (success) {
          const action = journey.isFavorite ? 'removed from' : 'added to';
          console.log(`Journey ${action} favorites successfully`);
        }
      },
      error: (error) => {
        console.error('Error toggling favorite:', error);
      }
    });
  }

  private openShareModal(journey: Journey): void {
    const shareType = confirm(
      `How would you like to share this journey?\n\n` +
      `From: ${journey.startLocation}\n` +
      `To: ${journey.arrivalLocation}\n\n` +
      `Click OK for direct sharing with friends, Cancel for public link generation.`
    );

    if (shareType !== null) {
      if (shareType) {
        this.shareWithFriends(journey);
      } else {
        this.generatePublicLink(journey);
      }
    }
  }

 private shareWithFriends(journey: Journey): void {
  const emailsInput = prompt(
    `Enter emails to share with (comma-separated):\n` +
    `Example: alice@email.com,bob@email.com\n\n` +
    `Journey: ${journey.startLocation} → ${journey.arrivalLocation}`
  );

  if (emailsInput && emailsInput.trim()) {
    const emails = emailsInput.split(',').map(e => e.trim()).filter(e => e.length > 0);

    if (emails.length > 0) {
      const shareRequest = {
        emails: emails,
        shareMessage: `Check out my journey from ${journey.startLocation} to ${journey.arrivalLocation}!`
      };

      this.journeyService.shareJourney(journey.id, shareRequest).subscribe({
        next: (response) => {
          if (response && response.success) {
            alert(`Journey shared successfully with ${emails.length} user(s)!`);
          } else {
            alert('Failed to share journey');
          }
        },
        error: (error) => {
          console.error('Error sharing journey:', error);
          alert('Failed to share journey');
        }
      });
    } else {
      alert('Please enter valid emails');
    }
  }
}

  private generatePublicLink(journey: Journey): void {
    this.journeyService.generatePublicLink(journey.id).subscribe({
      next: (response) => {
        console.log('Public link response:', response);
        console.log('Response type:', typeof response);
        console.log('Response keys:', Object.keys(response));
        console.log('Response.url:', response.url);
        
        // Handle potential response wrapping
        let actualResponse = response;
        if (response && typeof response === 'object' && 'data' in response) {
          actualResponse = (response as any).data;
          console.log('Response was wrapped, unwrapped data:', actualResponse);
        }
        
        if (actualResponse && actualResponse.url) {
          navigator.clipboard.writeText(actualResponse.url).then(() => {
            this.showPublicLinkModal(actualResponse.url);
          }).catch(() => {
            // Fallback if clipboard fails
            this.showPublicLinkModal(actualResponse.url, false);
          });
        } else {
          console.error('Invalid response structure:', actualResponse);
          alert('Failed to generate public link - invalid response');
        }
      },
      error: (error) => {
        console.error('Error generating public link:', error);
        alert('Failed to generate public link');
      }
    });
  }

  setPage(page: number): void {
    this.currentPage = page;
    this.loadJourneys();
  }

  private showPublicLinkModal(url: string, copiedToClipboard: boolean = true): void {
    const modal = document.createElement('div');
    modal.style.cssText = `
      position: fixed;
      top: 0;
      left: 0;
      width: 100%;
      height: 100%;
      background: rgba(0, 0, 0, 0.5);
      display: flex;
      justify-content: center;
      align-items: center;
      z-index: 10000;
    `;

    const modalContent = document.createElement('div');
    modalContent.style.cssText = `
      background: white;
      padding: 20px;
      border-radius: 8px;
      max-width: 400px;
      width: 90%;
      text-align: center;
    `;

    const title = document.createElement('h3');
    title.textContent = 'Public Link';
    title.style.cssText = `
      margin: 0 0 15px 0;
      color: #333;
      font-size: 18px;
    `;

    const message = document.createElement('p');
    message.textContent = copiedToClipboard ? 'Link copied!' : 'Your public link:';
    message.style.cssText = `
      margin: 0 0 15px 0;
      color: #666;
      font-size: 14px;
    `;

    const linkContainer = document.createElement('div');
    linkContainer.style.cssText = `
      background: #f5f5f5;
      border-radius: 4px;
      padding: 10px;
      margin: 15px 0;
      word-break: break-all;
      font-size: 12px;
      color: #007bff;
    `;
    linkContainer.textContent = url;

    const buttonContainer = document.createElement('div');
    buttonContainer.style.cssText = `
      display: flex;
      gap: 10px;
      justify-content: center;
    `;

    const openButton = document.createElement('button');
    openButton.textContent = 'Open';
    openButton.style.cssText = `
      background: #007bff;
      color: white;
      border: none;
      padding: 8px 16px;
      border-radius: 4px;
      font-size: 12px;
      cursor: pointer;
    `;

    const closeButton = document.createElement('button');
    closeButton.textContent = 'Close';
    closeButton.style.cssText = `
      background: #6c757d;
      color: white;
      border: none;
      padding: 8px 16px;
      border-radius: 4px;
      font-size: 12px;
      cursor: pointer;
    `;

    openButton.addEventListener('click', () => {
      window.open(url, '_blank');
    });

    closeButton.addEventListener('click', () => {
      modal.remove();
    });

    modal.addEventListener('click', (e) => {
      if (e.target === modal) {
        modal.remove();
      }
    });

    buttonContainer.appendChild(openButton);
    buttonContainer.appendChild(closeButton);

    modalContent.appendChild(title);
    modalContent.appendChild(message);
    modalContent.appendChild(linkContainer);
    modalContent.appendChild(buttonContainer);

    modal.appendChild(modalContent);
    document.body.appendChild(modal);
  }

  get totalPages(): number {
    return Math.ceil(this.filteredJourneys.length / this.pageSize);
  }

  private showJourneyUpdateNotification(event: JourneyUpdatedEvent): void {
    if (!event.journeyInfo || !event.userInfo) return;

    const toast = document.createElement('div');
    toast.style.cssText = `
      position: fixed;
      top: 20px;
      right: 20px;
      background: #007bff;
      color: white;
      padding: 15px;
      border-radius: 6px;
      max-width: 300px;
      z-index: 10000;
      font-size: 14px;
    `;

    const message = document.createElement('div');
    message.textContent = `Journey updated by ${event.userInfo.name}`;
    message.style.cssText = `
      margin-bottom: 5px;
      font-weight: 600;
    `;

    const details = document.createElement('div');
    details.textContent = `${event.journeyInfo.startLocation} → ${event.journeyInfo.arrivalLocation}`;
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

  getPageNumbers(): number[] {
    const totalPages = this.totalPages;
    const currentPage = this.currentPage;
    const delta = 2;
    
    const pages: number[] = [];
    const startPage = Math.max(1, currentPage - delta);
    const endPage = Math.min(totalPages, currentPage + delta);
    
    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }
    
    return pages;
  }

  onPageSizeChange(newPageSize: number): void {
    this.pageSize = newPageSize;
    this.currentPage = 1;
    this.loadJourneys();
  }

  Math = Math;
}
