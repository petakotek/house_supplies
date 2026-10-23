# Introduction
TODO: Give a short introduction of your project. Let this section explain the objectives or the motivation behind this project.

# Getting Started

## Vytvoření databázové migrace

Aplikace i všechny integrační testy používají Azure SQL provider a jedinou sadu produkčních migrací v projektu `HouseholdSupplies.Api.DAL`.

### 1. Instalace .NET EF

Nainstalujte .NET 10 SDK a v kořeni repozitáře nainstalujte nástroj odpovídající verzi EF Core:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.13
# Pokud už nástroj máte:
# dotnet tool update --global dotnet-ef --version 10.0.13
```

### 2. Nastavení vlastního připojení přes User Secrets

Nastavte připojení k vlastní vývojové databázi SQL Server / Azure SQL pro DAL (migrační nástroj). API přijímá připojení přes proměnnou prostředí:

```powershell
dotnet user-secrets set 'ConnectionStrings:DefaultConnection' '<SQL Server connection string>' --project .\HouseholdSupplies\HouseholdSupplies.Api.DAL\HouseholdSupplies.Api.DAL.csproj
$env:ConnectionStrings__DefaultConnection = '<SQL Server connection string>'
```

**Nespouštějte znovu** `user-secrets init`. Existující identifikátory secrets se nemění. Přihlašovací údaje nepatří do repozitáře. Migrační nástroj také přijímá proměnnou `ConnectionStrings__DefaultConnection`, která má přednost před User Secrets.

### 3. Aktualizace vlastní vývojové databáze

Z kořene repozitáře spusťte:

```powershell
dotnet ef database update --project .\HouseholdSupplies\HouseholdSupplies.Api.DAL --context HouseholdSuppliesDbContext
```

Pro novou změnu modelu vytvořte SQL Server migraci v DAL; integrační testy používají tutéž sadu migrací. Produkční pipeline generuje idempotentní SQL skript bez připojení k databázi; následný deployment ho aplikuje na Azure SQL.

### Oprava původního Azure SQL deploymentu

Původní `InitialMigration` používala typ `TEXT` i pro primární klíče GUID. SQL Server tento typ nemůže indexovat, proto selhala už tvorba tabulky `Users`. Následující `ProductionMigration` se ke konverzi typů nikdy nedostala.

Produkční migrace nyní od začátku používají typy SQL Serveru (`uniqueidentifier`, `nvarchar(max)`, `int`, `bit`, `float`, `datetime2`). Identifikátory migrací zůstávají stejné; `ProductionMigration` již nic nekonvertuje.

Pro databázi ze zaznamenaného neúspěšného prvního deploymentu stačí znovu spustit pipeline s opraveným skriptem; není potřeba mazat databázi ani historii migrací. Ručně upravené databáze nebo databáze s jinou již aplikovanou historií vyžadují samostatné porovnání schématu.

# Build and Test
All database tests require `HouseholdSuppliesTests__ConnectionString` pointing to a dedicated SQL Server or Azure SQL test server, with permission to create/drop isolated test databases. The pipeline uses Windows LocalDB. Setup and lifecycle details are in [the DAL testing guide](HouseholdSupplies/HouseholdSupplies.Api.DAL.Tests/README.md).

To test the project, configure that variable and run from the repository root with .NET 10:

```sh
dotnet test HouseholdSupplies/HouseholdSupplies.slnx --configuration Release
```

# REST API

| Metoda | Cesta | Operace |
| --- | --- | --- |
| GET | `/api/item`, `/api/user`, `/api/place`, `/api/shopping-list` | Seznam |
| GET | `/api/item/{id}`, `/api/user/{id}`, `/api/place/{id}`, `/api/shopping-list/{id}` | Detail |
| POST | `/api/item`, `/api/user`, `/api/place`, `/api/shopping-list` | Vytvoření (API vygeneruje ID, response obsahuje GUID) |
| PUT | `/api/item`, `/api/user`, `/api/place`, `/api/shopping-list` | Úprava (ID v těle požadavku) |
| DELETE | `/api/item/{id}`, `/api/user/{id}`, `/api/place/{id}`, `/api/shopping-list/{id}` | Smazání |
| POST | `/api/item/save`, `/api/place/save`, `/api/shopping-list/save` | Vytvoření nebo úprava záznamu |
| POST | `/api/item/{id}/shopping-list/{shoppingListId}` | Přidání položky do libovolného nákupního seznamu; tělo `{"amount": 2}` |

Všechny seznamy přijímají `searchQuery`, `ascending` (výchozí `true`), `page` (od 1) a `pageSize` (výchozí 25). Položky lze filtrovat přes `placeId` a `categoryName`, uživatele přes `role` (`Admin`, `User` nebo číselná hodnota), místa a nákupní seznamy přes `authorId`. Řazení je podle jména/názvu a následně ID.

# Contribute
TODO: Explain how other users and developers can contribute to make your code better.

If you want to learn more about creating good readme files then refer the following [guidelines](https://docs.microsoft.com/en-us/azure/devops/repos/git/create-a-readme?view=azure-devops). You can also seek inspiration from the below readme files:
- [ASP.NET Core](https://github.com/aspnet/Home)
- [Visual Studio Code](https://github.com/Microsoft/vscode)
- [Chakra Core](https://github.com/Microsoft/ChakraCore)
