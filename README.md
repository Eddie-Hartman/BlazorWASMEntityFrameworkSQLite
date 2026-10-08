# BlazorWASMEntityFrameworkSQLite

Use Entity Framework Core with a local SQLite database in a Blazor WebAssembly app. The library wires up dependency injection, restores the database on first access, applies EF schema setup, and saves database snapshots to browser storage. It is useful for local data and offline-capable apps without a database server; it does not provide server synchronization.

SQLite runs in the [Emscripten in-memory filesystem](https://emscripten.org/docs/api_reference/Filesystem-API.html), accessed through [Blazor JavaScript interop](https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/). The default storage provider copies that database to the browser's Cache Storage.

## Package and requirements

Install the published [NuGet package, version 1.1.0](https://www.nuget.org/packages/BlazorWASMEntityFrameworkSQLite/1.1.0):

```sh
dotnet add package BlazorWASMEntityFrameworkSQLite --version 1.1.0
```

| Package target | Minimum EF Core / SQLite provider version |
| --- | --- |
| .NET 8 (`net8.0`) | 8.0.19 |
| .NET 9 (`net9.0`) | 9.0.1 |

Keep EF Core packages on the matching major version. Both targets depend on `SQLitePCLRaw.bundle_e_sqlite3` 3.0.1. The example and quickstart use .NET 9; newer target frameworks have not been validated here.

Native SQLite requires `<WasmBuildNative>true</WasmBuildNative>` and the [WebAssembly build tools](https://learn.microsoft.com/en-us/aspnet/core/blazor/webassembly-build-tools-and-aot?view=aspnetcore-9.0). With a .NET 9 SDK targeting .NET 9, install `wasm-tools`. Workloads are SDK-specific; for example, a .NET 9 SDK targeting .NET 8 needs `wasm-tools-net8`. Linux also needs Python on `PATH`.

Native builds can emit `WASM0001` warnings for SQLite varargs APIs, which cannot be called on WebAssembly. The quickstart below does not invoke those APIs.

## Quickstart

With a .NET 9 SDK selected:

```sh
dotnet workload install wasm-tools
dotnet new blazorwasm -n BrowserDb --framework net9.0
cd BrowserDb
dotnet add package BlazorWASMEntityFrameworkSQLite --version 1.1.0
```

Add this property to the existing `<PropertyGroup>` in `BrowserDb.csproj`:

```xml
<WasmBuildNative>true</WasmBuildNative>
```

Create `Data/AppDbContext.cs`:

```csharp
using Microsoft.EntityFrameworkCore;

namespace BrowserDb.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Note> Notes => Set<Note>();
}

public class Note
{
    public int Id { get; set; }
    public string Text { get; set; } = "";
}
```

In `Program.cs`, add these imports and register the factory **before** `builder.Build().RunAsync()`:

```csharp
using BlazorWASMEntityFrameworkSQLite;
using BrowserDb.Data;

// Alongside the other builder.Services registrations:
builder.Services.AddBWEFSDbContextFactory<AppDbContext>(
    filename: "notes.db", useMigrations: false);
```

Create `Pages/Database.razor`, then visit `/database`:

```razor
@page "/database"
@using BlazorWASMEntityFrameworkSQLite
@using BrowserDb.Data
@using Microsoft.EntityFrameworkCore
@inject BWEFSFactory<AppDbContext> Factory

<h1>Local notes</h1>
<button @onclick="AddNote" disabled="@busy">Add note</button>
@foreach (var note in notes)
{
    <p>@note.Text</p>
}

@code {
    private List<Note> notes = new();
    private bool busy = true;

    protected override async Task OnInitializedAsync()
    {
        await using var db = await Factory.CreateDbContextAsync();
        notes = await db.Notes.ToListAsync();
        busy = false;
    }

    private async Task AddNote()
    {
        busy = true;
        await using var db = await Factory.CreateDbContextAsync();
        db.Notes.Add(new Note { Text = $"Note {notes.Count + 1}" });
        await db.SaveChangesAsync();
        notes = await db.Notes.ToListAsync();
        busy = false;
    }
}
```

```sh
dotnet run
```

Use `BWEFSFactory<TContext>.CreateDbContextAsync()` for application access: it restores storage, initializes the schema, and attaches the persistence handler. Using the underlying `IDbContextFactory<TContext>` directly bypasses those steps. No manual JavaScript import is needed.

## Schema and migrations

The quickstart sets `useMigrations: false`, so the first factory call uses `EnsureCreatedAsync()`. This creates a new schema but does **not** update an existing database after model changes. [Do not mix `EnsureCreated` with migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/ensure-created).

For an evolving schema, generate EF migrations and leave `useMigrations` at its default `true`. The first factory call restores the cached database, then runs `MigrateAsync()`; manual restore also runs schema setup. See [PWAExampleData](PWAExampleData), including its [design-time factory](PWAExampleData/Data/PWAExampleDbContextFactory.cs) and [checked-in migrations](PWAExampleData/Migrations). Creating or migrating the schema alone does not save a snapshot; a subsequent EF `SaveChanges` triggers persistence.

## Browser persistence and limits

- The implemented provider is `StorageProviderType.Cache` (the default). `IndexDb` and `LocalStorage` currently throw `NotImplementedException`; for `Custom`, implement and register `IDbStorageProvider`.
- On first access, the factory restores the database from Cache Storage. After EF `SaveChanges` / `SaveChangesAsync`, it copies the **whole database file** back to the cache. Raw SQL or other writes that bypass EF's `SavedChanges` event do not trigger this copy.
- The cache copy runs asynchronously without being awaited. Completing `SaveChangesAsync()` does **not** guarantee that browser persistence has finished; immediately closing or reloading the page can lose the latest changes. Snapshot cost grows with database size, and concurrent writes or multiple tabs are not coordinated.
- Data belongs to the browser profile and origin (scheme, host, and port), with cache entries keyed by database filename. It is not shared between devices. Browser quotas, eviction, clearing site data, or ending a private session can remove it. The library does not request persistent storage. Keep backups for important data; see [browser storage limits](https://developer.mozilla.org/en-US/docs/Web/API/Storage_API/Storage_quotas_and_eviction_criteria).
- Cache Storage requires a secure context: HTTPS in deployment, or localhost/loopback for development. Use a browser that supports [Blazor WebAssembly](https://learn.microsoft.com/en-us/aspnet/core/blazor/supported-platforms?view=aspnetcore-9.0) and [Cache Storage](https://developer.mozilla.org/en-US/docs/Web/API/CacheStorage); private mode or browser policy may restrict storage. Test your target browsers rather than assuming a cross-browser guarantee.

After initializing the factory, `GetDownloadUrlAsync()` returns a URL for the **cached snapshot** (or `null` if none exists). `ManualRestoreAsync(byte[])` replaces the database from backup bytes. The [example home page](PWAExample/Pages/Home.razor) demonstrates both. Browser persistence alone does not make application assets available offline; that requires a service worker/PWA setup.

## Run the example

[PWAExample](PWAExample) demonstrates ToDos, a persisted counter, backup/restore, and migrations in a separate data project. From the repository root, with a .NET 9 SDK and `wasm-tools` installed:

```sh
dotnet run --project PWAExample/PWAExample.csproj
```

The development service worker does not cache app assets for offline launch. Test offline app loading with a published PWA build, as described in [Blazor PWA guidance](https://learn.microsoft.com/en-us/aspnet/core/blazor/progressive-web-app?view=aspnetcore-9.0).

Released under the [Unlicense](LICENSE). Created by Eddie Hartman.
