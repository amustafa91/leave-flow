using System;

namespace LeaveFlow.Core.Interfaces;

public interface ITenantEntity
{
    Guid TenantId { get; set; }
}
