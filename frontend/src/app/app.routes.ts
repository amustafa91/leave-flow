import { Routes } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from './core/services/auth.service';
import { Router } from '@angular/router';

// Auth guard
const authGuard = () => {
  const authService = inject(AuthService);
  const router = inject(Router);
  if (authService.isAuthenticated()) {
    return true;
  }
  router.navigate(['/login']);
  return false;
};

// Admin guard
const adminGuard = () => {
  const authService = inject(AuthService);
  const router = inject(Router);
  if (authService.isAdmin()) {
    return true;
  }
  router.navigate(['/dashboard']);
  return false;
};

export const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { 
    path: 'login', 
    loadComponent: () => import('./features/auth/login.component').then(m => m.LoginComponent) 
  },
  { 
    path: 'register', 
    loadComponent: () => import('./features/auth/register.component').then(m => m.RegisterComponent) 
  },
  { 
    path: 'dashboard', 
    loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent),
    canActivate: [authGuard]
  },
  { 
    path: 'leave-request', 
    loadComponent: () => import('./features/leave-request/leave-request.component').then(m => m.LeaveRequestComponent),
    canActivate: [authGuard]
  },
  { 
    path: 'my-leaves', 
    loadComponent: () => import('./features/my-leaves/my-leaves.component').then(m => m.MyLeavesComponent),
    canActivate: [authGuard]
  },
  { 
    path: 'approvals', 
    loadComponent: () => import('./features/approvals/approvals.component').then(m => m.ApprovalsComponent),
    canActivate: [authGuard]
  },
  { 
    path: 'employees', 
    loadComponent: () => import('./features/employees/employees.component').then(m => m.EmployeesComponent),
    canActivate: [authGuard, adminGuard]
  },
  {
    path: 'notifications',
    loadComponent: () => import('./features/notifications/notifications.component').then(m => m.NotificationsComponent),
    canActivate: [authGuard]
  },
  {
    path: 'holidays',
    loadComponent: () => import('./features/holidays/holidays.component').then(m => m.HolidaysComponent),
    canActivate: [authGuard]
  },
  {
    path: 'settings',
    loadComponent: () => import('./features/settings/settings.component').then(m => m.SettingsComponent),
    canActivate: [authGuard, adminGuard]
  },
  { 
    path: 'profile', 
    loadComponent: () => import('./features/profile/profile.component').then(m => m.ProfileComponent),
    canActivate: [authGuard]
  },
  { path: '**', redirectTo: '/dashboard' }
];
