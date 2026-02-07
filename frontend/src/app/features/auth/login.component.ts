import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
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
          <p class="text-[var(--text-secondary)] mt-2">Sign in to your account</p>
        </div>

        <!-- Login Form -->
        <div class="card glass">
          <form [formGroup]="form" (ngSubmit)="onSubmit()" class="space-y-5">
            <div>
              <label class="block text-sm font-medium text-[var(--text-primary)] mb-1.5">Email</label>
              <input type="email" formControlName="email" class="input" placeholder="you@company.com" />
              @if (form.get('email')?.touched && form.get('email')?.invalid) {
                <p class="text-red-500 text-xs mt-1">Valid email is required</p>
              }
            </div>

            <div>
              <label class="block text-sm font-medium text-[var(--text-primary)] mb-1.5">Password</label>
              <input type="password" formControlName="password" class="input" placeholder="••••••••" />
              @if (form.get('password')?.touched && form.get('password')?.invalid) {
                <p class="text-red-500 text-xs mt-1">Password is required</p>
              }
            </div>

            @if (error) {
              <div class="p-3 rounded-lg bg-red-50 border border-red-200 text-red-700 text-sm">
                {{ error }}
              </div>
            }

            <button type="submit" [disabled]="loading || form.invalid" 
                    class="btn btn-primary w-full py-3 text-base disabled:opacity-50 disabled:cursor-not-allowed">
              @if (loading) {
                <svg class="animate-spin h-5 w-5" viewBox="0 0 24 24">
                  <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" fill="none"></circle>
                  <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"></path>
                </svg>
                Signing in...
              } @else {
                Sign In
              }
            </button>
          </form>

          <div class="mt-6 text-center">
            <p class="text-[var(--text-secondary)] text-sm">
              Don't have an account? 
              <a routerLink="/register" class="text-emerald-600 font-medium hover:underline">Sign up</a>
            </p>
          </div>
        </div>
      </div>
    </div>
  `
})
export class LoginComponent {
  form: FormGroup;
  loading = false;
  error = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  onSubmit(): void {
    if (this.form.invalid) return;

    this.loading = true;
    this.error = '';

    this.authService.login(this.form.value).subscribe({
      next: () => {
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.error = err.error?.message || 'Invalid email or password';
        this.loading = false;
      }
    });
  }
}
