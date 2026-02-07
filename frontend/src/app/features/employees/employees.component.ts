import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EmployeeService } from '../../core/services/employee.service';
import { LeaveService } from '../../core/services/leave.service';
import { Employee, UserRole, SensitiveField, Gender, LeaveType } from '../../core/models/models';
import { SensitiveFieldComponent } from '../../shared/components/sensitive-field.component';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-employees',
  standalone: true,
  imports: [CommonModule, FormsModule, SensitiveFieldComponent],
  template: `
    <div class="animate-fadeIn">
      <div class="flex justify-between items-center mb-6">
        <h1 class="text-2xl font-bold text-[var(--text-primary)]">Employee Management</h1>
        <button class="btn btn-primary" (click)="openAddModal()">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
          </svg>
          Add Employee
        </button>
      </div>

      <div class="card">
        <div class="overflow-x-auto">
          <table class="w-full">
            <thead>
              <tr class="text-left text-[var(--text-secondary)] text-sm border-b border-[var(--border)]">
                <th class="pb-3 font-medium">Employee</th>
                <th class="pb-3 font-medium">Department</th>
                <th class="pb-3 font-medium">Role</th>
                <th class="pb-3 font-medium">Salary</th>
                <th class="pb-3 font-medium">Status</th>
                <th class="pb-3 font-medium">Actions</th>
              </tr>
            </thead>
            <tbody>
              @for (emp of employees(); track emp.id) {
                <tr class="border-b border-[var(--border)] last:border-0 hover:bg-[var(--background)]"
                    [class.bg-red-50]="!emp.isActive" [class.text-gray-500]="!emp.isActive">
                  <td class="py-4">
                    <div class="flex items-center gap-3">
                      <div class="w-10 h-10 rounded-full bg-gradient-to-br from-emerald-400 to-cyan-400 flex items-center justify-center text-white font-medium text-sm">
                        {{ (emp.firstName || '')[0] || '' }}{{ (emp.lastName || '')[0] || '' }}
                      </div>
                      <div>
                        <p class="font-medium text-[var(--text-primary)]">{{ emp.fullName }}</p>
                        <p class="text-sm text-[var(--text-secondary)]">{{ emp.email }}</p>
                        @if (!emp.isActive) {
                          <span class="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-red-100 text-red-800">Inactive</span>
                        }
                      </div>
                    </div>
                  </td>
                  <td class="py-4 text-[var(--text-secondary)]">{{ emp.department || '-' }}</td>
                  <td class="py-4">
                    <span [class]="getRoleBadge(emp.role)">{{ getRoleName(emp.role) }}</span>
                  </td>
                  <td class="py-4">
                    <app-sensitive-field 
                      [field]="getSalaryField(emp)"
                      (reveal)="revealSalary(emp.id)"
                      (hide)="hideSalary(emp.id)">
                    </app-sensitive-field>
                  </td>
                  <td class="py-4">
                    <span [class]="emp.isActive ? 'badge badge-approved' : 'badge badge-cancelled'">
                      {{ emp.isActive ? 'Active' : 'Inactive' }}
                    </span>
                  </td>
                  <td class="py-4 text-right">
                    <button class="btn btn-sm btn-secondary" (click)="openEditModal(emp)">Edit</button>
                    <button class="btn btn-sm btn-outline ml-2" (click)="openBalanceModal(emp)">Balances</button>
                    <button class="btn btn-sm ml-2" 
                            [class.text-red-600]="emp.isActive" 
                            [class.text-emerald-600]="!emp.isActive"
                            [class.bg-red-50]="emp.isActive"
                            [class.bg-emerald-50]="!emp.isActive"
                            (click)="toggleStatus(emp)">
                       {{ emp.isActive ? 'Disable' : 'Activate' }}
                    </button>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Add Employee Modal -->
    @if (showModal()) {
      <div class="fixed inset-0 bg-black/50 flex items-center justify-center z-50" (click)="closeModal()">
        <div class="bg-[var(--surface)] rounded-xl shadow-2xl p-6 w-full max-w-lg mx-4" (click)="$event.stopPropagation()">
          <h2 class="text-xl font-bold text-[var(--text-primary)] mb-4">{{ isEditing() ? 'Edit Employee' : 'Add New Employee' }}</h2>
          
          <form (ngSubmit)="saveEmployee()">
            <div class="grid grid-cols-2 gap-4 mb-4">
              <div>
                <label class="block text-sm font-medium text-[var(--text-secondary)] mb-1">First Name</label>
                <input type="text" [(ngModel)]="employeeForm.firstName" name="firstName" required
                  class="w-full px-3 py-2 rounded-lg border border-[var(--border)] bg-[var(--background)] text-[var(--text-primary)]">
              </div>
              <div>
                <label class="block text-sm font-medium text-[var(--text-secondary)] mb-1">Last Name</label>
                <input type="text" [(ngModel)]="employeeForm.lastName" name="lastName" required
                  class="w-full px-3 py-2 rounded-lg border border-[var(--border)] bg-[var(--background)] text-[var(--text-primary)]">
              </div>
            </div>

            <div class="mb-4">
              <label class="block text-sm font-medium text-[var(--text-secondary)] mb-1">Email</label>
              <input type="email" [(ngModel)]="employeeForm.email" name="email" required
                class="w-full px-3 py-2 rounded-lg border border-[var(--border)] bg-[var(--background)] text-[var(--text-primary)]">
            </div>

            @if (!isEditing()) {
              <div class="mb-4">
                <label class="block text-sm font-medium text-[var(--text-secondary)] mb-1">Password</label>
                <input type="password" [(ngModel)]="employeeForm.password" name="password" required
                  class="w-full px-3 py-2 rounded-lg border border-[var(--border)] bg-[var(--background)] text-[var(--text-primary)]">
              </div>
            }

            <div class="grid grid-cols-2 gap-4 mb-4">
              <div>
                <label class="block text-sm font-medium text-[var(--text-secondary)] mb-1">Gender</label>
                <select [(ngModel)]="employeeForm.gender" name="gender"
                  class="w-full px-3 py-2 rounded-lg border border-[var(--border)] bg-[var(--background)] text-[var(--text-primary)]">
                  <option [ngValue]="0">Male</option>
                  <option [ngValue]="1">Female</option>
                  <option [ngValue]="2">Other</option>
                </select>
              </div>
              <div>
                <label class="block text-sm font-medium text-[var(--text-secondary)] mb-1">Role</label>
                <select [(ngModel)]="employeeForm.role" name="role"
                  class="w-full px-3 py-2 rounded-lg border border-[var(--border)] bg-[var(--background)] text-[var(--text-primary)]">
                  <option [ngValue]="0">Employee</option>
                  <option [ngValue]="1">Approver/Manager</option>
                  <option [ngValue]="2">HR Admin</option>
                  <option [ngValue]="3">Super Admin</option>
                </select>
              </div>
            </div>

            <div class="mb-4">
              <label class="block text-sm font-medium text-[var(--text-secondary)] mb-1">Manager</label>
              <select [(ngModel)]="employeeForm.managerId" name="managerId"
                class="w-full px-3 py-2 rounded-lg border border-[var(--border)] bg-[var(--background)] text-[var(--text-primary)]">
                <option [ngValue]="null">-- No Manager --</option>
                @for (manager of managers(); track manager.id) {
                  <option [value]="manager.id">{{ manager.firstName }} {{ manager.lastName }}</option>
                }
              </select>
            </div>

            <div class="mb-4">
              <label class="block text-sm font-medium text-[var(--text-secondary)] mb-1">Department</label>
              <input type="text" [(ngModel)]="employeeForm.department" name="department"
                class="w-full px-3 py-2 rounded-lg border border-[var(--border)] bg-[var(--background)] text-[var(--text-primary)]">
            </div>

            <div class="mb-4">
              <label class="block text-sm font-medium text-[var(--text-secondary)] mb-2">Working Days</label>
              <div class="grid grid-cols-4 gap-2">
                @for (day of weekDays; track day.bit) {
                  <label class="flex items-center gap-2 text-sm cursor-pointer p-2 rounded hover:bg-gray-50 border border-[var(--border)]">
                    <input type="checkbox" [checked]="isDayChecked(day.bit)" (change)="toggleDay(day.bit)"
                           class="rounded border-[var(--border)] text-emerald-600 focus:ring-emerald-500">
                    <span class="text-[var(--text-primary)]">{{ day.label }}</span>
                  </label>
                }
              </div>
            </div>

            <div class="grid grid-cols-2 gap-4 mb-6">
              <div>
                <label class="block text-sm font-medium text-[var(--text-secondary)] mb-1">Daily Hours</label>
                <input type="number" [(ngModel)]="employeeForm.dailyWorkingHours" name="dailyWorkingHours" step="0.5"
                  class="w-full px-3 py-2 rounded-lg border border-[var(--border)] bg-[var(--background)] text-[var(--text-primary)]"
                  placeholder="8 (default)">
              </div>
              <div>
                <label class="block text-sm font-medium text-[var(--text-secondary)] mb-1">Country</label>
                <select [(ngModel)]="employeeForm.countryCode" name="countryCode"
                  class="w-full px-3 py-2 rounded-lg border border-[var(--border)] bg-[var(--background)] text-[var(--text-primary)]">
                  @for (country of countries; track country.code) {
                    <option [value]="country.code">{{ country.name }}</option>
                  }
                </select>
              </div>
            </div>

            @if (error()) {
              <div class="mb-4 p-3 bg-red-100 text-red-700 rounded-lg text-sm">{{ error() }}</div>
            }

            <div class="flex gap-3 justify-end">
              <button type="button" (click)="closeModal()" class="btn bg-gray-200 text-gray-700 hover:bg-gray-300">
                Cancel
              </button>
              <button type="submit" [disabled]="loading()" class="btn btn-primary">
                {{ loading() ? 'Saving...' : (isEditing() ? 'Update' : 'Create') }}
              </button>
            </div>
          </form>
        </div>
      </div>
    }

    <!-- Balances Modal -->
    @if (showBalanceModal()) {
      <div class="fixed inset-0 bg-black/50 flex items-center justify-center z-50" (click)="closeBalanceModal()">
        <div class="bg-[var(--surface)] rounded-xl shadow-2xl p-6 w-full max-w-2xl mx-4" (click)="$event.stopPropagation()">
          <h2 class="text-xl font-bold text-[var(--text-primary)] mb-4">Manage Balances: {{ selectedEmployeeForBalance()?.fullName }}</h2>
          
          <div class="overflow-x-auto">
            <table class="w-full text-left text-sm">
              <thead>
                <tr class="text-[var(--text-secondary)] border-b border-[var(--border)]">
                  <th class="pb-2 font-medium">Leave Type</th>
                  <th class="pb-2 font-medium">Total Days</th>
                  <th class="pb-2 font-medium">Used</th>
                  <th class="pb-2 font-medium">Remaining</th>
                  <th class="pb-2 font-medium">Actions</th>
                </tr>
              </thead>
              <tbody>
                @for (bal of employeeBalances(); track bal.id) {
                  <tr class="border-b border-[var(--border)] last:border-0 hover:bg-[var(--background)]">
                    <td class="py-3">
                      <div class="font-medium" [style.color]="bal.leaveTypeColor">{{ bal.leaveTypeName }}</div>
                      @if (bal.note) {
                        <div class="text-xs text-gray-500 mt-1 italic">{{ bal.note }}</div>
                      }
                    </td>
                    <td class="py-3">
                      <div class="flex items-center gap-2">
                        <span>{{ bal.totalDays }}</span>
                      </div>
                    </td>
                    <td class="py-3">{{ bal.usedDays }}</td>
                    <td class="py-3 font-bold">{{ bal.remainingDays }}</td>
                    <td class="py-3">
                      <button class="text-emerald-600 hover:text-emerald-800 text-xs font-medium" 
                              (click)="updateBalance(bal)">
                        Update
                      </button>
                    </td>
                  </tr>
                }
              </tbody>
            </table>
            
            @if (employeeBalances().length === 0) {
              <div class="text-center py-4 text-[var(--text-secondary)]">No balances found.</div>
            }
          </div>

          <!-- Add Balance Section -->
          <div class="mt-4 border-t border-[var(--border)] pt-4">
             @if (!showAddBalanceForm()) {
               <button class="btn btn-sm btn-outline" (click)="showAddBalanceForm.set(true)">+ Add Balance</button>
             } @else {
               <div class="bg-[var(--background)] p-4 rounded-lg">
                 <h4 class="font-bold text-sm mb-2">Assign Leave Type</h4>
                 <div class="flex gap-2 items-end">
                   <div class="flex-1">
                     <label class="text-xs font-medium block mb-1">Leave Type</label>
                     <select [(ngModel)]="newBalanceTypeId" class="input text-sm">
                       <option value="">Select Type</option>
                       @for (type of leaveTypes(); track type.id) {
                         <option [value]="type.id">{{ type.name }}</option>
                       }
                     </select>
                   </div>
                   <div class="w-24">
                     <label class="text-xs font-medium block mb-1">Days</label>
                     <input type="number" [(ngModel)]="newBalanceDays" class="input text-sm" placeholder="Days">
                   </div>
                   <div class="flex-1">
                     <label class="text-xs font-medium block mb-1">Note (Optional)</label>
                     <input type="text" [(ngModel)]="newBalanceNote" class="input text-sm" placeholder="Reason (e.g. Bonus)">
                   </div>
                   <button class="btn btn-sm btn-primary" (click)="addBalance()" [disabled]="!newBalanceTypeId()">Save</button>
                   <button class="btn btn-sm btn-secondary" (click)="showAddBalanceForm.set(false)">Cancel</button>
                 </div>
               </div>
             }
          </div>

          <div class="mt-6 flex justify-end">
            <button class="btn btn-secondary" (click)="closeBalanceModal()">Close</button>
          </div>
        </div>
      </div>
    }
  `
})
export class EmployeesComponent implements OnInit {
  employees = signal<Employee[]>([]);
  revealedSalaries = signal<Record<string, string>>({});
  showModal = signal(false);
  showBalanceModal = signal(false);
  employeeBalances = signal<any[]>([]);
  selectedEmployeeForBalance = signal<Employee | null>(null);
  
