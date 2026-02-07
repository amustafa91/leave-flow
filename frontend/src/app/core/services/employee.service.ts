import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Employee } from '../models/models';

@Injectable({ providedIn: 'root' })
export class EmployeeService {
  private readonly apiUrl = `${environment.apiUrl}/employees`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Employee[]> {
    return this.http.get<Employee[]>(this.apiUrl);
  }

  getById(id: string): Observable<Employee> {
    return this.http.get<Employee>(`${this.apiUrl}/${id}`);
  }

  create(employee: Partial<Employee> & { password: string }): Observable<Employee> {
    return this.http.post<Employee>(this.apiUrl, employee);
  }

  update(id: string, employee: Partial<Employee>): Observable<Employee> {
    return this.http.put<Employee>(`${this.apiUrl}/${id}`, employee);
  }

  revealSensitiveField(id: string, fieldName: string): Observable<{ fieldName: string; value: string }> {
    return this.http.post<{ fieldName: string; value: string }>(`${this.apiUrl}/${id}/reveal`, { fieldName });
  }

  updateProfile(data: any, passportFile?: File, nationalIdFile?: File): Observable<Employee> {
    const formData = new FormData();
    if (data.phone !== undefined && data.phone !== null) formData.append('phone', data.phone);
    if (data.address !== undefined && data.address !== null) formData.append('address', data.address);
    if (data.emergencyContact !== undefined && data.emergencyContact !== null) formData.append('emergencyContact', data.emergencyContact);
    if (data.bankAccount !== undefined && data.bankAccount !== null) formData.append('bankAccount', data.bankAccount);
    
    if (passportFile) formData.append('passportFile', passportFile);
    if (nationalIdFile) formData.append('nationalIdFile', nationalIdFile);

    return this.http.put<Employee>(`${this.apiUrl}/profile`, formData);
  }

  getBalances(id: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/${id}/balances`);
  }

  updateLeaveBalance(id: string, leaveTypeId: string, totalHours: number, note?: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/leave-balance/${leaveTypeId}`, { totalHours, note });
  }
}
