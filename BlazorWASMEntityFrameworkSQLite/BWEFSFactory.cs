using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BlazorWASMEntityFrameworkSQLite;

public class BWEFSFactory<TContext> where TContext : DbContext
{
    private readonly string filename;
    private readonly IDbContextFactory<TContext> dbContextFactory;
    private readonly IDbStorageProvider dbStorageProvider;
    private readonly bool useMigrations;
    
    private bool firstConnection = true;

    public BWEFSFactory(string filename, IDbContextFactory<TContext> dbContextFactory, IDbStorageProvider dbStorageProvider, bool useMigrations)
    {
        this.filename = filename;
        this.dbContextFactory = dbContextFactory;
        this.dbStorageProvider = dbStorageProvider;
        this.useMigrations = useMigrations;
    }

    /// <summary>
    /// Creates a new db context and hooks up events such as saving. Handles some other logic such as first time connecting.
    /// </summary>
    /// <returns>A db context.</returns>
    public async Task<TContext> CreateDbContextAsync()
    {
        TContext context;

        if (firstConnection)
        {
            context = await dbContextFactory.CreateDbContextAsync();
            
            await dbStorageProvider.InitializeAsync(filename);

            await MigrateOrCreateAsync(context);
            
            firstConnection = false;
        }
        else
        {
            context = await dbContextFactory.CreateDbContextAsync();
        }

        // This cannot be awaited and must be a fire and forget.
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        context.SavedChanges += (o, e) => SaveDatabase(context, e);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        
        return context;
    }

    /// <summary>
    /// Manually restore a database from a byte array.
    /// </summary>
    /// <param name="arrayBuffer">byte[] of the database file to restore.</param>
    /// <returns>Whether the restore was successful or not.</returns>
    public async Task<bool> ManualRestoreAsync(byte[] arrayBuffer)
    {
        SqliteConnection.ClearAllPools();
        bool result = await dbStorageProvider.RestoreAsync(filename, arrayBuffer);
        TContext context = await CreateDbContextAsync();
        await MigrateOrCreateAsync(context);

        return result;
    }

    /// <summary>
    /// Get the url to download a copy of the database file.
    /// </summary>
    /// <returns>The url to the database or null if it can't be found.</returns>
    public async Task<string?> GetDownloadUrlAsync()
    {
        return await dbStorageProvider.GetDownloadURLAsync(filename);
    }

    private async Task<bool> SaveDatabase(TContext context, SavedChangesEventArgs e)
    {
        await context.Database.CloseConnectionAsync();
        SqliteConnection.ClearAllPools();
        return await dbStorageProvider.SaveAsync(filename);
    }

    private async Task MigrateOrCreateAsync(TContext context)
    {
        if (useMigrations)
        {
            await context.Database.MigrateAsync();
        }
        else
        {
            await context.Database.EnsureCreatedAsync();
        }
    }
}