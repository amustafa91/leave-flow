// User roles
export enum UserRole {
  Employee = 0,
  Approver = 1,
  HRAdmin = 2,
  SuperAdmin = 3
}

// Gender
export enum Gender {
  Male = 0,
  Female = 1,
  Other = 2
}

// Leave status
export enum LeaveStatus {
  Pending = 0,
  Approved = 1,
  Rejected = 2,
  Cancelled = 3
}

// Notification
export interface Notification {
  id: string;
  userId: string;
  title: string;
  message: string;
  type: NotificationType;
  isRead: boolean;
  createdAt: string;
  relatedEntityId?: string;
}

export enum NotificationType {
  Info = 0,
  Success = 1,
  Warning = 2,
  Error = 3
}



// Sensitive field with show/hide support
export interface SensitiveField {
  maskedValue: string;
  value?: string;
  isRevealed: boolean;
  canReveal: boolean;
}

// Employee
export interface Employee {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  role: UserRole;
  gender: Gender;
  department?: string;
  managerId?: string;
  managerName?: string;
  isActive: boolean;
  dailyWorkingHours?: number;
  countryCode: string;
  workingDays?: number;
  salary: SensitiveField;
  passportNumber: SensitiveField;
  phone: SensitiveField;
  address: SensitiveField;
  emergencyContact: SensitiveField;
  nationalId: SensitiveField;
  bankAccount: SensitiveField;
}

// Leave type
export interface LeaveType {
  id: string;
  name: string;
  description?: string;
  defaultHoursPerYear: number;
  defaultDaysPerYear: number;
  requiresApproval: boolean;
  requiresDocument: boolean;
  colorCode: string;
}

// Leave request
export interface LeaveRequest {
  id: string;
  employeeId: string;
  employeeName: string;
  leaveTypeId: string;
  leaveTypeName: string;
  leaveTypeColor: string;
  startDate: Date;
  endDate: Date;
  leaveHours: number;
  isFullDay: boolean;
  totalDays: number;
  status: LeaveStatus;
  reason?: string;
  approverName?: string;
  approvedAt?: Date;
  approverComments?: string;
  attachmentPath?: string;
  attachmentFileName?: string;
  createdAt: Date;
}

// Leave balance (hours-based)
export interface LeaveBalance {
  id: string;
  leaveTypeId: string;
  leaveTypeName: string;
  leaveTypeColor: string;
  year: number;
  totalHours: number;
  usedHours: number;
  pendingHours: number;
  remainingHours: number;
  totalDays: number;
  usedDays: number;
  remainingDays: number;
  accruedHours?: number;
  accruedDays?: number;
}

// Auth
export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  expiresAt: Date;
  user: Employee;
}

// Create leave request (hours-based)
export interface CreateLeaveRequest {
  leaveTypeId: string;
  startDate: Date;
  endDate: Date;
  isFullDay: boolean;
  leaveHours: number;
  reason?: string;
  personalNotes?: string;
}

// Approve request
export interface ApproveLeaveRequest {
  approve: boolean;
  comments?: string;
}

// Settings
export interface CompanySettings {
  id: string;
  companyName: string;
  defaultDailyWorkingHours: number;
  minLeaveHours: number;
  sunday: boolean;
  monday: boolean;
  tuesday: boolean;
  wednesday: boolean;
  thursday: boolean;
  friday: boolean;
  saturday: boolean;
  defaultCountryCode: string;
}

// Public holiday
export interface PublicHoliday {
  id: string;
  date: Date;
  name: string;
  localName?: string;
  countryCode: string;
  isCustom: boolean;
}

export interface CountryWorkday {
  countryCode: string;
  sunday: boolean;
  monday: boolean;
  tuesday: boolean;
  wednesday: boolean;
  thursday: boolean;
  friday: boolean;
  saturday: boolean;
}
