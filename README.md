# Dawaee Pharmacy

A full-stack medical platform for browsing medications, managing pharmacy data, and exposing a secure API. The project combines a .NET backend with a static frontend interface and is designed to run locally or on Railway.

## Overview

This repository contains:

- backend: ASP.NET Core Web API with JWT authentication, PostgreSQL support, Swagger, and seeded demo data
- frontend: responsive Arabic/RTL web interface for browsing medications and user flows
- deployment files: Dockerfile and Railway configuration for cloud deployment

## Features

- Medication catalog with Arabic and English names
- Pharmacy-style dashboard and profile views
- JWT-based authentication and authorization
- Role-based demo accounts (admin and patient)
- PostgreSQL database integration
- Swagger API documentation at /swagger
- Health endpoint at /health
- Railway-ready deployment configuration

## Tech Stack

- Backend: C#, ASP.NET Core, Entity Framework Core, PostgreSQL, JWT
- Frontend: HTML, CSS, JavaScript
- Deployment: Docker, Railway

## Repository Structure

```text
.
├── backend/
│   ├── Controllers/
│   ├── Data/
│   ├── DTOs/
│   ├── Migrations/
│   ├── Models/
│   ├── appsettings.json
│   ├── DawaeeBackend.csproj
│   ├── Program.cs
│   └── ...
├── frontend/
│   ├── css/
│   ├── js/
│   ├── index.html
│   └── profile.html
├── Dockerfile
├── railway.json
├── DawaeePlatform.slnx
├── .gitignore
└── README.md
```

## Getting Started

### Prerequisites

- .NET 10 SDK
- PostgreSQL database
- Optional: Docker for containerized deployment

### Backend setup

```bash
cd backend
 dotnet restore
 dotnet run
```

The API will be available at:

- Swagger UI: http://localhost:5000/swagger
- Health check: http://localhost:5000/health

### Frontend setup

Open the frontend in a browser, or serve it locally with a lightweight static server.

Example:

```bash
cd frontend
python -m http.server 8000
```

Then visit:

- http://localhost:8000

## Environment Variables

The backend reads these values when present:

- DATABASE_URL
- JWT_KEY
- RAILWAY_ENVIRONMENT

If no database URL is supplied, it falls back to connection strings from appsettings.json for local development.

## Demo Credentials

Seeded users from the application include:

- Admin: admin@dawaee.com / Admin123!
- Patient: patient@test.com / Patient123!

## Deployment

The project includes a Dockerfile and Railway configuration for deployment. For Railway, the application expects PostgreSQL connectivity and environment variables such as DATABASE_URL and JWT_KEY.

## License

This project does not currently specify a license file.

## Notes

This repository is a demo medical/pharmacy platform with seeded content and sample accounts intended for development and demonstration purposes.
