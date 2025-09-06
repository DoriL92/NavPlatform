import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  
  isLoggingIn = false;
  showSuspensionMessage = false;
  suspensionMessage = '';

  constructor(
    public authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    // Check if user was redirected due to suspension
    this.route.queryParams.pipe(takeUntil(this.destroy$)).subscribe(params => {
      if (params['suspended'] === 'true') {
        this.showSuspensionMessage = true;
        this.suspensionMessage = 'Your account has been suspended. Please contact support.';
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onLogin(): void {
    this.isLoggingIn = true;
    
    // Use OAuth login
    this.authService.login();
    this.isLoggingIn = false;
  }

  onLogout(): void {
    this.authService.logout();
  }




}
