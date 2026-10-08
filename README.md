# Dawaee Pharmacy

A full-stack medical and pharmacy platform built with an ASP.NET Core backend and a static RTL frontend. The system supports authentication, medication browsing, patient scheduling, admin management, and an AI-powered medication assistant.

## Live Demo

- Frontend: https://tasharuky.duckdns.org/pharmacy/
- Repository: https://github.com/superiorshipet/pharmacy

## Overview

Dawaee Pharmacy is a demo healthcare platform designed for:

- discovering medications in Arabic and English
- managing personal medication schedules
- tracking daily adherence and weekly reports
- authenticating users with JWT tokens
- managing admin-only patient and medication workflows
- chatting with a medication assistant backed by Groq or a local fallback system

The project is organized into two main areas:

- backend/: ASP.NET Core Web API
- frontend/: HTML/CSS/JavaScript patient-facing web app

## Technology Stack

### Backend

- C#
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL via Npgsql
- JWT authentication
- Swagger / OpenAPI
- BCrypt for password hashing
- Docker support
- Railway deployment support

### Frontend

- HTML
- CSS
- JavaScript
- Tailwind CSS via CDN
- Arabic RTL layout

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

## Key Features

### Patient Features

- register and log in
- receive JWT token access
- browse medications by name or active ingredient
- view medication details and warnings
- create and manage medication schedules
- mark doses as taken or not taken
- view weekly medication adherence stats
- update profile information and password
- chat with the medication assistant

### Admin Features

- view all patients
- inspect individual patient details and chat history
- review adherence rates and schedule information
- manage medication records
- create, update, and delete medications
- delete patient accounts

### AI Chatbot

The chatbot endpoint uses:

- a Groq LLM when `Groq:ApiKey` is configured
- a built-in local response system as a fallback

It includes emergency response handling and medication-specific assistance, while also storing chat history per user.

## API Overview

The backend exposes the API under `/api` and includes the following controllers:

### Auth

- `POST /api/auth/register`
  - registers a new patient user
  - returns a JWT token
- `POST /api/auth/login`
  - authenticates a user
  - returns user profile data and JWT token

### Medications

- `GET /api/medications?search={term}`
  - returns medication records
  - supports search by Arabic/English names and active ingredients

### Profile

- `GET /api/profile`
  - returns current user profile and summary stats
- `PUT /api/profile`
  - updates first name, last name, and optionally password

### Dashboard

- `GET /api/dashboard/schedules`
  - returns today's scheduled medications for the authenticated user
- `POST /api/dashboard/schedules`
  - creates a new medication schedule
- `PUT /api/dashboard/schedules/toggle`
  - marks a schedule as taken or not taken
- `DELETE /api/dashboard/schedules/{id}`
  - removes a schedule
- `GET /api/dashboard/weekly-report`
  - returns weekly adherence statistics

### Chatbot

- `POST /api/chatbot/chat`
  - sends a chat message and receives AI-generated output
- `GET /api/chatbot/history`
  - returns recent user chat history

### Admin

- `GET /api/admin/patients`
  - lists all patients and their adherence summary
- `GET /api/admin/patients/{id}`
  - gets a patient profile with schedules and chat history
- `DELETE /api/admin/patients/{id}`
  - removes a patient
- `GET /api/admin/medications`
  - lists medications
- `POST /api/admin/medications`
  - creates a medication
- `PUT /api/admin/medications/{id}`
  - updates a medication
- `DELETE /api/admin/medications/{id}`
  - removes a medication

### Health

- `GET /health`
  - basic application health endpoint

## Local Development

### Prerequisites

- .NET 10 SDK
- PostgreSQL database
- optional: Docker

### Backend Setup

```bash
cd backend
dotnet restore
dotnet run
```

The backend uses local connection strings from `appsettings.json` when no environment variables are set.

Swagger UI is available at:

```text
http://localhost:5000/swagger
```

Health endpoint:

```text
http://localhost:5000/health
```

### Frontend Setup

You can open the frontend directly in a browser or serve it locally.

```bash
cd frontend
python -m http.server 8000
```

Then open:

```text
http://localhost:8000
```

## Environment Variables

The application supports the following environment variables:

- `DATABASE_URL`
- `JWT_KEY`
- `RAILWAY_ENVIRONMENT`
- `Groq:ApiKey`
- `Groq:Model`

### Database behavior

The backend auto-detects whether it is running in Railway or local mode:

- if `DATABASE_URL` exists, it converts it to a PostgreSQL connection string
- otherwise it falls back to `LocalConnection` or `RailwayConnection`

## Seeded Demo Accounts

When the app starts, it seeds default users:

- Admin: `admin@dawaee.com` / `Admin123!`
- Patient: `patient@test.com` / `Patient123!`

## Deployment

The repository includes:

- `Dockerfile` for containerized deployment
- `railway.json` for Railway hosting

The app is configured to run in a cloud environment with PostgreSQL and environment variables such as `DATABASE_URL` and `JWT_KEY`.

## Notes

This project is a demonstration medical/pharmacy platform with seed data and sample accounts intended for local development, testing, and presentation purposes.

## License

This repository does not currently include a license file.
