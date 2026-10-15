# Introduction
TODO: Give a short introduction of your project. Let this section explain the objectives or the motivation behind this project.

# Getting Started

## Vytvoření databázové migrace

Používáme SQLite: každý vývojář si vytvoří vlastní lokální databázi ze společných migrací. Tím získá stejné tabulky, ale data uložená na počítači jiného vývojáře se nepřenesou.

### 1. Instalace .NET EF

Nainstalujte .NET 10 SDK, naklonujte repozitář a otevřete PowerShell v jeho kořeni. Nainstalujte nástroj pro migrace:

```powershell
dotnet tool install --global dotnet-ef
```

### 2. Nastavení vlastního připojení za pomocí User Secrets

V témže terminálu spusťte:

```powershell
cd .\HouseholdSupplies\

dotnet user-secrets set 'ConnectionStrings:DefaultConnection' "Data Source=household-supplies.db" --project .\HouseholdSupplies.slnx
```

**Nespouštějte znovu** `user-secrets init`. Každý má své hodnoty secrets uložené ve vlastním uživatelském profilu mimo repozitář.

### 3. Vytvoření lokální databáze za pomocí existující migrace

Pokračujte ve stejném terminálu:

```powershell
cd .\HouseholdSupplies.Api.DAL.EF\

dotnet ef database update --context HouseholdSuppliesDbContext
```

Vznikne soubor `HouseholdSupplies/household-supplies.db` s tabulkami.

# Build and Test
To test project run from the repository root with .NET 10:

```sh
dotnet test HouseholdSupplies/HouseholdSupplies.slnx --configuration Release
```

# Contribute
TODO: Explain how other users and developers can contribute to make your code better.

If you want to learn more about creating good readme files then refer the following [guidelines](https://docs.microsoft.com/en-us/azure/devops/repos/git/create-a-readme?view=azure-devops). You can also seek inspiration from the below readme files:
- [ASP.NET Core](https://github.com/aspnet/Home)
- [Visual Studio Code](https://github.com/Microsoft/vscode)
- [Chakra Core](https://github.com/Microsoft/ChakraCore)
