# ScreenTime – Biograf- och bokningssystem

ScreenTime är en fullstackapplikation för administration och bokning av biovisningar. Systemet har stöd för filmer, salonger, visningar, sätesval, bokningar, användarkonton och rollbaserad behörighet.

Projektet är utvecklat som ett grupparbete inom systemutveckling på Newton och består av ett REST API i ASP.NET Core samt en frontend i React.

## Funktioner

### För besökare och kunder

- Visa tillgängliga filmer och filminformation.
- Söka och filtrera filmer efter titel, genre, datum och salong.
- Visa kommande föreställningar.
- Registrera konto och logga in.
- Välja platser inför en föreställning.
- Se pris för standard- och VIP-platser.
- Skapa en bokning.
- Visa aktuella och tidigare bokningar.
- Avboka en bokning.

### För personal och administratörer

- Skapa, redigera och inaktivera filmer.
- Skapa och administrera salonger.
- Konfigurera salongernas stolar och sätestyper.
- Skapa, redigera och ställa in visningar.
- Visa även inställda visningar.
- Förhindra överlappande visningar i samma salong.
- Rollbaserad åtkomst för `Admin`, `Staff` och `Customer`.

Backend har även stöd för att lista användare och tilldela roller. Det fullständiga gränssnittet för personaladministration är under utveckling.

## Tekniker

### Backend

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- SQLite
- ASP.NET Core Identity
- JWT-autentisering
- Swagger/OpenAPI
- xUnit
- FluentAssertions

### Frontend

- React 19
- TypeScript
- Vite
- React Router
- CSS

### Övrigt

- Git och GitHub
- GitHub Actions
- Pull requests och feature branches
- Automatisk build och testning i CI

## Arkitektur

Backend är uppdelad i flera lager:

```text
backend/
├── src/
│   ├── ScreenTime.Domain/
│   │   ├── Entities/
│   │   ├── Enums/
│   │   └── Common/
│   ├── ScreenTime.Application/
│   │   ├── Contracts/
│   │   ├── Interfaces/
│   │   ├── Services/
│   │   └── Exceptions/
│   ├── ScreenTime.Infrastructure/
│   │   ├── Authentication/
│   │   ├── Data/
│   │   ├── Identity/
│   │   ├── Migrations/
│   │   └── Repositories/
│   └── ScreenTime.WebApi/
│       ├── Controllers/
│       ├── Filters/
│       └── Program.cs
└── tests/
    └── ScreenTime.WebApi.IntegrationTests/
```

Frontend är uppdelad efter ansvar:

```text
frontend/src/
├── api/
├── assets/
├── components/
├── context/
├── hooks/
├── pages/
├── types/
└── utils/
```

### Lagerfördelning

- **Domain** innehåller entiteter, enums och grundläggande domänmodeller.
- **Application** innehåller DTO:er, interfaces och affärslogik.
- **Infrastructure** hanterar databas, repositories, Identity och JWT.
- **WebApi** exponerar funktionaliteten genom REST-endpoints.
- **Frontend** presenterar systemet och kommunicerar med backend via HTTP.

## Tekniska koncept

Projektet demonstrerar minst tre av de efterfrågade teknikerna.

### Generics

En generell repository-abstraktion används för operationer som delas mellan olika entiteter:

```csharp
public interface IRepository<TEntity>
    where TEntity : BaseEntity
```

```csharp
public abstract class Repository<TEntity> : IRepository<TEntity>
    where TEntity : BaseEntity
```

`TEntity` gör att samma implementation kan återanvändas för exempelvis filmer, salonger, bokningar och visningar. Begränsningen `where TEntity : BaseEntity` garanterar att entiteten har ett `Id`.

### LINQ

LINQ används för bland annat filtrering, sortering, gruppering och omvandling:

```csharp
return await Context.Seats
    .Where(seat => auditoriumIds.Contains(seat.AuditoriumId))
    .GroupBy(seat => seat.AuditoriumId)
    .Select(group => new
    {
        AuditoriumId = group.Key,
        Count = group.Count()
    })
    .ToDictionaryAsync(
        item => item.AuditoriumId,
        item => item.Count);
```

