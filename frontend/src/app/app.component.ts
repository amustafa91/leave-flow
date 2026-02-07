import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './core/services/auth.service';
import { UserRole } from './core/models/models';
import { NotificationService } from './core/services/notification.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="min-h-screen bg-[var(--background)]">
      @if (authService.isAuthenticated()) {
        <!-- Sidebar -->
        <aside class="fixed left-0 top-0 h-screen w-64 bg-[var(--surface)] border-r border-[var(--border)] p-4 flex flex-col">
          <!-- Logo -->
          <!-- Logo -->
          <a routerLink="/dashboard" class="flex items-center gap-3 mb-8 px-2 cursor-pointer select-none">
            <div class="w-10 h-10 rounded-xl bg-gradient-to-br from-emerald-500 to-cyan-500 flex items-center justify-center">
              <svg class="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
            </div>
            <span class="text-xl font-bold bg-gradient-to-r from-emerald-600 to-cyan-600 bg-clip-text text-transparent">LeaveFlow</span>
          </a>

           <!-- Navigation -->
           <nav class="flex-1 space-y-1">
             <!-- ... existing links ... -->
             <a routerLink="/dashboard" routerLinkActive="bg-emerald-50 text-emerald-700 border-emerald-200" 
                class="flex items-center gap-3 px-3 py-2.5 rounded-lg text-[var(--text-secondary)] hover:bg-[var(--background)] transition-colors border border-transparent">
               <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                 <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
               </svg>
               Dashboard
             </a>

             <a routerLink="/notifications" routerLinkActive="bg-emerald-50 text-emerald-700 border-emerald-200"
                class="flex items-center gap-3 px-3 py-2.5 rounded-lg text-[var(--text-secondary)] hover:bg-[var(--background)] transition-colors border border-transparent">
               <div class="relative">
                 <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                   <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
                 </svg>
                 @if (notificationService.unreadCount() > 0) {
                   <span class="absolute -top-1 -right-1 flex h-3 w-3">
                     <span class="animate-ping absolute inline-flex h-full w-full rounded-full bg-red-400 opacity-75"></span>
                     <span class="relative inline-flex rounded-full h-3 w-3 bg-red-500 text-[8px] text-white justify-center items-center">
                       {{ notificationService.unreadCount() }}
                     </span>
                   </span>
                 }
               </div>
               Notifications
             </a>
            
            <!-- Holidays -->
            <a routerLink="/holidays" routerLinkActive="bg-emerald-50 text-emerald-700 border-emerald-200"
               class="flex items-center gap-3 px-3 py-2.5 rounded-lg text-[var(--text-secondary)] hover:bg-[var(--background)] transition-colors border border-transparent">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
              Public Holidays
            </a>
            
            @if (getRoleName() === 'SuperAdmin' || getRoleName() === 'HRAdmin') {
                <a routerLink="/settings" routerLinkActive="bg-emerald-50 text-emerald-700 border-emerald-200"
                   class="flex items-center gap-3 px-3 py-2.5 rounded-lg text-[var(--text-secondary)] hover:bg-[var(--background)] transition-colors border border-transparent">
                  <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                  </svg>
                  Settings
                </a>
            }

            <div class="h-px bg-gray-200 my-2"></div>

            <a routerLink="/leave-request" routerLinkActive="bg-emerald-50 text-emerald-700 border-emerald-200"
               class="flex items-center gap-3 px-3 py-2.5 rounded-lg text-[var(--text-secondary)] hover:bg-[var(--background)] transition-colors border border-transparent">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
              </svg>
              Request Leave
            </a>

            <a routerLink="/my-leaves" routerLinkActive="bg-emerald-50 text-emerald-700 border-emerald-200"
               class="flex items-center gap-3 px-3 py-2.5 rounded-lg text-[var(--text-secondary)] hover:bg-[var(--background)] transition-colors border border-transparent">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
              </svg>
              My Leaves
            </a>

            @if (authService.isApprover()) {
              <a routerLink="/approvals" routerLinkActive="bg-emerald-50 text-emerald-700 border-emerald-200"
                 class="flex items-center gap-3 px-3 py-2.5 rounded-lg text-[var(--text-secondary)] hover:bg-[var(--background)] transition-colors border border-transparent">
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                Approvals
              </a>
            }

            @if (authService.isAdmin()) {
              <a routerLink="/employees" routerLinkActive="bg-emerald-50 text-emerald-700 border-emerald-200"
                 class="flex items-center gap-3 px-3 py-2.5 rounded-lg text-[var(--text-secondary)] hover:bg-[var(--background)] transition-colors border border-transparent">
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
                </svg>
                Employees
              </a>
            }
          </nav>

          <!-- User section -->
          <div class="border-t border-[var(--border)] pt-4 mt-4">
            <a routerLink="/profile" class="flex items-center gap-3 px-3 py-2 rounded-lg hover:bg-[var(--background)] transition-colors">
              <div class="w-9 h-9 rounded-full bg-gradient-to-br from-emerald-400 to-cyan-400 flex items-center justify-center text-white font-medium">
                {{ (authService.user()?.firstName || '')[0] || '' }}{{ (authService.user()?.lastName || '')[0] || '' }}
              </div>
              <div class="flex-1 min-w-0">
                <p class="text-sm font-medium text-[var(--text-primary)] truncate">{{ authService.user()?.fullName }}</p>
                <p class="text-xs text-[var(--text-secondary)]">{{ getRoleName() }}</p>
              </div>
            </a>
            <button (click)="authService.logout()" class="w-full mt-2 flex items-center gap-3 px-3 py-2 rounded-lg text-red-600 hover:bg-red-50 transition-colors">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
              </svg>
              Sign Out
            </button>
          </div>
        </aside>

        <!-- Main content -->
        <main class="ml-64 p-8">
          <router-outlet />
        </main>
      } @else {
        <!-- No sidebar for login/register -->
        <router-outlet />
      }
    </div>
  `,
  styles: [`
    :host { display: block; }
  `]
})
export class AppComponent {
  constructor(public authService: AuthService, public notificationService: NotificationService) {}

  getRoleName(): string {
    const role = this.authService.user()?.role;
    switch (role) {
      case UserRole.SuperAdmin: return 'Super Admin';
      case UserRole.HRAdmin: return 'HR Admin';
      case UserRole.Approver: return 'Manager';
      default: return 'Employee';
    }
  }
}
