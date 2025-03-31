UpFlux-WebApp

UpFlux-WebApp is a comprehensive web-based application designed to manage gateway simulation, hybrid access control systems, and form error handling. This repository contains the frontend, backend, gateway simulator, and database schema required for full functionality.

# 📂 Project Structure

UpFlux-WebApp/
├── .github/workflows/          # GitHub Actions workflows for CI/CD
├── Upflux-GatewaySimulator/    # Gateway Simulator for channel package signature handling
├── upflux-client/              # Frontend client application
├── upflux-webservice/          # Backend web services
├── .gitignore                  # Git ignore rules
├── README.md                   # Documentation
├── upflux_db.sql               # Database schema
├── upflux_db.sql.bak           # Database schema backup

# 🚀 Getting Started

## ✅ Requirements

  - Node.js
  - .NET SDK
  - SQL Server or compatible RDBMS

## 🔧 Installation Steps

  1. Clone Repository
  2. git clone https://github.com/your-organization/UpFlux-WebApp.git
  3. cd UpFlux-WebApp

## Install Frontend Dependencies
```
  cd upflux-client
  npm install
  npm start
```

## Setup Backend Web Service
```
  cd ../upflux-webservice
  dotnet restore
  dotnet build
  dotnet run
```

Database Setup

Import the upflux_db.sql file to set up a new database.

For restoring from backup, use upflux_db.sql.bak.

# 🌟 Features

Gateway Simulation: Manage and simulate control channel communication.

Hybrid Access Control: Comprehensive access control and audit functionality.

Enhanced Error Handling: Improved handling and user feedback for form-related errors.

Automated Workflow: CI/CD integration via GitHub Actions.

# 📦 Recent Updates

Error Handling Improvements (#58070)

Hybrid Access Control & Database Enhancements (#58066)

Gateway Channel Package Signature Handling (#57)

Manual GitHub Action Triggers (#45)

# 🌿 Branching Strategy

main: Stable production branch.

deploy: Deployment pipeline.

# 🤝 Contributing

Contributions are welcome! Follow these steps to contribute:

Fork this repository

Create your feature branch (git checkout -b feature-name)

Commit your changes (git commit -m 'Your feature description')

Push your branch (git push origin feature-name)

Open a Pull Request

# 📜 License

This project is licensed under the MIT License. See LICENSE for more information.

# 📞 Contact

If you encounter issues or have suggestions, please submit an issue or pull request on GitHub.
Thank you for using UpFlux-WebApp!
