# Dawaee Pharmacy

A full-stack Arabic/English medical platform for managing medications, patient schedules, and healthcare dashboard workflows. The project combines a .NET backend with a static frontend and includes AI-assisted medication chat support.

## Live Demo

- Frontend: https://tasharuky.duckdns.org/pharmacy/
- Repository: https://github.com/superiorshipet/pharmacy

## Project Summary

Dawaee Pharmacy is designed to help patients and administrators manage healthcare-related workflows through a simple web experience. It includes:

- medication catalog browsing
- personal medication scheduling
- adherence tracking and weekly reports
- JWT-based authentication
- profile management
- admin tools for patient and medication management
- medication chatbot with Groq AI support and local fallback
- deployment support for Docker and Railway

## Tech Stack

### Backend

- C#
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL (Npgsql)
- JWT authentication
- Swagger / OpenAPI
- BCrypt.Net for password hashing
- Docker support
- Railway-ready configuration

### Frontend

- HTML
- CSS
- JavaScript
- Tailwind CSS via CDN
- RTL Arabic layout

## Repository Structure

```text
.
├── backend/
│   ├── Controllers/
│   │   ├── AdminController.cs
│   │   ├── AuthController.cs
│   │   ├── ChatbotController.cs
│   │   ├── DashboardController.cs
│   │   ├── MedicationsController.cs
│   │   ├── ProfileController.cs
│   │   └── TestController.cs
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
├── .gitignore
├── Dockerfile
├── DawaeePlatform.slnx
├── railway.json
├── README.md
└── ...
```

## Features

### Patient Features

- register a new patient account
- log in securely with JWT
- browse medications by Arabic/English name or active ingredient
- view medication details, ingredient information, warnings, and risk level
- create medication reminders/schedules
- toggle dose completion status
- remove schedules
- see daily and weekly adherence performance
- update account profile and password
- chat with the medication assistant

### Admin Features

- list all patients
- view patient details, schedules, and chat history
- evaluate adherence percentages
- manage medication catalog entries
- create, update, and delete medications
- delete patient records

### AI Chatbot

The chatbot is implemented in `ChatbotController` and includes:

- Groq API integration when `Groq:ApiKey` is configured
- local fallback responses for offline or missing-key scenarios
- emergency detection for urgent cases
- medication lookups and personalized responses grounded in the medication catalog
- saved user chat history

## Application Behavior

### Backend Boot Flow

`Program.cs` performs the following on startup:

- configures controllers and Swagger
- configures JWT authentication
- configures CORS
- connects to PostgreSQL
- detects Railway vs local environment
- exposes `/health`
- ensures database tables exist
- seeds default demo data
- starts the ASP.NET Core application

### Database and Demo Data

The app seeds default records for demo use, including:

- admin user
- patient user
- a list of sample medications

## API Documentation

The API is served at `/api` and documented with Swagger UI at `/swagger`.

### Authentication

#### Register

- `POST /api/auth/register`

Request body example:

```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@example.com",
  "password": "StrongPass123!",
  "chronicDiseases": "Diabetes"
}
```

Response includes a JWT token and user info.

#### Login

- `POST /api/auth/login`

Request body example:

```json
{
  "email": "patient@test.com",
  "password": "Patient123!"
}
```

### Medications

- `GET /api/medications`
- `GET /api/medications?search=panadol`

Returns medication records with Arabic and English names, active ingredients, descriptions, warnings, and danger level.

### Profile

- `GET /api/profile`
- `PUT /api/profile`

The profile endpoint returns current user details and usage statistics such as:

- total schedules
- taken doses
- chat message count

### Dashboard

- `GET /api/dashboard/schedules`
- `POST /api/dashboard/schedules`
- `PUT /api/dashboard/schedules/toggle`
- `DELETE /api/dashboard/schedules/{id}`
- `GET /api/dashboard/weekly-report`

These endpoints are protected and allow authenticated patients to manage medication timing and adherence reports.

### Chatbot

- `POST /api/chatbot/chat`
- `GET /api/chatbot/history`

The chatbot sends a user message and returns a response generated from either Groq or a local fallback logic. Previous conversation history is stored per user.

### Admin

- `GET /api/admin/patients`
- `GET /api/admin/patients/{id}`
- `DELETE /api/admin/patients/{id}`
- `GET /api/admin/medications`
- `POST /api/admin/medications`
- `PUT /api/admin/medications/{id}`
- `DELETE /api/admin/medications/{id}`

Admin routes require the `Admin` role.

### Health

- `GET /health`

Returns a lightweight health response indicating the app status and environment.

## Frontend Overview

The frontend is static and includes the main app entry point plus a profile page.

### `frontend/index.html`

This is the primary patient dashboard and landing page. It includes:

- Arabic RTL design
- Tailwind-based layout
- medication browsing interface
- chatbot widget
- modal and toast interfaces
- schedule and dashboard interactions

### `frontend/profile.html`

This provides the user profile experience and additional account actions.

## Local Setup

### Prerequisites

- .NET 10 SDK
- PostgreSQL server
- optional: Docker

### Backend

```bash
cd backend
dotnet restore
dotnet run
```

The app will run with local database settings when environment variables are not configured.

Swagger UI:

```text
http://localhost:5000/swagger
```

Health check:

```text
http://localhost:5000/health
```

### Frontend

Serve the frontend locally:

```bash
cd frontend
python -m http.server 8000
```

Then open:

```text
http://localhost:8000
```

## Environment Variables

The app reads these values when provided:

- `DATABASE_URL`
- `JWT_KEY`
- `RAILWAY_ENVIRONMENT`
- `Groq:ApiKey`
- `Groq:Model`

### Runtime behavior

- if `DATABASE_URL` is present, it is converted into a PostgreSQL connection string
- otherwise it falls back to local configuration values
- if a Groq key is not configured, the chatbot falls back to local scripted responses

## Demo Credentials

Seeded accounts:

- Admin: `admin@dawaee.com` / `Admin123!`
- Patient: `patient@test.com` / `Patient123!`

## Deployment

The repository contains deployment configuration for cloud hosting:

- `Dockerfile`
- `railway.json`

The app is designed to run in a containerized or Railway environment with PostgreSQL connectivity and environment variables set.

## Notes

This repository is a demo medical/pharmacy project with sample data and seeded users intended for development, testing, and presentation use.

## License

No explicit project license file is included in the repository at this time.