  // Add Balance Logic
  leaveTypes = signal<LeaveType[]>([]);
  showAddBalanceForm = signal(false);
  newBalanceTypeId = signal('');
  newBalanceDays = signal(0);
  newBalanceNote = signal('');
  
  loading = signal(false);
  error = signal('');

  countries = [
    { code: 'IN', name: 'India' },
    { code: 'AE', name: 'United Arab Emirates' },
    { code: 'PK', name: 'Pakistan' },
    { code: 'JO', name: 'Jordan' },
    { code: 'EG', name: 'Egypt' }
  ];

  isEditing = signal(false);
  managers = computed(() => this.employees().filter(e => 
    e.role === UserRole.Approver || 
    e.role === UserRole.HRAdmin || 
    e.role === UserRole.SuperAdmin
  ));

  weekDays = [
    { bit: 1, label: 'Mon' },
    { bit: 2, label: 'Tue' },
    { bit: 4, label: 'Wed' },
    { bit: 8, label: 'Thu' },
    { bit: 16, label: 'Fri' },
    { bit: 32, label: 'Sat' },
    { bit: 64, label: 'Sun' }
  ];

  employeeForm: {
    id?: string;
    email: string;
    password?: string;
    firstName: string;
    lastName: string;
    gender: number;
    role: number;
    department: string;
    managerId?: string;
    dailyWorkingHours?: number;
    countryCode: string;
    workingDays?: number;
  } = {
    email: '',
    firstName: '',
    lastName: '',
    gender: 0,
    role: 0,
    department: '',
    countryCode: 'AE',
    workingDays: 31 // Mon-Fri default
  };

