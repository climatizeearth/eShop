# Usage

```
git clone git@github.com:climatizeearth/eShop.git
cd eShop

dotnet sln remove src\ClientApp\ClientApp.csproj
dotnet sln remove src\HybridApp\HybridApp.csproj

dotnet workload restore
dotnet restore
dotnet build

dotnet run --project .\src\eShop.AppHost\
```

Then open the Aspire dashboard. It is at a URL that looks like this:
https://localhost:19888/login?t=29ebf147cf4104ce7645008e40133d90

Next, open the Ordering API. It is usually at this URL:
http://localhost:5224/scalar/v1

# Relevant Projects

These projects are of most interest to Project Kernel:

Demonstration of .NET Aspire

- src/eShop.AppHost

Demonstration of Domain Driven Design with Entity Framework

- Ordering.API
- Ordering.Domain
- Ordering.Infrastructure
