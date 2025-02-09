using Microsoft.JSInterop;

namespace BlazorWASMEntityFrameworkSQLite.DbStorageProviders;

public class CacheStorage : IAsyncDisposable, IDbStorageProvider
{
    private readonly Lazy<Task<IJSObjectReference>> moduleTask;

    public CacheStorage(IJSRuntime jsRuntime)
    {
        moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/BlazorWASMEntityFrameworkSQLite/BlazorWASMEntityFrameworkSQLite.js").AsTask());
    }

    public async ValueTask DisposeAsync()
    {
        if (moduleTask.IsValueCreated)
        {
            var module = await moduleTask.Value;
            await module.DisposeAsync();
        }
    }

    public async Task<bool> InitializeAsync(string filename)
    {
        var module = await moduleTask.Value;
        return await module.InvokeAsync<bool>("cacheInitialize", filename);
    }

    public async Task<bool> SaveAsync(string filename)
    {
        var module = await moduleTask.Value;
        return await module.InvokeAsync<bool>("cacheSave", filename);
    }

    public async Task<bool> RestoreAsync(string filename, byte[] backupFile)
    {
        var module = await moduleTask.Value;
        return await module.InvokeAsync<bool>("cacheRestore", filename, backupFile);
    }

    public async Task<string?> GetDownloadURLAsync(string filename)
    {
        var module = await moduleTask.Value;
        return await module.InvokeAsync<string?>("cacheGetDownloadURL", filename);
    }
}