  constructor(
    private employeeService: EmployeeService,
    private leaveService: LeaveService,
    public authService: AuthService
  ) {}

  ngOnInit(): void {
    this.loadEmployees();
  }

  loadEmployees(): void {
    this.employeeService.getAll().subscribe({
      next: (data) => this.employees.set(data)
    });
  }

  toggleStatus(emp: Employee): void {
    if (!confirm(`Are you sure you want to ${emp.isActive ? 'disable' : 'activate'} ${emp.fullName}?`)) return;

    this.employeeService.update(emp.id, { isActive: !emp.isActive }).subscribe({
      next: () => {
        this.loadEmployees();
      },
      error: (err) => alert('Failed to update status')
    });
  }


  openAddModal(): void {
    this.isEditing.set(false);
    this.showModal.set(true);
    this.error.set('');
    this.resetForm();
  }

  openEditModal(emp: Employee): void {
    this.isEditing.set(true);
    this.employeeForm = {
      id: emp.id,
      email: emp.email,
      firstName: emp.firstName,
      lastName: emp.lastName,
      gender: emp.gender,
      role: emp.role,
      department: emp.department || '',
      managerId: emp.managerId,
      dailyWorkingHours: emp.dailyWorkingHours,
      countryCode: emp.countryCode,
      workingDays: emp.workingDays ?? 31
    };
    this.showModal.set(true);
    this.error.set('');
  }

