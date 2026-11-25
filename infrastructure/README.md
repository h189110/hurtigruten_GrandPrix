# Infrastructure

This folder contains the Docker Compose configuration that starts both the backend API and the frontend.

## Services

| Service | Container port | Default host port | Description                         |
|---------|----------------|-------------------|-------------------------------------|
| `api`   | 8080           | 5080              | ASP.NET Core Web API                |
| `web`   | 80             | 5173              | React app served by nginx           |

## Configuration

Set these environment variables (or add them to a `.env` file next to the compose file) to override the defaults:

| Variable                | Default                   | Purpose                                        |
|-------------------------|---------------------------|------------------------------------------------|
| `API_PORT`              | `5080`                    | Host port for the API                          |
| `WEB_PORT`              | `5173`                    | Host port for the frontend                     |
| `VITE_API_BASE_URL`     | `http://localhost:5080`   | Backend URL baked into the frontend at build   |
| `FRONTEND_ORIGIN`       | `http://localhost:5173`   | Origin allowed by the API's CORS policy        |
| `ASPNETCORE_ENVIRONMENT`| `Production`              | ASP.NET Core environment for the API           |

The frontend reads `VITE_API_BASE_URL` at build time, so rebuild the `web` image after changing it.

## Usage

```bash
docker compose -f infrastructure/docker-compose.yml up --build
docker compose -f infrastructure/docker-compose.yml down
```

No database container is included; the application does not require persistent storage yet.
