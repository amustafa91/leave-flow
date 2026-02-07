import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../core/services/auth.service';
import { LeaveService } from '../../core/services/leave.service';
import { LeaveBalance, LeaveRequest, LeaveStatus } from '../../core/models/models';
import { environment } from '../../../environments/environment';

interface Holiday {
  id: string;
  date: string;
  name: string;
  isCustom: boolean;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="animate-fadeIn">
      <!-- Header -->
      <div class="mb-8">
        <h1 class="text-2xl font-bold text-[var(--text-primary)]">
          Welcome back, {{ authService.user()?.firstName }}! 👋
        </h1>
        <p class="text-[var(--text-secondary)] mt-1">Here's your leave overview</p>
      </div>

      <!-- Quick Actions -->
      <div class="flex gap-3 mb-8">
        <a routerLink="/leave-request" class="btn btn-primary">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
          </svg>
          Request Leave
        </a>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-8">
        <!-- Leave Balances (2 cols) -->
        <div class="lg:col-span-2">
          <h2 class="text-lg font-semibold text-[var(--text-primary)] mb-4">Leave Balances</h2>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            @if (balancesLoading) {
              <div class="col-span-2 text-center py-8 text-[var(--text-secondary)] animate-pulse">
                Loading balances...
              </div>
            } @else {
              @for (balance of balances(); track balance.id) {
                <div class="card hover:shadow-md transition-shadow">
                  <div class="flex items-center justify-between mb-3">
                    <div class="flex items-center gap-2">
                      <div class="w-3 h-3 rounded-full" [style.background-color]="balance.leaveTypeColor"></div>
                      <span class="font-medium text-[var(--text-primary)]">{{ balance.leaveTypeName }}</span>
                    </div>
                    <span class="text-2xl font-bold" [style.color]="balance.leaveTypeColor">
                      {{ balance.remainingDays | number:'1.0-2' }}
                    </span>
                  </div>
                  <div class="text-sm text-[var(--text-secondary)] mb-2">
                    of {{ balance.totalDays | number:'1.0-2' }} days remaining
                  </div>
                  <!-- Progress bar -->
                  <div class="h-2 bg-gray-100 rounded-full overflow-hidden">
                    <div class="h-full rounded-full transition-all duration-500" 
                         [style.width.%]="(balance.remainingDays / balance.totalDays) * 100"
                         [style.background-color]="balance.leaveTypeColor">
                    </div>
                  </div>
                  <div class="flex justify-between text-xs text-[var(--text-secondary)] mt-2">
                    <span>Used: {{ balance.usedDays }}</span>
                    <span class="flex flex-col items-end">
                      <span>Pending: {{ balance.pendingHours }}h</span>
                      @if (balance.accruedDays) {
                        <span class="text-emerald-600 font-medium" title="Earned pro-rata based on days worked this year">Accrued: {{ balance.accruedDays }}</span>
                      }
                    </span>
                  </div>
                </div>
              } @empty {
                <div class="col-span-2 text-center py-8 text-[var(--text-secondary)]">
                  No leave balances found. Contact HR to assign leave types.
                </div>
              }
            }
          </div>
        </div>

        <!-- Upcoming Holidays (1 col) -->
        <div>
          <h2 class="text-lg font-semibold text-[var(--text-primary)] mb-4">Upcoming Holidays 🎉</h2>
          <div class="card">
            @if (holidaysLoading) {
              <div class="text-center py-4 text-[var(--text-secondary)] animate-pulse">Loading...</div>
            } @else if (upcomingHolidays().length > 0) {
              <div class="space-y-3">
                @for (h of upcomingHolidays(); track h.id) {
                  <div class="flex items-center gap-3 p-3 bg-gradient-to-r from-amber-50 to-orange-50 rounded-lg border border-amber-100">
                    <div class="flex flex-col items-center justify-center w-12 h-12 bg-white rounded-lg shadow-sm">
                      <span class="text-xs font-medium text-amber-600">{{ h.date | date:'MMM' }}</span>
                      <span class="text-lg font-bold text-amber-700">{{ h.date | date:'d' }}</span>
                    </div>
                    <div class="flex-1">
                      <div class="font-medium text-[var(--text-primary)]">{{ h.name }}</div>
                      <div class="text-xs text-[var(--text-secondary)]">{{ h.date | date:'EEEE' }}</div>
                    </div>
                    @if (h.isCustom) {
                      <span class="text-xs px-2 py-0.5 bg-amber-100 text-amber-700 rounded">Custom</span>
                    }
                  </div>
                }
              </div>
            } @else {
              <div class="text-center py-6 text-[var(--text-secondary)]">
                <svg class="w-12 h-12 mx-auto mb-2 text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
                No upcoming holidays
              </div>
            }
          </div>
        </div>
      </div>

