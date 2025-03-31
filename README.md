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

### 📫 Authors & Maintainers
👨‍💻 Collaborative Team: UpFlux Final Project Team

🛠️ Tools Used: Postman · Playwright · C# · .NET 7 · Visual Studio 2022