  closeModal(): void {
    this.showModal.set(false);
  }

  openBalanceModal(emp: Employee): void {
    this.selectedEmployeeForBalance.set(emp);
    this.showBalanceModal.set(true);
    this.loading.set(true);
    this.showAddBalanceForm.set(false);
    
    // Load Balances
    this.employeeService.getBalances(emp.id).subscribe({
      next: (data) => {
        this.employeeBalances.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load balances', err);
        this.loading.set(false);
      }
    });

    // Load Leave Types if needed
    if (this.leaveTypes().length === 0) {
       this.leaveService.getLeaveTypes().subscribe(types => this.leaveTypes.set(types));
    }
  }

  closeBalanceModal(): void {
    this.showBalanceModal.set(false);
    this.selectedEmployeeForBalance.set(null);
  }

  updateBalance(bal: any): void {
    const newDays = prompt(`Enter new TOTAL DAYS for ${bal.leaveTypeName}:`, bal.totalDays);
    if (newDays !== null) {
      const days = parseFloat(newDays);
      if (!isNaN(days)) {
        const emp = this.selectedEmployeeForBalance()!;
        const hours = days * (emp.dailyWorkingHours || 8); 
        this.employeeService.updateLeaveBalance(emp.id, bal.leaveTypeId, hours).subscribe({
          next: () => {
             // Refresh balances
             this.openBalanceModal(emp);
          },
          error: (err) => alert('Failed to update: ' + err.message)
        });
      }
    }
  }

