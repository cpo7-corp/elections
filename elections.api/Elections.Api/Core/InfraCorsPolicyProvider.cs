using Microsoft.AspNetCore.Cors.Infrastructure;

namespace Elections.Api.Core;

public class InfraCorsPolicyProvider : ICorsPolicyProvider
{
    private readonly CorsPolicy defaultPolicy;
    private readonly CorsPolicy qaPolicy;
    private readonly ConfigSvc configSvc;
    private readonly ILogger<InfraCorsPolicyProvider> log;

    public InfraCorsPolicyProvider(ConfigSvc configSvc, ILogger<InfraCorsPolicyProvider> log)
    {
        this.configSvc = configSvc;
        this.log = log;
        this.log.LogInformation("InfraCorsPolicyProvider initialized");
        defaultPolicy = BuildDefaultPolicy();
        qaPolicy = BuildQAPolicy();
    }


    public Task<CorsPolicy?> GetPolicyAsync(HttpContext context, string? policyName)
    {
        Console.WriteLine($"GetPolicyAsync for {policyName}");
        this.log.LogInformation("GetPolicyAsync called with policyName: {policyName}", policyName);
        
        if (string.Equals(policyName, "qa", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<CorsPolicy?>(qaPolicy);
        }

        return Task.FromResult<CorsPolicy?>(defaultPolicy);
    }


    private CorsPolicy BuildDefaultPolicy()
    {
        var arr = configSvc.Get<string[]>("corsDomains");

        return new CorsPolicyBuilder()
            .WithOrigins(arr)
            .SetIsOriginAllowedToAllowWildcardSubdomains()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .Build();
    }

    private CorsPolicy BuildQAPolicy()
    {
        return new CorsPolicyBuilder()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true)
            .Build();
    }
}
