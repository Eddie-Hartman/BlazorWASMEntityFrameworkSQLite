# Welcome

This repository exports a library called BlazorWASMEntityFrameworkSQLite. Creatively named, this library allows easy use of Entity Framework in Blazor WASM with SQLite being the database backing storage.

This works by creating a database in the [Emscripten File System API](https://emscripten.org/docs/api_reference/Filesystem-API.html) exposed by [Blazor WASM Javascript Interop](https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/?view=aspnetcore-9.0) APIs.

## Getting started

Take a look at the PWAExample code to get familiar with everything.

1. Import the nuget package to your Blazor WASM project.
2. Add `builder.Services.AddBWEFSDbContextFactory<YOURDBCONTEXT>();` in your program.cs to set up dependency injection.
3. Replace `YOURDBCONTEXT` with your db context class above.
4. For default settings, that's it in terms of setup.

Optional:
To avoid getting warnings on build, add `<NoWarn>WASM0001</NoWarn>` to a `<PropertyGroup>` in your Blazor WASM project.

**IMPORTANT** If you are NOT using entity framework core migrations, be sure to set the `useMigrations` parameter to false in the `AddBWEFSDbContextFactory` method. This uses the `EnsureCreated()` method instead of migrations to make sure the database is created.

## Example project

An example project is viewable in the PWAExample project. Simply run the project to see a working Blazor WASM PWA with a working offline database that also handles EF Migrations.