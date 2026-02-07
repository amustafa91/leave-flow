using System;

namespace LeaveFlow.Core.Interfaces;

public interface ICurrentTenantService
{
    Guid? TenantId { get; }
    void SetTenant(Guid tenantId);
}