### Asynkron programmering

Databas- och API-operationer använder `async`, `await` och `Task<T>`:

```csharp
public async Task<List<Auditorium>> GetAllAsync(
    CancellationToken cancellationToken = default)
{
    return await Query()
        .ToListAsync(cancellationToken);
}
```

Det gör att servertrådar inte behöver blockeras medan systemet väntar på databasoperationer.

## Användarscenarier

### Scenario 1 – Kund bokar biobiljetter

**Som kund vill jag kunna välja en visning och lediga stolar för att boka biobiljetter.**

1. Kunden öppnar en film och väljer en kommande visning.
2. Systemet visar salongens stolar och deras tillgänglighet.
3. Kunden väljer standard- eller VIP-stolar.
4. Systemet visar det totala priset.
5. Kunden bekräftar bokningen.
6. Bokningen visas under kundens aktuella bokningar.
7. De bokade stolarna markeras som otillgängliga för andra kunder.

### Scenario 2 – Kund avbokar en bokning

**Som kund vill jag kunna avboka en framtida bokning så att platserna blir tillgängliga igen.**

1. Kunden loggar in och öppnar sina bokningar.
2. Kunden väljer en bokning som fortfarande kan avbokas.
3. Systemet ändrar bokningens status till avbokad.
4. Bokningen flyttas till bokningshistoriken.
5. De reserverade stolarna blir tillgängliga för andra kunder.

### Scenario 3 – Personal administrerar en visning

**Som personal vill jag kunna skapa och administrera visningar för att hålla bioprogrammet uppdaterat.**

1. Personalen loggar in med rollen `Staff` eller `Admin`.
2. Personalen väljer film, salong samt startdatum och starttid.
3. Systemet beräknar sluttiden utifrån filmens längd och städtiden.
4. Systemet kontrollerar att visningen inte överlappar en annan visning i samma salong.
5. En giltig visning läggs till i programmet.
6. Personalen kan senare redigera eller ställa in visningen.

## Förutsättningar

Följande behöver vara installerat:

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) version 24 eller kompatibel version
- npm
- Git

Kontrollera installationerna med:

```powershell
dotnet --version
node --version
npm --version
```

## Installation

Klona repot och gå till projektmappen:

```powershell
git clone <repository-url>
cd ScreenTime
```

Återställ backendpaketen:

```powershell
dotnet restore
```

Installera frontendpaketen:

```powershell
cd frontend
npm install
cd ..
```

## Starta projektet

Backend och frontend behöver köras samtidigt i två terminaler.

### Terminal 1 – backend

Kör från projektets rot:

```powershell
dotnet run --project .\backend\src\ScreenTime.WebApi\ScreenTime.WebApi.csproj --launch-profile http
```

Backend startar på:

```text
http://localhost:5267
```

Swagger finns på:

```text
http://localhost:5267/swagger
```

### Terminal 2 – frontend

```powershell
cd frontend
npm run dev
```

Frontend öppnas på:

```text
http://localhost:5173
```

Frontend är konfigurerad för att kommunicera med backend på port `5267`.

## Databas och seed-data

Projektet använder SQLite. I utvecklingsmiljön utför backend automatiskt följande vid uppstart:

1. Skapar databasen om den saknas.
2. Kör väntande EF Core-migrationer.
3. Lägger in seed-data.
4. Skapar de roller som systemet använder.
5. Skapar utvecklingskonton.

Seed-datan innehåller bland annat:

- Filmer och genrer.
- Filmplanscher.
- Salonger och stolar.
- Standard- och VIP-platser.
- Visningar.
- Roller och testkonton.

### Återställa utvecklingsdatabasen

Stoppa backend och ta bort:

```text
backend/src/ScreenTime.WebApi/screenTime.db
```

Starta sedan backend igen. Databasen skapas och seedas på nytt.

> Detta tar bort alla lokalt skapade användare, bokningar och andra ändringar.

## Utvecklingskonton

Följande konton är endast avsedda för lokal utveckling och demonstration.