  addBalance(): void {
    if (!this.newBalanceTypeId()) return;
    
    const emp = this.selectedEmployeeForBalance()!;
    const days = this.newBalanceDays();
    const note = this.newBalanceNote();
    const hours = days * (emp.dailyWorkingHours || 8);

    this.employeeService.updateLeaveBalance(emp.id, this.newBalanceTypeId(), hours, note).subscribe({
      next: () => {
        this.showAddBalanceForm.set(false);
        this.newBalanceTypeId.set('');
        this.newBalanceDays.set(0);
        this.newBalanceNote.set('');
        this.openBalanceModal(emp);
      },
      error: (err) => alert('Failed to add balance: ' + err.message)
    });
  }

  resetForm(): void {
    this.employeeForm = {
      email: '',
      password: '',
      firstName: '',
      lastName: '',
      gender: 0,
      role: 0,
      department: '',
      countryCode: 'AE',
      workingDays: 31
    };
  }

  isDayChecked(bit: number): boolean {
      return ((this.employeeForm.workingDays || 0) & bit) !== 0;
  }

  toggleDay(bit: number): void {
      const current = this.employeeForm.workingDays || 0;
      if ((current & bit) !== 0) {
          this.employeeForm.workingDays = current & ~bit;
      } else {
          this.employeeForm.workingDays = current | bit;
      }
  }

  saveEmployee(): void {
    this.loading.set(true);
    this.error.set('');

    const operation = this.isEditing() 
      ? this.employeeService.update(this.employeeForm.id!, this.employeeForm)
      : this.employeeService.create(this.employeeForm as any);

    operation.subscribe({
      next: () => {
        this.loading.set(false);
        this.closeModal();
        this.loadEmployees();
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err.error?.message || 'Failed to save employee');
      }
    });
  }

  getRoleName(role: UserRole): string {
    switch (role) {
      case UserRole.SuperAdmin: return 'Super Admin';
      case UserRole.HRAdmin: return 'HR Admin';
      case UserRole.Approver: return 'Manager';
      default: return 'Employee';
    }
  }

  getRoleBadge(role: UserRole): string {
    switch (role) {
      case UserRole.SuperAdmin: return 'badge bg-purple-100 text-purple-700';
      case UserRole.HRAdmin: return 'badge bg-blue-100 text-blue-700';
      case UserRole.Approver: return 'badge bg-amber-100 text-amber-700';
      default: return 'badge bg-gray-100 text-gray-700';
    }
  }

  getSalaryField(emp: Employee): SensitiveField {
    const revealed = this.revealedSalaries()[emp.id];
    if (revealed) {
      return { maskedValue: '********', value: revealed, isRevealed: true, canReveal: true };
    }
    return emp.salary || { maskedValue: '********', isRevealed: false, canReveal: true };
  }

  revealSalary(empId: string): void {
    this.employeeService.revealSensitiveField(empId, 'salary').subscribe({
      next: (result) => {
        this.revealedSalaries.update(s => ({ ...s, [empId]: result.value }));
      }
    });
  }

  hideSalary(empId: string): void {
    this.revealedSalaries.update(s => {
      const { [empId]: _, ...rest } = s;
      return rest;
    });
  }
}
