using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wheelzy.Application.Common.Interfaces;

namespace Wheelzy.Persistence.Common;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddDbContext<ApplicationDbContext>(BuildDbContextOptions);
        services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
        return services;
    }

    private static void BuildDbContextOptions(IServiceProvider serviceProvider, DbContextOptionsBuilder options)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var connString = configuration.GetConnectionString("DefaultConnection");
        options.UseSqlServer(connString, builder => builder.MigrationsAssembly("Wheelzy.Persistence"));
    }
}
