import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { LeaveService } from '../../core/services/leave.service';
import { LeaveType, LeaveBalance } from '../../core/models/models';

@Component({
  selector: 'app-leave-request',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="max-w-2xl animate-fadeIn">
      <h1 class="text-2xl font-bold text-[var(--text-primary)] mb-6">Request Leave</h1>

      <div class="card">
        <form [formGroup]="form" (ngSubmit)="onSubmit()" class="space-y-6">
          <!-- Leave Type -->
          <div>
            <label class="block text-sm font-medium text-[var(--text-primary)] mb-2">Leave Type</label>
            <div class="grid grid-cols-2 md:grid-cols-3 gap-3">
              @for (type of leaveTypes(); track type.id) {
                <button type="button" 
                        (click)="selectLeaveType(type)"
                        [class]="'p-4 rounded-xl border-2 transition-all text-left ' + 
                                 (selectedType()?.id === type.id ? 'border-emerald-500 bg-emerald-50' : 'border-[var(--border)] hover:border-emerald-300')">
                  <div class="flex items-center gap-2 mb-1">
                    <div class="w-3 h-3 rounded-full" [style.background-color]="type.colorCode"></div>
                    <span class="font-medium text-[var(--text-primary)]">{{ type.name }}</span>
                  </div>
                  <div class="text-xs text-[var(--text-secondary)]">
                    {{ getBalance(type.id)?.remainingDays || 0 }} days remaining
                  </div>
                </button>
              }
            </div>
          </div>

          <!-- Balance Info -->
          @if (selectedType()) {
             <div class="p-4 bg-blue-50 border border-blue-200 rounded-xl animate-fadeIn">
                <div class="flex flex-col sm:flex-row justify-between sm:items-center gap-2">
                   <span class="font-medium text-blue-900">{{ selectedType()?.name }} Balance</span>
                   <div class="flex gap-4 text-sm">
                      <div class="flex flex-col">
                        <span class="text-blue-700 text-xs uppercase tracking-wider">Remaining</span>
                        <span class="font-bold text-blue-900 text-lg">{{ getBalance(selectedType()!.id)?.remainingDays || 0 }} days</span>
                      </div>
                      <div class="flex flex-col border-l border-blue-200 pl-4">
                        <span class="text-blue-700 text-xs uppercase tracking-wider">Accrued</span>
                        <span class="font-bold text-blue-900 text-lg">{{ getBalance(selectedType()!.id)?.accruedDays || 0 }} days</span>
                      </div>
                      <div class="flex flex-col border-l border-blue-200 pl-4">
                        <span class="text-blue-700 text-xs uppercase tracking-wider">Total/Year</span>
                        <span class="text-blue-900 text-lg">{{ getBalance(selectedType()!.id)?.totalDays || 0 }} days</span>
                      </div>
                   </div>
                </div>
             </div>
          }

          <!-- Dates -->
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-[var(--text-primary)] mb-1.5">Start Date</label>
              <input type="date" formControlName="startDate" class="input" />
            </div>
            <div>
              <label class="block text-sm font-medium text-[var(--text-primary)] mb-1.5">End Date</label>
              <input type="date" formControlName="endDate" class="input" />
            </div>
          </div>

          <!-- Leave Type: Full Day vs Hours -->
          <div class="space-y-3">
            <div class="flex items-center gap-3">
              <input type="checkbox" formControlName="isFullDay" id="fullDay" 
                     class="w-4 h-4 rounded border-[var(--border)] text-emerald-600 focus:ring-emerald-500" />
              <label for="fullDay" class="text-sm text-[var(--text-primary)]">Full day(s)</label>
            </div>
            
            @if (!form.get('isFullDay')?.value) {
              <div>
                <label class="block text-sm font-medium text-[var(--text-secondary)] mb-1">Hours</label>
                <input type="number" formControlName="leaveHours" step="0.5" min="0.5"
                       class="input w-32" placeholder="4" />
              </div>
            }
          </div>

          <!-- Reason -->
          <div>
            <label class="block text-sm font-medium text-[var(--text-primary)] mb-1.5">Reason (optional)</label>
            <textarea formControlName="reason" rows="3" class="input resize-none" 
                      placeholder="Briefly describe why you're taking leave..."></textarea>
          </div>

          <!-- Attachment -->
          <div>
            <label class="block text-sm font-medium text-[var(--text-primary)] mb-1.5">Attachment (optional)</label>
            <div class="flex flex-col gap-2">
              <input type="file" (change)="onFileSelected($event)" 
                     class="block w-full text-sm text-[var(--text-secondary)]
                            file:mr-4 file:py-2 file:px-4
                            file:rounded-full file:border-0
                            file:text-sm file:font-semibold
                            file:bg-emerald-50 file:text-emerald-700
                            hover:file:bg-emerald-100" />
              <p class="text-xs text-[var(--text-secondary)]">
                Recommended for Sick Leave. Max 5MB.
              </p>
            </div>
          </div>

          <!-- Summary -->
          @if (selectedType() && form.get('startDate')?.value && form.get('endDate')?.value) {
            <div class="p-4 rounded-xl bg-emerald-50 border border-emerald-200">
              <div class="flex justify-between items-center">
                <span class="text-emerald-800">Total requested:</span>
                <span class="text-2xl font-bold text-emerald-700">
                  @if (form.get('isFullDay')?.value) {
                    {{ calculateDays() }} days
                  } @else {
                    {{ form.get('leaveHours')?.value || 0 }} hours
                  }
                </span>
              </div>
            </div>
          }

          @if (error) {
            <div class="p-3 rounded-lg bg-red-50 border border-red-200 text-red-700 text-sm">
              {{ error }}
            </div>
          }

          <!-- Actions -->
          <div class="flex gap-3 pt-4">
            <button type="button" (click)="cancel()" class="btn btn-secondary flex-1">Cancel</button>
            <button type="submit" [disabled]="loading || form.invalid || !selectedType()" 
                    class="btn btn-primary flex-1 disabled:opacity-50">
              @if (loading) {
                Submitting...
              } @else {
                Submit Request
              }
            </button>
          </div>
        </form>
      </div>
    </div>
  `
})
export class LeaveRequestComponent implements OnInit {
  form: FormGroup;
  leaveTypes = signal<LeaveType[]>([]);
  balances = signal<LeaveBalance[]>([]);
  selectedType = signal<LeaveType | null>(null);
  selectedFile: File | null = null;
  loading = false;
  error = '';

  constructor(
    private fb: FormBuilder,
    private leaveService: LeaveService,
    private router: Router
  ) {
    this.form = this.fb.group({
      startDate: ['', Validators.required],
      endDate: ['', Validators.required],
      isFullDay: [true],
      leaveHours: [8],
      reason: ['']
    });
  }

  ngOnInit(): void {
    this.leaveService.getLeaveTypes().subscribe({
      next: (types) => this.leaveTypes.set(types)
    });
    this.leaveService.getMyBalances().subscribe({
      next: (balances) => this.balances.set(balances)
    });
  }

  selectLeaveType(type: LeaveType): void {
    this.selectedType.set(type);
  }

  getBalance(typeId: string): LeaveBalance | undefined {
    return this.balances().find(b => b.leaveTypeId === typeId);
  }

  calculateDays(): number {
    const start = new Date(this.form.get('startDate')?.value);
    const end = new Date(this.form.get('endDate')?.value);
    const diffTime = Math.abs(end.getTime() - start.getTime());
    return Math.ceil(diffTime / (1000 * 60 * 60 * 24)) + 1;
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      if (file.size > 5 * 1024 * 1024) {
        this.error = 'File size exceeds 5MB';
        event.target.value = '';
        this.selectedFile = null;
        return;
      }
      this.selectedFile = file;
      this.error = '';
    }
  }

  onSubmit(): void {
    if (this.form.invalid || !this.selectedType()) return;

    this.loading = true;
    this.error = '';

    const request = {
      leaveTypeId: this.selectedType()!.id,
      startDate: new Date(this.form.get('startDate')?.value),
      endDate: new Date(this.form.get('endDate')?.value),
      isFullDay: this.form.get('isFullDay')?.value,
      leaveHours: this.form.get('isFullDay')?.value ? 0 : (this.form.get('leaveHours')?.value || 8),
      reason: this.form.get('reason')?.value
    };

    this.leaveService.createRequest(request, this.selectedFile || undefined).subscribe({
      next: () => {
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.error = err.error?.message || 'Failed to submit request';
        this.loading = false;
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/dashboard']);
  }
}
