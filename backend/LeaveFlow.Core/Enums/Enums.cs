namespace LeaveFlow.Core.Enums;

public enum UserRole
{
    Employee = 0,
    Approver = 1,
    HRAdmin = 2,
    SuperAdmin = 3
}

public enum LeaveStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Cancelled = 3
}

public enum Gender
{
    Male = 0,
    Female = 1,
    Other = 2
}

public enum ApplicableGender
{
    All = 0,      // Available to everyone
    Male = 1,     // Paternity leave
    Female = 2    // Maternity leave
}
