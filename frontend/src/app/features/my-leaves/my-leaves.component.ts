import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { LeaveService } from '../../core/services/leave.service';
import { LeaveRequest, LeaveStatus } from '../../core/models/models';

@Component({
  selector: 'app-my-leaves',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="animate-fadeIn">
      <div class="flex justify-between items-center mb-6">
        <h1 class="text-2xl font-bold text-[var(--text-primary)]">My Leave Requests</h1>
        <a routerLink="/leave-request" class="btn btn-primary">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
          </svg>
          New Request
        </a>
      </div>

      <div class="card">
        @if (requests().length > 0) {
          <div class="overflow-x-auto">
            <table class="w-full">
              <thead>
                <tr class="text-left text-[var(--text-secondary)] text-sm border-b border-[var(--border)]">
                  <th class="pb-3 font-medium">Type</th>
                  <th class="pb-3 font-medium">Start Date</th>
                  <th class="pb-3 font-medium">End Date</th>
                  <th class="pb-3 font-medium">Days</th>
                  <th class="pb-3 font-medium">Status</th>
                  <th class="pb-3 font-medium">Actions</th>
                </tr>
              </thead>
              <tbody>
                @for (request of requests(); track request.id) {
                  <tr class="border-b border-[var(--border)] last:border-0 hover:bg-[var(--background)] transition-colors">
                    <td class="py-4">
                      <div class="flex items-center gap-2">
                        <div class="w-2 h-2 rounded-full" [style.background-color]="request.leaveTypeColor"></div>
                        <span class="font-medium">{{ request.leaveTypeName }}</span>
                      </div>
                    </td>
                    <td class="py-4 text-[var(--text-secondary)]">{{ request.startDate | date }}</td>
                    <td class="py-4 text-[var(--text-secondary)]">{{ request.endDate | date }}</td>
                    <td class="py-4">{{ request.totalDays }}</td>
                    <td class="py-4">
                      <span [class]="getStatusClass(request.status)">
                        {{ getStatusLabel(request.status) }}
                      </span>
                    </td>
                    <td class="py-4">
                      @if (request.status === LeaveStatus.Pending) {
                        <button (click)="cancelRequest(request.id)" 
                                class="text-red-600 hover:text-red-700 text-sm font-medium">
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
          <div class="text-center py-12 text-[var(--text-secondary)]">
            <svg class="w-16 h-16 mx-auto mb-4 text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
            <p class="mb-2">No leave requests yet</p>
            <a routerLink="/leave-request" class="text-emerald-600 hover:underline">Request your first leave</a>
          </div>
        }
      </div>
    </div>
  `
})
export class MyLeavesComponent implements OnInit {
  requests = signal<LeaveRequest[]>([]);
  LeaveStatus = LeaveStatus;

  constructor(private leaveService: LeaveService) {}

  ngOnInit(): void {
    this.loadRequests();
  }

  loadRequests(): void {
    this.leaveService.getMyRequests().subscribe({
      next: (data) => this.requests.set(data)
    });
  }

  cancelRequest(id: string): void {
    if (confirm('Are you sure you want to cancel this request?')) {
      this.leaveService.cancelRequest(id).subscribe({
        next: () => this.loadRequests()
      });
    }
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
}
