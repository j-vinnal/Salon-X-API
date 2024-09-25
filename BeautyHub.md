# BeautyHub

- `Name: Jüri Vinnal`
- `Uni-id: juvinn`
- `Student code: 192858IADB`

# DB Migrations

### Update tools

```bash
dotnet tool update --global dotnet-ef
```

```bash
dotnet tool update --global dotnet-aspnet-codegenerator
```

### Create new migration

```bash
dotnet ef migrations add --project DAL.EF --startup-project WebApp InitialCreate
```

```bash
dotnet ef database update --project DAL.EF --startup-project WebApp
```

## Code generation

```bash
cd WebApp
```

### API controllers

```bash
dotnet aspnet-codegenerator controller -m Domain.Provider -name ProvidersController -outDir Api -api -dc AppDbContext -udl --referenceScriptLibraries -f
```

### MVC

```bash
dotnet aspnet-codegenerator controller -m Domain.Provider -name ProvidersController -outDir Controllers -dc AppDbContext -udl --referenceScriptLibraries -f
```