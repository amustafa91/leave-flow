import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SettingsService } from '../../core/services/settings.service';
import { CompanySettings, CountryWorkday } from '../../core/models/models';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="animate-fadeIn max-w-4xl mx-auto space-y-8">
      <h1 class="text-2xl font-bold text-[var(--text-primary)] mb-6">Settings</h1>
      
      @if (isLoading()) {
          <div class="flex justify-center p-8"><span class="loading-spinner"></span></div>
      } @else {
          <!-- Global Config -->
          <div class="bg-white rounded-xl shadow-sm border border-[var(--border)] p-6 space-y-6">
              <h3 class="text-lg font-semibold text-[var(--text-primary)]">Global Configuration</h3>
              
              <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                      <label class="block text-sm font-medium text-[var(--text-secondary)] mb-1">Company Name</label>
                      <input type="text" [(ngModel)]="settings.companyName" class="input-field w-full">
                  </div>
                  <div>
                      <label class="block text-sm font-medium text-[var(--text-secondary)] mb-1">Default Country (ISO Code)</label>
                      <input type="text" [(ngModel)]="settings.defaultCountryCode" placeholder="AE" class="input-field w-full uppercase" maxlength="2">
                  </div>
                  <div>
                      <label class="block text-sm font-medium text-[var(--text-secondary)] mb-1">Standard Daily Hours</label>
                      <input type="number" [(ngModel)]="settings.defaultDailyWorkingHours" class="input-field w-full">
                  </div>
              </div>

               <div class="flex justify-end pt-4">
                  <button (click)="saveGlobal()" [disabled]="isSaving()" class="btn-primary min-w-[120px]">
                      {{ isSaving() ? 'Saving...' : 'Save Global Settings' }}
                  </button>
              </div>
          </div>

          <!-- Country Specific Workdays -->
          <div class="bg-white rounded-xl shadow-sm border border-[var(--border)] p-6 space-y-6">
              <div class="flex justify-between items-center">
                  <h3 class="text-lg font-semibold text-[var(--text-primary)]">Country Workdays</h3>
                  <div class="flex gap-2">
                       <input type="text" [(ngModel)]="selectedCountryCode" placeholder="Enter Country Code (e.g. US)" 
                              class="input-field w-24 uppercase text-center font-bold" maxlength="2">
                       <button (click)="loadCountrySettings()" class="px-3 py-2 bg-gray-100 rounded hover:bg-gray-200">
                           Load
                       </button>
                  </div>
              </div>

              @if (currentCountrySettings) {
                  <div class="animate-fadeIn p-4 bg-gray-50 rounded-lg border border-gray-100">
                      <h4 class="font-medium text-gray-700 mb-4">Working Days for {{ currentCountrySettings.countryCode }}</h4>
                      
                      <div class="grid grid-cols-2 md:grid-cols-7 gap-2">
                          @for (day of days; track day.key) {
                              <label class="flex flex-col items-center gap-2 p-3 rounded bg-white border cursor-pointer hover:border-blue-500 transition-colors"
                                     [class.border-blue-500]="currentCountrySettings[day.key]"
                                     [class.bg-blue-50]="currentCountrySettings[day.key]">
                                  <span class="text-sm font-medium">{{ day.label }}</span>
                                  <input type="checkbox" [(ngModel)]="currentCountrySettings[day.key]" class="w-5 h-5 text-blue-600 rounded">
                              </label>
                          }
                      </div>

                      <div class="flex justify-end mt-4">
                          <button (click)="saveCountry()" [disabled]="isSavingCountry()" class="btn-primary">
                             {{ isSavingCountry() ? 'Saving...' : 'Save ' + currentCountrySettings.countryCode + ' Workdays' }}
                          </button>
                      </div>
                  </div>
              }
          </div>
      }
    </div>
  `
})
export class SettingsComponent {
  settings: CompanySettings = {} as CompanySettings;
  
  // Country Settings
  selectedCountryCode = 'AE';
  currentCountrySettings: any = null; // Using any for dynamic property access
  
  isLoading = signal(true);
  isSaving = signal(false);
  isSavingCountry = signal(false);

  days = [
      { key: 'monday', label: 'Mon' },
      { key: 'tuesday', label: 'Tue' },
      { key: 'wednesday', label: 'Wed' },
      { key: 'thursday', label: 'Thu' },
      { key: 'friday', label: 'Fri' },
      { key: 'saturday', label: 'Sat' },
      { key: 'sunday', label: 'Sun' }
  ];

  private apiUrl = `${environment.apiUrl}/settings`;

  constructor(private settingsService: SettingsService, private http: HttpClient) {
      this.loadSettings();
  }

  loadSettings() {
      this.settingsService.getSettings().subscribe({
          next: (data) => {
              this.settings = data;
              this.selectedCountryCode = data.defaultCountryCode || 'AE';
              this.loadCountrySettings(); // Auto-load default
              this.isLoading.set(false);
          },
          error: () => this.isLoading.set(false)
      });
  }

  loadCountrySettings() {
      if (!this.selectedCountryCode) return;
      this.http.get<CountryWorkday>(`${this.apiUrl}/country/${this.selectedCountryCode}`).subscribe({
          next: (res) => this.currentCountrySettings = res,
          error: () => {
             // Default if not found
             this.currentCountrySettings = {
                 countryCode: this.selectedCountryCode.toUpperCase(),
                 monday: true, tuesday: true, wednesday: true, thursday: true, friday: true,
                 saturday: false, sunday: false
             };
          }
      });
  }

  saveGlobal() {
      this.isSaving.set(true);
      this.settingsService.updateSettings(this.settings).subscribe({
          next: (data) => {
              this.settings = data;
              this.isSaving.set(false);
              alert('Global settings updated!');
          },
          error: () => {
              alert('Failed to update global settings');
              this.isSaving.set(false);
          }
      });
  }

  saveCountry() {
      if (!this.currentCountrySettings) return;
      this.isSavingCountry.set(true);
      this.http.post<CountryWorkday>(`${this.apiUrl}/country`, this.currentCountrySettings).subscribe({
          next: (res) => {
              this.currentCountrySettings = res;
              this.isSavingCountry.set(false);
              alert(`Settings for ${res.countryCode} saved!`);
          },
          error: (err) => {
              alert('Failed to save country settings');
              this.isSavingCountry.set(false);
          }
      });
  }
}
