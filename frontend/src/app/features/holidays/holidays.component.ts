import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { AuthService } from '../../core/services/auth.service';
// Trigger rebuild

interface Holiday {
  id: string;
  date: string;
  name: string;
  localName: string;
  countryCode: string;
  isCustom: boolean;
  isModified: boolean;
}

@Component({
  selector: 'app-holidays',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="animate-fadeIn space-y-6">
      <div class="flex justify-between items-center">
        <h1 class="text-2xl font-bold text-[var(--text-primary)]">Public Holidays</h1>
        
        <div class="flex gap-3" *ngIf="hasAdminPrivileges">
             <button (click)="syncHolidays()" [disabled]="isLoading()"
                class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors flex items-center gap-2">
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                </svg>
                Sync with Global Calendar
             </button>
        </div>
      </div>

      <!-- Filters -->
      <div class="flex gap-4 bg-white p-4 rounded-xl shadow-sm border border-[var(--border)]">
        <div class="flex-1">
            <label class="block text-sm font-medium text-[var(--text-secondary)] mb-1">Country</label>
            <select [(ngModel)]="selectedCountry" (change)="loadHolidays()" class="input-field w-full">
                <option value="AE">United Arab Emirates</option>
                <option value="IN">India</option>
                <option value="PK">Pakistan</option>
                <option value="JO">Jordan</option>
                <option value="EG">Egypt</option>
                <option value="US">United States</option>
                <option value="GB">United Kingdom</option>
            </select>
        </div>
        <div class="flex-1">
            <label class="block text-sm font-medium text-[var(--text-secondary)] mb-1">Year</label>
            <select [(ngModel)]="selectedYear" (change)="loadHolidays()" class="input-field w-full">
                <option [value]="2024">2024</option>
                <option [value]="2025">2025</option>
                <option [value]="2026">2026</option>
            </select>
        </div>
      </div>

      <!-- Add Custom Form (Admin Only) -->
      <div *ngIf="hasAdminPrivileges" class="bg-white p-6 rounded-xl shadow-sm border border-[var(--border)]">
         <h3 class="font-bold text-lg mb-4">Add Custom Holiday</h3>
         <div class="grid grid-cols-1 md:grid-cols-4 gap-4 items-end">
             <div>
                 <label class="block text-sm font-medium text-[var(--text-secondary)] mb-1">Date</label>
                 <input type="date" [(ngModel)]="newHoliday.date" class="input-field w-full">
             </div>
             <div class="md:col-span-2">
                 <label class="block text-sm font-medium text-[var(--text-secondary)] mb-1">Name</label>
                 <input type="text" [(ngModel)]="newHoliday.name" placeholder="E.g. Company Foundation Day" class="input-field w-full">
             </div>
             <button (click)="addHoliday()" class="btn-primary w-full">Add Holiday</button>
         </div>
      </div>

      <!-- List -->
      <div class="bg-white rounded-xl shadow-sm border border-[var(--border)] overflow-hidden">
        <table class="w-full text-left">
            <thead class="bg-gray-50 border-b border-[var(--border)]">
                <tr>
                    <th class="p-4 font-semibold text-[var(--text-secondary)]">Date</th>
                    <th class="p-4 font-semibold text-[var(--text-secondary)]">Holiday Name</th>
                    <th class="p-4 font-semibold text-[var(--text-secondary)]">Local Name</th>
                    <th class="p-4 font-semibold text-[var(--text-secondary)]">Source</th>
                    <th *ngIf="hasAdminPrivileges" class="p-4 font-semibold text-[var(--text-secondary)]">Actions</th>
                </tr>
            </thead>
            <tbody class="divide-y divide-[var(--border)]">
                @for (h of holidays(); track h.id) {
                    <tr class="hover:bg-gray-50 transition-colors">
                        @if (editingId === h.id) {
                            <!-- Edit Mode -->
                            <td class="p-3">
                                <input type="date" [(ngModel)]="editHoliday.date" class="input-field w-full text-sm">
                            </td>
                            <td class="p-3">
                                <input type="text" [(ngModel)]="editHoliday.name" class="input-field w-full text-sm">
                            </td>
                            <td class="p-3">
                                <input type="text" [(ngModel)]="editHoliday.localName" class="input-field w-full text-sm">
                            </td>
                            <td class="p-4">
                                <span [class]="h.isCustom ? 'badge-amber' : (h.isModified ? 'badge-amber' : 'badge-blue')">
                                    {{ h.isCustom ? 'Custom' : (h.isModified ? 'Official (Edited)' : 'Official') }}
                                </span>
                            </td>
                            <td class="p-3 flex gap-2">
                                <button (click)="saveEdit()" class="text-green-600 hover:text-green-800 font-medium">Save</button>
                                <button (click)="cancelEdit()" class="text-gray-500 hover:text-gray-700">Cancel</button>
                            </td>
                        } @else {
                            <!-- View Mode -->
                            <td class="p-4">{{ h.date | date }}</td>
                            <td class="p-4 font-medium">{{ h.name }}</td>
                            <td class="p-4 text-gray-500">{{ h.localName || '-' }}</td>
                            <td class="p-4">
                                <span [class]="h.isCustom ? 'badge-amber' : (h.isModified ? 'badge-amber' : 'badge-blue')">
                                    {{ h.isCustom ? 'Custom' : (h.isModified ? 'Official (Edited)' : 'Official') }}
                                </span>
                            </td>
                            <td *ngIf="hasAdminPrivileges" class="p-4 flex gap-3">
                                <button (click)="startEdit(h)" class="text-blue-600 hover:text-blue-800">Edit</button>
                                <button (click)="deleteHoliday(h.id)" class="text-red-500 hover:text-red-700">Delete</button>
                            </td>
                        }
                    </tr>
                }
                @if (holidays().length === 0) {
                    <tr><td [attr.colspan]="hasAdminPrivileges ? 5 : 4" class="p-8 text-center text-gray-500">No holidays found for this selection.</td></tr>
                }
            </tbody>
        </table>
      </div>
    </div>
  `
})
export class HolidaysComponent {

  holidays = signal<Holiday[]>([]);
  selectedCountry = 'AE';
  selectedYear = new Date().getFullYear();
  isLoading = signal(false);

  newHoliday = { date: '', name: '' };

  // Edit state
  editingId: string | null = null;
  editHoliday = { date: '', name: '', localName: '' };

  private apiUrl = `${environment.apiUrl}/holidays`;

  constructor(private http: HttpClient, public authService: AuthService) {
      this.loadHolidays();
  }

  get hasAdminPrivileges(): boolean {
      return this.authService.isAdmin();
  }

  loadHolidays() {
      this.isLoading.set(true);
      this.http.get<Holiday[]>(`${this.apiUrl}/${this.selectedCountry}/${this.selectedYear}`)
          .subscribe({
              next: (data) => {
                  this.holidays.set(data);
                  this.isLoading.set(false);
              },
              error: () => this.isLoading.set(false)
          });
  }

  syncHolidays() {
      if (!confirm(`Are you sure you want to sync official holidays for ${this.selectedCountry} ${this.selectedYear}?`)) return;
      
      this.isLoading.set(true);
      this.http.post<Holiday[]>(`${this.apiUrl}/sync/${this.selectedCountry}/${this.selectedYear}`, {})
          .subscribe({
              next: (data) => {
                  this.holidays.set(data);
                  this.isLoading.set(false);
                  alert(`Successfully synced ${data.length} holidays!`);
              },
              error: (err) => {
                  alert("Failed to sync: " + (err.error?.message || err.message));
                  this.isLoading.set(false);
              }
          });
  }

  addHoliday() {
      if (!this.newHoliday.date || !this.newHoliday.name) return;

      this.http.post<Holiday>(this.apiUrl, {
          date: this.newHoliday.date,
          name: this.newHoliday.name,
          countryCode: this.selectedCountry
      }).subscribe({
          next: (h) => {
              this.holidays.update(list => [...list, h].sort((a,b) => a.date.localeCompare(b.date)));
              this.newHoliday = { date: '', name: '' };
          },
          error: (err) => alert(err.message)
      });
  }

  startEdit(h: Holiday) {
      this.editingId = h.id;
      this.editHoliday = {
          date: h.date.substring(0, 10), // Format for date input
          name: h.name,
          localName: h.localName || ''
      };
  }

  cancelEdit() {
      this.editingId = null;
  }

  saveEdit() {
      if (!this.editingId) return;

      this.http.put<Holiday>(`${this.apiUrl}/${this.editingId}`, {
          date: this.editHoliday.date,
          name: this.editHoliday.name,
          localName: this.editHoliday.localName,
          countryCode: this.selectedCountry
      }).subscribe({
          next: (updated) => {
              this.holidays.update(list => 
                  list.map(h => h.id === this.editingId ? updated : h)
                      .sort((a,b) => a.date.localeCompare(b.date))
              );
              this.editingId = null;
          },
          error: (err) => alert("Failed to update: " + (err.error?.message || err.message))
      });
  }

  deleteHoliday(id: string) {
      if (!confirm('Delete this holiday?')) return;
      this.http.delete(`${this.apiUrl}/${id}`).subscribe({
          next: () => {
              this.holidays.update(list => list.filter(h => h.id !== id));
          }
      });
  }
}
