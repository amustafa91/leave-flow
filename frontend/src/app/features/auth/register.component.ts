import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-emerald-50 via-white to-cyan-50 p-4">
      <div class="w-full max-w-md animate-fadeIn">
        <!-- Logo -->
        <div class="text-center mb-8">
          <div class="inline-flex items-center justify-center w-16 h-16 rounded-2xl bg-gradient-to-br from-emerald-500 to-cyan-500 mb-4 shadow-lg">
            <svg class="w-8 h-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
          </div>
          <h1 class="text-3xl font-bold bg-gradient-to-r from-emerald-600 to-cyan-600 bg-clip-text text-transparent">LeaveFlow</h1>
          <p class="text-[var(--text-secondary)] mt-2">Employee Leave Management</p>
        </div>

        <!-- Registration Disabled Message -->
        <div class="card glass text-center">
          <div class="mb-6">
            <svg class="w-16 h-16 text-amber-500 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
            </svg>
            <h2 class="text-xl font-semibold text-[var(--text-primary)] mb-2">Registration Disabled</h2>
            <p class="text-[var(--text-secondary)]">
              Public registration is not available. Please contact your HR administrator to create an account for you.
            </p>
          </div>

          <a routerLink="/login" class="btn btn-primary w-full py-3 text-base">
            Go to Login
          </a>
        </div>
      </div>
    </div>
  `
})
export class RegisterComponent {}