### Kund

```text
E-post: test@screentime.com
Lösenord: Test1234!
Roll: Customer
```

### Administratör

```text
E-post: admin@screentime.com
Lösenord: Admin1234!
Roll: Admin
```

Använd inte dessa lösenord i en produktionsmiljö.

## Roller och behörigheter

| Funktion | Customer | Staff | Admin |
|---|:---:|:---:|:---:|
| Visa filmer och visningar | Ja | Ja | Ja |
| Skapa bokningar | Ja | Ja | Ja |
| Visa egna bokningar | Ja | Ja | Ja |
| Administrera filmer | Nej | Ja | Ja |
| Administrera salonger | Nej | Ja | Ja |
| Administrera visningar | Nej | Ja | Ja |
| Administrera användarroller | Nej | Nej | Ja |

Behörigheterna kontrolleras både i frontend och i backend. Backend använder `[Authorize]` och rollbaserade policies för att skydda administrativa endpoints.

## Testning

Backend innehåller integrationstester för bland annat:

- Registrering och inloggning.
- Skyddade endpoints.
- Rollbaserad auktorisering.
- Skapande och redigering av visningar.
- Kontroll av överlappande visningar.
- Filtrering efter datum, genre och salong.
- Avbokning av visningar.
- Påverkan på befintliga bokningar.
- Bokningshistorik.

Kör alla backendtester:

```powershell
dotnet test .\ScreenTime.sln
```

Kör frontendens produktionsbygge:

```powershell
cd frontend
npm run build
```

Kör lint:

```powershell
npm run lint
```

Vid senaste lokala verifieringen passerade samtliga 26 backendtester och frontendens produktionsbygge slutfördes utan TypeScript-fel.

## API-dokumentation

När backend körs i utvecklingsläge finns den interaktiva API-dokumentationen i Swagger:

```text
http://localhost:5267/swagger
```

För endpoints som kräver autentisering:

1. Logga in genom `/api/auth/login`.
2. Kopiera JWT-token från svaret.
3. Klicka på **Authorize** i Swagger.
4. Klistra in token.
5. Testa den skyddade endpointen.

## CI/CD

Projektet använder GitHub Actions för att automatiskt:

- Återställa NuGet-paket.
- Bygga backend.
- Köra backendtester.
- Installera frontendpaket.
- Bygga frontend.
- Köra ESLint.

Workflow-filen finns i:

```text
.github/workflows/ci.yml
```

## Versionshantering och arbetsflöde

Projektet utvecklas med:

- Separata feature branches.
- Regelbundna commits.
- Pull requests.
- Kodgranskning innan merge.
- En gemensam utvecklingsbranch.
- Automatisk verifiering genom GitHub Actions.

Exempel på branchstruktur:

```text
main
└── dev
    ├── feature/movie-management
    ├── feature/booking-flow
    ├── feature/screenings
    └── feature/role-authorization
```

## Projektstatus

Följande delar är implementerade:

- Filmregister.
- Salonghantering.
- Visningsflöde.
- Bokningsflöde och sätesval.
- Bokningshistorik och avbokning.
- Registrering och inloggning.
- JWT-autentisering.
- Rollbaserad auktorisering.
- Sökning och filtrering.
- Generiskt repository.
- Integrationstester.
- GitHub Actions.

Kvarvarande arbete:

- Färdigställa frontend för personal- och användaradministration.
- Utöka dokumentationen av scenariobaserade tester.
- Genomföra slutlig manuell regressionstestning.
- Förbereda slutdatabasen inför inlämningen.

## Team

Projektet har utvecklats som ett grupparbete med agila arbetsmetoder, feature branches, pull requests och gemensam kodgranskning.

| Roll | Namn |
|---|---|
| Scrum Master | Björn |
| Tech Lead | Amanda |
| Testansvarig | Ezana/Filip/Amanda |
| Utvecklare | Lucas |
| Utvecklare | Ezana |
| Utvecklare | Filip |

## Licens

Projektet är framtaget i utbildningssyfte och är inte avsett för kommersiell användning.
