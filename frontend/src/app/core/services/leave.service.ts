import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LeaveRequest, LeaveBalance, LeaveType, CreateLeaveRequest, ApproveLeaveRequest } from '../models/models';

@Injectable({ providedIn: 'root' })
export class LeaveService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  // Leave Types
  getLeaveTypes(): Observable<LeaveType[]> {
    return this.http.get<LeaveType[]>(`${this.apiUrl}/leavetypes`);
  }

  // Leave Requests
  getMyRequests(): Observable<LeaveRequest[]> {
    return this.http.get<LeaveRequest[]>(`${this.apiUrl}/leaverequests`);
  }

  getPendingRequests(): Observable<LeaveRequest[]> {
    return this.http.get<LeaveRequest[]>(`${this.apiUrl}/leaverequests/pending`);
  }

  createRequest(request: CreateLeaveRequest, file?: File): Observable<LeaveRequest> {
    const formData = new FormData();
    formData.append('leaveTypeId', request.leaveTypeId);
    formData.append('startDate', request.startDate.toISOString());
    formData.append('endDate', request.endDate.toISOString());
    formData.append('isFullDay', String(request.isFullDay));
    formData.append('leaveHours', String(request.leaveHours));
    if (request.reason) formData.append('reason', request.reason);
    if (request.personalNotes) formData.append('personalNotes', request.personalNotes);
    if (file) formData.append('file', file);

    return this.http.post<LeaveRequest>(`${this.apiUrl}/leaverequests`, formData);
  }

  approveRequest(id: string, request: ApproveLeaveRequest): Observable<LeaveRequest> {
    return this.http.post<LeaveRequest>(`${this.apiUrl}/leaverequests/${id}/approve`, request);
  }

  cancelRequest(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/leaverequests/${id}`);
  }

  // Leave Balances
  getMyBalances(): Observable<LeaveBalance[]> {
    return this.http.get<LeaveBalance[]>(`${this.apiUrl}/leavebalances`);
  }

  getEmployeeBalances(employeeId: string): Observable<LeaveBalance[]> {
    return this.http.get<LeaveBalance[]>(`${this.apiUrl}/leavebalances/employee/${employeeId}`);
  }
}
