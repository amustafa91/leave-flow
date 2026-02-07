import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, timer, switchMap, retry, share } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Notification } from '../models/models';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private apiUrl = `${environment.apiUrl}/notifications`;
  
  // State
  notifications = signal<Notification[]>([]);
  unreadCount = signal(0);

  constructor(private http: HttpClient, private authService: AuthService) {
    // Start polling if authenticated
    this.startPolling();
  }

  private startPolling() {
    timer(0, 30000) // Poll every 30s
      .pipe(
        switchMap(() => {
          if (this.authService.isAuthenticated()) {
            return this.getMyNotifications();
          }
          return [];
        }),
        retry(3)
      )
      .subscribe();
  }

  getMyNotifications(): Observable<Notification[]> {
    return this.http.get<Notification[]>(this.apiUrl).pipe(
      tap(data => {
        this.notifications.set(data);
        this.unreadCount.set(data.filter(n => !n.isRead).length);
      })
    );
  }

  markAsRead(id: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/read`, {}).pipe(
      tap(() => {
         this.notifications.update(list => 
            list.map(n => n.id === id ? { ...n, isRead: true } : n)
         );
         this.unreadCount.set(this.notifications().filter(n => !n.isRead).length);
      })
    );
  }

  markAllAsRead(): Observable<any> {
    return this.http.put(`${this.apiUrl}/read-all`, {}).pipe(
      tap(() => {
         this.notifications.update(list => 
            list.map(n => ({ ...n, isRead: true }))
         );
         this.unreadCount.set(0);
      })
    );
  }
}
