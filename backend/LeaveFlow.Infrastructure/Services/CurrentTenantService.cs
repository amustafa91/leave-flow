using System;
using System.Security.Claims;
using LeaveFlow.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace LeaveFlow.Infrastructure.Services;

public class CurrentTenantService : ICurrentTenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private Guid? _tenantId;

    public CurrentTenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? TenantId
    {
        get
        {
            if (_tenantId.HasValue)
            {
                return _tenantId.Value;
            }

            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !user.Identity.IsAuthenticated)
            {
                return null;
            }

            var tenantClaim = user.FindFirst("tenant_id")?.Value;
            if (Guid.TryParse(tenantClaim, out var tenantId))
            {
                _tenantId = tenantId;
                return _tenantId;
            }

            return null;
        }
    }

    public void SetTenant(Guid tenantId)
    {
        _tenantId = tenantId;
    }
}
