# Hurtigrute Grand Prix

A small portfolio web application that ranks Hurtigruten vessels by their current speed, using public AIS data from the Norwegian Coastal Administration (Kystverket).

This repository currently contains the project foundation only: the API placeholder endpoint and the frontend shell. AIS integration and leaderboard calculations are not implemented yet.

## Technology Stack

- **Backend:** C#, .NET 10, ASP.NET Core Web API (controllers), OpenAPI, IHttpClientFactory
- **Frontend:** React, TypeScript, Vite, CSS, ESLint (npm)
- **Infrastructure:** Docker, Docker Compose, GitHub Actions

## Repository Structure

```text
backend/GrandPrix.Api/     ASP.NET Core Web API (Controllers, Services, Models, DTOs, Interfaces, Configuration)
frontend/grand-prix-web/   React + TypeScript + Vite application
infrastructure/            Docker Compose configuration
.github/workflows/         CI workflow
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org/) with npm
- [Docker](https://www.docker.com/) with Docker Compose (optional, for containers)

## Running the Backend Locally

```bash
dotnet run --project backend/GrandPrix.Api/GrandPrix.Api.csproj --launch-profile http
```

## Running the Frontend Locally

```bash
cd frontend/grand-prix-web
cp .env.example .env
npm install
npm run dev
```

The `.env` file sets `VITE_API_BASE_URL` to point at the backend.

## Running with Docker Compose

```bash
docker compose -f infrastructure/docker-compose.yml up --build
```

## Running a Published Release

Each version tag (`v*`) publishes one combined image to GitHub Container Registry. It contains the API and the built web frontend, which the API serves itself. Run it with one command, without the source code:

```bash
docker run -p 8080:8080 ghcr.io/hurtigrute-grand-prix/hurtigrute-grand-prix:latest
```

Open `http://localhost:8080` for the leaderboard. The API is at `http://localhost:8080/api/ships`.

The `docker-compose.release.yml` file attached to each GitHub Release runs the same image with compose. Set `APP_PORT` to change the host port (default `5080`).

## Local Development URLs

| Service             | URL                                  |
|---------------------|--------------------------------------|
| Frontend (Vite dev) | http://localhost:5173                |
| Frontend (Docker)   | http://localhost:5173                |
| Backend API         | http://localhost:5080                |
| Placeholder endpoint| http://localhost:5080/api/ships      |
| OpenAPI (Development) | http://localhost:5080/openapi/v1.json |
