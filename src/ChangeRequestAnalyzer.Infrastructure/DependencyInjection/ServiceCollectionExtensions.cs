using ChangeRequestAnalyzer.Core.Interfaces;
using ChangeRequestAnalyzer.Infrastructure.Data;
using ChangeRequestAnalyzer.Infrastructure.Repositories;
using ChangeRequestAnalyzer.Infrastructure.Services;
using ChangeRequestAnalyzer.Infrastructure.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeRequestAnalyzer.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AzureFoundryOptions>(configuration.GetSection("AzureFoundry"));
            services.AddSingleton(resolver => resolver.GetRequiredService<Microsoft.Extensions.Options.IOptions<AzureFoundryOptions>>().Value);
            services.Configure<AzureDevOpsOptions>(configuration.GetSection("AzureDevOps"));
            services.AddSingleton(resolver => resolver.GetRequiredService<Microsoft.Extensions.Options.IOptions<AzureDevOpsOptions>>().Value);

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IChangeRequestRepository, ChangeRequestRepository>();
            services.AddScoped<IChangeRequestAnalyzer, ChangeRequestAnalysisService>();
            services.AddScoped<IAzureFoundryOrchestrationService, AzureFoundryOrchestrationService>();
            services.AddHttpClient<IAzureDevOpsService, AzureDevOpsService>();

            return services;
        }
    }
}
