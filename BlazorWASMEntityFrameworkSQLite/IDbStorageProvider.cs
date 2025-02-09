namespace BlazorWASMEntityFrameworkSQLite;

public interface IDbStorageProvider
{
    Task<bool> InitializeAsync(string filename);
    
    Task<bool> SaveAsync(string filename);

    Task<bool> RestoreAsync(string filename, byte[] backupFile);

    Task<string?> GetDownloadURLAsync(string filename);
}