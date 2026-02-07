# LeaveFlow - Enterprise Leave Management System

LeaveFlow is a modern, enterprise-grade Leave Management System (LMS) designed for multi-tenant SaaS environments. It provides a seamless experience for employees to request leave and for HR/Admins to manage accruals, policies, and working schedules.

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![Status](https://img.shields.io/badge/status-Active-success.svg)
![Backend](https://img.shields.io/badge/backend-.NET%208-purple)
![Frontend](https://img.shields.io/badge/frontend-Angular%2018-red)
![Database](https://img.shields.io/badge/database-PostgreSQL-blue)

## 🚀 Key Features

### 🏢 Multi-tenancy
- **Secure Isolation**: Built-in Row-Level Security (RLS) using EF Core Global Query Filters.
- **Data Privacy**: Each tenant's data is logically isolated, preventing accidental leaks.

### 📅 Smart Leave Management
- **Global & Country-Specific Holidays**: Automatically syncs public holidays based on employee location.
- **Custom Work Schedules**: Configure specific working days (e.g., Mon-Fri vs Sun-Thu) per employee.
- **Accurate Calculations**: Automatically excludes weekends and holidays from leave duration.
- **Leave Types**: Configurable policies for Annual, Sick, Maternity, Paternity, etc.

### 👥 User Roles & Permissions
- **Super Admin**: Full system control, tenant management.
- **HR Admin**: Manage employees, leave policies, and approvals.
- **Manager**: Approve/reject leave requests for direct reports.
- **Employee**: Request leave, view balances, and track history.

### 🔒 Security & Compliance
- **Encryption**: Sensitive data (Salary, Pivot Data) is encrypted at rest using AES-256.
- **Audit Logs**: Comprehensive tracking of critical actions for compliance.
- **JWT Authentication**: Secure, stateless authentication with role-based authorization.

### 📊 Dashboard & Reporting
- **Interactive Dashboard**: Real-time overview of leave balances, upcoming holidays, and team availability.
- **Visual Calendar**: Easy-to-read view of team schedules.

## 🛠️ Tech Stack

### Backend
- **Framework**: .NET 8 (ASP.NET Core Web API)
- **Database**: PostgreSQL (via Npgsql)
- **ORM**: Entity Framework Core 8
- **Authentication**: JWT (JSON Web Tokens)
- **Encryption**: AES-256 Custom Encryption Service
- **Documentation**: Swagger / OpenAPI

### Frontend
- **Framework**: Angular 18 (Standalone Components)
- **Styling**: TailwindCSS
- **State Management**: Angular Signals
- **HTTP Client**: Angular HttpClient

## ⚙️ Setup & Installation

### Prerequisites
- .NET 8 SDK
- Node.js (v18+)
- PostgreSQL Database

### 1. Database Setup
Ensure PostgreSQL is running. Create a database named `leaveflow` (or update `appsettings.json` connection string).

### 2. Backend Setup
```bash
cd backend/LeaveFlow.Api
# Update connection string in appsettings.json if needed
dotnet restore
dotnet run
```
*The application will automatically apply migrations and seed the database with a default Admin user.*

**Default Admin Credentials:**
- **Email:** `admin@leaveflow.com`
- **Password:** `Admin@123`

### 3. Frontend Setup
```bash
cd frontend
npm install
npm start
```
Navigate to `http://localhost:4200` to access the application.

### 4. Managing Tenants (Create New Tenant)
Since there is no "Sign Up" UI yet, you can create a new tenant via the API.

**Endpoint:** `POST /api/Auth/register-tenant`
**Body:**
```json
{
  "companyName": "Acme Corp",
  "adminEmail": "admin@acme.com",
  "adminPassword": "Password123!",
  "adminFirstName": "John",
  "adminLastName": "Doe"
}
```
*This will create a new tenant, an admin user, and default leave types. You can then log in with these credentials to access the new tenant's dashboard.*

## 📚 API Documentation

The API is fully documented using Swagger. Once the backend is running, visit:
`https://localhost:7153/swagger` (port may vary)

## 🏗️ Architecture Highlights

### Tenant Isolation
We use a **Discriminator Column** strategy with **Global Query Filters**.
- All tenant-specific entities implement `ITenantEntity`.
- `LeaveFlowDbContext` automatically filters all queries by `TenantId`.
- `CurrentTenantService` resolves the tenant from the user's JWT claims.

### Security
- **Sensitive Data**: Fields like `Salary`, `PassportNumber`, and `BankDetails` are never stored in plain text.
- **File Uploads**: Supports secure document storage for medical certificates and ID proofs.

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

Distributed under the MIT License. See `LICENSE` for more information.
