# UpFlux-WebApp

UpFlux-WebApp is UpFlux's Cloud Solution which consists of Web-Service, Client Application, and the Gateway Simulator.

 ![image](https://github.com/user-attachments/assets/156a2dce-c458-42b0-99c0-f5ac2a2c5091)

# 📂 Project Structure
```
UpFlux-WebApp/
├── .github/workflows/          # GitHub Actions workflows for CI/CD
├── Upflux-GatewaySimulator/    # Gateway Simulator for simulating a real gateway connection
├── upflux-client/              # Frontend client application
├── upflux-webservice/          # Backend web services
├── .gitignore                  # Git ignore rules
├── README.md                   # Documentation
├── upflux_db.sql               # Database schema
├── upflux_db.sql.bak           # Database schema backup
```
# 🌟 Features

- Gateway Simulator: Simulato persistent gRPC connection of the real gateway.

- Web-Service: Contains the core busines logic of UpFlux and the APIs which expose these logics.

- Client Application: Contains the UI of UpFlux.

- Automated Workflow: CI/CD integration via GitHub Actions.
  
# 🌿 Branching Strategy

`main`: Stable production branch.

`deploy`: Deployment pipeline (deploys web-service automatically to UpFlux's EC2 instance).

# 🚀 Getting Started

## ✅ Requirements

  - Node.js
  - .NET SDK
  - SQL Server or compatible RDBMS

## 📦 Backend Dependencies

The `upflux-webservice` project is built with .NET 8 and relies on the following NuGet packages:

### 🔒 Authentication & Security
- `Microsoft.AspNetCore.Authentication.JwtBearer` – JWT-based authentication
- `Microsoft.IdentityModel.Tokens` – Token validation for JWT
- `System.IdentityModel.Tokens.Jwt` – JWT support and parsing

### 🌐 gRPC & Protocol Buffers
- `Grpc.AspNetCore.Server` – gRPC server support for ASP.NET Core
- `Grpc.Tools` – gRPC tooling and code generation
- `Google.Protobuf` – Protocol Buffers support

### 🗃️ Database & ORM
- `Microsoft.EntityFrameworkCore.Tools` – EF Core CLI support
- `Pomelo.EntityFrameworkCore.MySql` – MySQL support for EF Core
- `MySqlConnector` – A high-performance MySQL ADO.NET library

### 📊 Logging
- `Serilog` – Structured logging for .NET
- `Serilog.AspNetCore` – ASP.NET Core integration
- `Serilog.Settings.Configuration` – Load config from `appsettings.json`
- `Serilog.Sinks.File` – Log to files

### 📦 API & Tooling
- `Swashbuckle.AspNetCore` – Swagger/OpenAPI docs for ASP.NET Core APIs
- `Newtonsoft.Json` – JSON serialization/deserialization

### ⚙️ Utilities
- `DotNetRateLimiter` – Rate limiting middleware
- `System.Threading.RateLimiting` – Built-in rate limiting (preview)
- `AWSSDK.KeyManagementService` – AWS KMS integration

## 🔧 Installation Steps

 ###  1. Clone Repository
 ```
   git clone https://github.com/your-organization/UpFlux-WebApp.git
   cd UpFlux-WebApp
```

###  2. Install Frontend Dependencies
```
  cd upflux-client
  npm install
  npm start
```

### 3. Setup Backend Web Service
```
  cd ../upflux-webservice
  dotnet restore
  dotnet build
  dotnet run
```

### 4. Database Setup

  - Import the upflux_db.sql file to set up a new MySql database.
  
  - For restoring from backup, use upflux_db.sql.bak.

### 5. Setup Gateway Simulator
  - Run the web-service project
    
  - Run the gateway simulator project
    
  - Send device status data through gateway simulator console application
    
  - Gateway simulator is now persistently connected to the web-service

📫 Authors & Maintainers
👨‍💻 Collaborative Team: UpFlux Final Project Team
