
---

## 🛠️ Used Technologies

- **.NET 10** (C# 13)
- **ASP.NET Core Web API**
- **Entity Framework Core 8** + **SQLite**
- **AutoMapper**
- **JWT Bearer Authentication**
- **Serilog**
- **Swagger/OpenAPI**
- **xUnit** + **EF Core InMemory**

---

**Running steps** 🚀  

- **Backend**
``` 
pushd code/backend/
dotnet clean
dotnet build
dotnet run --project TA-API/TA-API.csproj
``` 

- **Frontend**
``` 
pushd code/frontend/angular
npm install
npm start
```