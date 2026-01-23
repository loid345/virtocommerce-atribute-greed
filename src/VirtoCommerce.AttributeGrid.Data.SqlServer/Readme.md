## Package manager
```
Add-Migration Initial -Context VirtoCommerce.AttributeGrid.Data.Repositories.AttributeGridDbContext -Project VirtoCommerce.AttributeGrid.Data.SqlServer -StartupProject VirtoCommerce.AttributeGrid.Data.SqlServer -OutputDir Migrations -Verbose -Debug
```

### Entity Framework Core Commands
```
dotnet tool install --global dotnet-ef --version 8.*
```

**Generate Migrations**
```
dotnet ef migrations add Initial -- "{connection string}"
dotnet ef migrations add Update1 -- "{connection string}"
dotnet ef migrations add Update2 -- "{connection string}"
```
etc..

**Apply Migrations**
```
dotnet ef database update -- "{connection string}"
```