      <!-- Recent Requests -->
      <div>
        <h2 class="text-lg font-semibold text-[var(--text-primary)] mb-4">Recent Requests</h2>
        <div class="card">
          @if (requests().length > 0) {
              <div class="overflow-x-auto">
                <table class="w-full text-left text-sm">
                  <thead>
                    <tr class="text-[var(--text-secondary)] border-b border-[var(--border)]">
                      <th class="pb-3 font-medium">Employee</th>
                      <th class="pb-3 font-medium">Applied On</th>
                      <th class="pb-3 font-medium">Type</th>
                      <th class="pb-3 font-medium">Dates</th>
                      <th class="pb-3 font-medium">Days</th>
                      <th class="pb-3 font-medium">Status</th>
                      <th class="pb-3 font-medium">Approver</th>
                      <th class="pb-3 font-medium">Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    @for (request of requests().slice(0, 5); track request.id) {
                      <tr class="border-b border-[var(--border)] last:border-0 hover:bg-[var(--surface-hover)]">
                        <td class="py-3 font-medium text-[var(--text-primary)]">
                          {{ request.employeeName }}
                        </td>
                        <td class="py-3 text-[var(--text-secondary)]">
                          {{ request.createdAt | date }}
                        </td>
                        <td class="py-3">
                          <div class="flex items-center gap-2">
                            <div class="w-2 h-2 rounded-full" [style.background-color]="request.leaveTypeColor"></div>
                            <span class="font-medium text-[var(--text-primary)]">{{ request.leaveTypeName }}</span>
                          </div>
                        </td>
                        <td class="py-3 text-[var(--text-secondary)]">
                          {{ request.startDate | date }} - {{ request.endDate | date }}
                        </td>
                        <td class="py-3 text-[var(--text-secondary)]">
                          {{ request.isFullDay ? request.totalDays + ' days' : request.leaveHours + ' hours' }}
                        </td>
                        <td class="py-3">
                          <span [class]="getStatusClass(request.status)">
                            {{ getStatusLabel(request.status) }}
                          </span>
                        </td>
                        <td class="py-3 text-[var(--text-secondary)]">
                          {{ request.approverName || '-' }}
                        </td>
                        <td class="py-3">
                          @if (canCancel(request)) {
                            <button class="text-red-600 hover:text-red-800 text-sm font-medium transition-colors" 
                                    (click)="cancelRequest(request.id)">
                              Cancel
                            </button>
                          }
                        </td>
                      </tr>
                    }
                  </tbody>
                </table>
              </div>
          } @else {
            <div class="text-center py-8 text-[var(--text-secondary)]">
              No leave requests yet. 
              <a routerLink="/leave-request" class="text-emerald-600 hover:underline">Request one!</a>
            </div>
          }
        </div>
      </div>
    </div>
  `
})
export class DashboardComponent implements OnInit {
  balances = signal<LeaveBalance[]>([]);
  requests = signal<LeaveRequest[]>([]);
  upcomingHolidays = signal<Holiday[]>([]);

  balancesLoading = true;
  holidaysLoading = true;

  constructor(
    public authService: AuthService,
    private leaveService: LeaveService,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    this.loadData();
    this.loadHolidays();
  }

  loadData(): void {
    this.balancesLoading = true;
    this.leaveService.getMyBalances().subscribe({
      next: (data) => {
        this.balances.set(data);
        this.balancesLoading = false;
      },
      error: (err) => {
        console.error('Failed to load balances', err);
        this.balancesLoading = false;
      }
    });

    this.leaveService.getMyRequests().subscribe({
      next: (data) => this.requests.set(data),
      error: (err) => console.error('Failed to load requests', err)
    });
  }

  loadHolidays(): void {
    this.holidaysLoading = true;
    const countryCode = this.authService.user()?.countryCode || 'AE';
    const year = new Date().getFullYear();
    
    this.http.get<Holiday[]>(`${environment.apiUrl}/holidays/${countryCode}/${year}`)
      .subscribe({
        next: (holidays) => {
          // Filter to only upcoming holidays (today or future)
          const today = new Date();
          today.setHours(0, 0, 0, 0);
          
          const upcoming = holidays
            .filter(h => new Date(h.date) >= today)
            .slice(0, 5); // Show max 5
          
          this.upcomingHolidays.set(upcoming);
          this.holidaysLoading = false;
        },
        error: () => {
          this.holidaysLoading = false;
        }
      });
  }

  getStatusClass(status: LeaveStatus): string {
    const base = 'badge ';
    switch (status) {
      case LeaveStatus.Pending: return base + 'badge-pending';
      case LeaveStatus.Approved: return base + 'badge-approved';
      case LeaveStatus.Rejected: return base + 'badge-rejected';
      case LeaveStatus.Cancelled: return base + 'badge-cancelled';
      default: return base;
    }
  }

  getStatusLabel(status: LeaveStatus): string {
    switch (status) {
      case LeaveStatus.Pending: return 'Pending';
      case LeaveStatus.Approved: return 'Approved';
      case LeaveStatus.Rejected: return 'Rejected';
      case LeaveStatus.Cancelled: return 'Cancelled';
      default: return 'Unknown';
    }
  }

  canCancel(request: LeaveRequest): boolean {
    if (request.status === LeaveStatus.Pending) return true;
    if (request.status === LeaveStatus.Approved) {
      const start = new Date(request.startDate);
      const now = new Date();
      return start > now; 
    }
    return false;
  }

  cancelRequest(id: string): void {
    if (confirm('Are you sure you want to cancel this leave request?')) {
      this.leaveService.cancelRequest(id).subscribe({
        next: () => {
          this.loadData();
        },
        error: (err) => console.error('Failed to cancel request', err)
      });
    }
  }
}
