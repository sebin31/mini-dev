# Mini TWISE — Weekend Learning Project

A small, TWISE-style multi-container stack for practicing Docker + AWS + CI/CD,
without touching the real TWISE environment.

## Architecture

```
Nginx (reverse proxy, port 8080)
 ├── /api, /swagger, /health  -> API container (ASP.NET Core, port 8080 internal)
 └── /                        -> Frontend container (Angular, served by internal Nginx, port 80)

API container -> db container (SQL Server 2022, port 1433)

All containers share one custom bridge network: mini_default
```

This mirrors TWISE's shape (API + Frontend + SQL Server + Nginx, all on one Docker network)
just with a single API service instead of ControlPlane + Tenant + ETL.

## Run it locally

From the `mini-twise/` folder:

```bash
docker compose up --build
```

First run will take a few minutes (SQL Server image is large, Angular build takes a bit).

Once it's up:
- Frontend: http://localhost:8080
- Swagger: http://localhost:8080/swagger
- Health check: http://localhost:8080/health
- API directly (bypassing Nginx): http://localhost:9300/api/items

The API uses `Database.EnsureCreatedAsync()` on startup instead of real EF Core
migrations, so there's no `dotnet ef migrations add` step needed to get going.
When you're ready to practice real migrations (like TWISE does), install the
`dotnet-ef` tool and switch `Program.cs` to use `Database.MigrateAsync()` instead —
that's a good Day 3+ exercise.

To stop and remove containers:
```bash
docker compose down
```

To also wipe the database volume:
```bash
docker compose down -v
```

## Project layout

```
mini-twise/
├── api/              ASP.NET Core minimal API (EF Core + SQL Server)
├── frontend/          Angular 17 standalone app
├── nginx/              Top-level reverse proxy config
├── docker-compose.yml  Wires everything together
└── README.md
```

## Day 2: AWS + CI/CD (next steps)

1. **VPC & networking** — one VPC, one public subnet, a security group allowing
   inbound 22 (SSH), 80 (HTTP), and optionally 443.
2. **EC2** — a `t2.micro`/`t3.micro` instance, install Docker + Docker Compose
   plugin via a `user-data` script or manually over SSH.
3. **Manual first deploy** — `git clone` this repo onto the instance, run
   `docker compose up --build -d` there, confirm you can reach it via the
   instance's public IP on port 8080.
4. **CI/CD** — a GitHub Actions workflow that, on push to `main`:
   - builds the `api` and `frontend` images
   - pushes them to Docker Hub or ECR
   - SSHes into the EC2 instance
   - runs `docker compose pull && docker compose up -d`
   - hits `/health` to confirm the deploy worked, with a manual rollback step
     (redeploy the previous image tag) if it fails — same shape as TWISE's
     pipeline, just smaller.

Want help with any of these? Just ask and we can go step by step.
