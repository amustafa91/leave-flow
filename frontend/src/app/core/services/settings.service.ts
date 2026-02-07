import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CompanySettings } from '../models/models';

@Injectable({
  providedIn: 'root'
})
export class SettingsService {
  private apiUrl = `${environment.apiUrl}/settings`;

  constructor(private http: HttpClient) {}

  getSettings(): Observable<CompanySettings> {
    return this.http.get<CompanySettings>(this.apiUrl);
  }

  updateSettings(settings: Partial<CompanySettings>): Observable<CompanySettings> {
    return this.http.put<CompanySettings>(this.apiUrl, settings);
  }
}
