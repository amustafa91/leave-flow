import { Component, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService } from '../../core/services/notification.service';
import { NotificationType } from '../../core/models/models';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="animate-fadeIn">
      <div class="flex justify-between items-center mb-6">
        <h1 class="text-2xl font-bold text-[var(--text-primary)]">Notifications</h1>
        @if (notificationService.unreadCount() > 0) {
          <button (click)="markAllRead()" class="text-sm text-emerald-600 hover:text-emerald-800 font-medium">
            Mark all as read
          </button>
        }
      </div>

      <div class="space-y-4">
        @for (notification of notificationService.notifications(); track notification.id) {
          <div class="card p-4 border-l-4 transition-colors"
               [class.border-l-emerald-500]="notification.type === 1"
               [class.border-l-blue-500]="notification.type === 0"
               [class.border-l-amber-500]="notification.type === 2"
               [class.border-l-red-500]="notification.type === 3"
               [class.bg-gray-50]="notification.isRead">
            
            <div class="flex justify-between items-start">
              <div class="flex-1">
                <h3 class="font-bold text-[var(--text-primary)]" [class.text-gray-500]="notification.isRead">
                  {{ notification.title }}
                </h3>
                <p class="text-sm text-[var(--text-secondary)] mt-1">{{ notification.message }}</p>
                <p class="text-xs text-gray-400 mt-2">{{ notification.createdAt | date }}</p>
              </div>
              
              @if (!notification.isRead) {
                <button (click)="markRead(notification.id)" 
                        class="ml-4 w-3 h-3 rounded-full bg-emerald-500 hover:bg-emerald-600"
                        title="Mark as read">
                </button>
              }
            </div>
          </div>
        }

        @if (notificationService.notifications().length === 0) {
          <div class="text-center py-12 text-[var(--text-secondary)]">
            <svg class="w-16 h-16 mx-auto mb-4 text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
            </svg>
            <p>No notifications yet</p>
          </div>
        }
      </div>
    </div>
  `
})
export class NotificationsComponent {
  constructor(public notificationService: NotificationService) {}

  markRead(id: string) {
    this.notificationService.markAsRead(id).subscribe();
  }

  markAllRead() {
    this.notificationService.markAllAsRead().subscribe();
  }
}
