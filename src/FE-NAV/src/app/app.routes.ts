// src/app/app.routes.ts
import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { ListingComponent } from './pages/listing/listing.component';
import { JourneyDetailsComponent } from './pages/journey-details/journey-details.component';
import { AdminDashboardComponent } from './pages/admin/admin-dashboard.component';
import { PublicJourneyComponent } from './pages/public-journey/public-journey.component';

import { authGuard, loginGuard } from './guards/auth.guard';
import { adminGuard } from './guards/admin.guard';
import { userOrAdminGuard } from './guards/user-or-admin.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { 
    path: 'login', 
    component: LoginComponent,
    canActivate: [loginGuard]
  },
  { 
    path: 'listing', 
    component: ListingComponent,
    canActivate: [userOrAdminGuard]
  },
  { 
    path: 'journey/:id', 
    component: JourneyDetailsComponent,
    canActivate: [userOrAdminGuard]
  },
  { 
    path: 'admin', 
    component: AdminDashboardComponent,
    canActivate: [adminGuard]
  },
  { 
    path: 'api/journeys/:token', 
    component: PublicJourneyComponent,
    pathMatch: 'full'
  },
  { path: '**', redirectTo: 'listing' },
];