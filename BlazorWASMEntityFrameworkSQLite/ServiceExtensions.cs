using BlazorWASMEntityFrameworkSQLite.DbStorageProviders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace BlazorWASMEntityFrameworkSQLite;

public static class ServiceExtensions
{
    public static IServiceCollection AddBWEFSDbContextFactory<TContext>(this IServiceCollection services,
        string? filename = null,
        Action<SqliteDbContextOptionsBuilder>? optionsAction = null,
        StorageProviderType storageProviderType = StorageProviderType.Cache,
        bool useMigrations = true)
        where TContext : DbContext
    {

        switch (storageProviderType)
        {
            case StorageProviderType.Cache:
            {
                services.AddSingleton<IDbStorageProvider, CacheStorage>();
                break;
            }
            case StorageProviderType.IndexDb:
            {
                throw new NotImplementedException("IndexDb is not implemented yet.");
            }
            case StorageProviderType.LocalStorage:
            {
                throw new NotImplementedException("LocalStorage is not implemented yet.");
            }
            case StorageProviderType.Custom:
            {
                // Their own service has been implemented.
                break;
            }
            default:
            {
                throw new NotImplementedException("Unknown StorageProviderType.");
            }
        }
        
        filename ??= $"{typeof(TContext).Name}.db";
        
        services.AddDbContextFactory<TContext>(options => options.UseSqlite($"Data Source={filename}", optionsAction));

        services.AddSingleton<BWEFSFactory<TContext>>(provider =>
        {
            IDbContextFactory<TContext> factory = provider.GetRequiredService<IDbContextFactory<TContext>>();
            IDbStorageProvider storageProvider = provider.GetRequiredService<IDbStorageProvider>();
            return new BWEFSFactory<TContext>(filename, factory, storageProvider, useMigrations);
        });
        
        return services;
    }
}