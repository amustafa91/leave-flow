import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LeaveService } from '../../core/services/leave.service';
import { LeaveRequest, LeaveStatus } from '../../core/models/models';

@Component({
  selector: 'app-approvals',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="animate-fadeIn">
      <h1 class="text-2xl font-bold text-[var(--text-primary)] mb-6">Pending Approvals</h1>

      @if (pendingRequests().length > 0) {
        <div class="grid gap-4">
          @for (request of pendingRequests(); track request.id) {
            <div class="card hover:shadow-md transition-shadow">
              <div class="flex items-start justify-between">
                <div class="flex-1">
                  <div class="flex items-center gap-3 mb-2">
                    <div class="w-10 h-10 rounded-full bg-gradient-to-br from-emerald-400 to-cyan-400 flex items-center justify-center text-white font-medium text-sm">
                      {{ (request.employeeName.split(' ')[0] || '')[0] || '' }}{{ (request.employeeName.split(' ')[1] || '')[0] || '' }}
                    </div>
                    <div>
                      <p class="font-semibold text-[var(--text-primary)]">{{ request.employeeName }}</p>
                      <p class="text-sm text-[var(--text-secondary)]">
                        Requested on {{ request.createdAt | date }}
                      </p>
                    </div>
                  </div>
                  
                  <div class="flex items-center gap-4 mt-4 text-sm">
                    <div class="flex items-center gap-2">
                      <div class="w-2 h-2 rounded-full" [style.background-color]="request.leaveTypeColor"></div>
                      <span>{{ request.leaveTypeName }}</span>
                    </div>
                    <span class="text-[var(--text-secondary)]">|</span>
                    <span>{{ request.startDate | date }} - {{ request.endDate | date }}</span>
                    <span class="text-[var(--text-secondary)]">|</span>
                    <span class="font-medium">{{ request.totalDays }} days</span>
                  </div>

                  @if (request.reason) {
                    <p class="mt-3 text-sm text-[var(--text-secondary)] italic">
                      "{{ request.reason }}"
                    </p>
                  }
                </div>

                <div class="flex gap-2 ml-4">
                  <button (click)="reject(request.id)" 
                          class="btn btn-secondary text-red-600 border-red-200 hover:bg-red-50">
                    Reject
                  </button>
                  <button (click)="approve(request.id)" 
                          class="btn btn-primary">
                    Approve
                  </button>
                </div>
              </div>
            </div>
          }
        </div>
      } @else {
        <div class="card text-center py-12">
          <svg class="w-16 h-16 mx-auto mb-4 text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <p class="text-[var(--text-secondary)]">No pending requests</p>
          <p class="text-sm text-[var(--text-secondary)] mt-1">All caught up! 🎉</p>
        </div>
      }
    </div>
  `
})
export class ApprovalsComponent implements OnInit {
  pendingRequests = signal<LeaveRequest[]>([]);

  constructor(private leaveService: LeaveService) {}

  ngOnInit(): void {
    this.loadPending();
  }

  loadPending(): void {
    this.leaveService.getPendingRequests().subscribe({
      next: (data) => this.pendingRequests.set(data)
    });
  }

  approve(id: string): void {
    this.leaveService.approveRequest(id, { approve: true }).subscribe({
      next: () => this.loadPending()
    });
  }

  reject(id: string): void {
    const comments = prompt('Reason for rejection (optional):');
    this.leaveService.approveRequest(id, { approve: false, comments: comments || undefined }).subscribe({
      next: () => this.loadPending()
    });
  }
}
