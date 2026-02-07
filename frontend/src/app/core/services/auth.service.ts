import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse, LoginRequest, Employee, UserRole } from '../models/models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiUrl = `${environment.apiUrl}/auth`;
  
  private currentUser = signal<Employee | null>(null);
  private token = signal<string | null>(null);

  user = this.currentUser.asReadonly();
  isAuthenticated = computed(() => !!this.token());
  isAdmin = computed(() => {
    const user = this.currentUser();
    return user?.role === UserRole.SuperAdmin || user?.role === UserRole.HRAdmin;
  });
  isApprover = computed(() => {
    const user = this.currentUser();
    return user?.role === UserRole.Approver || user?.role === UserRole.HRAdmin || user?.role === UserRole.SuperAdmin;
  });

  constructor(private http: HttpClient, private router: Router) {
    this.loadFromStorage();
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, request).pipe(
      tap(response => this.handleAuthResponse(response))
    );
  }

  logout(): void {
    this.currentUser.set(null);
    this.token.set(null);
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return this.token();
  }

  private handleAuthResponse(response: AuthResponse): void {
    this.token.set(response.token);
    this.currentUser.set(response.user);
    localStorage.setItem('token', response.token);
    localStorage.setItem('user', JSON.stringify(response.user));
  }

  private loadFromStorage(): void {
    const token = localStorage.getItem('token');
    const userStr = localStorage.getItem('user');
    if (token && userStr) {
      this.token.set(token);
      this.currentUser.set(JSON.parse(userStr));
      this.currentUser.set(JSON.parse(userStr));
    }
  }

  updateUser(user: Employee): void {
    this.currentUser.set(user);
    localStorage.setItem('user', JSON.stringify(user));
  }
}
