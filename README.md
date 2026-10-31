# Grocery List

A demo grocery list web application. Users register and log in, then manage their own private
shopping list: add and remove products, and tag products as picked up (in the basket) or put them
back. Nobody can see or change another user's list. The backend is an ASP.NET Core (.NET 10) REST
API following Clean Architecture with EF Core and SQL Server; the frontend is React + TypeScript
(Vite, Sass).

## Getting started

TBD

## Repository layout

| Path | Contents |
|---|---|
| `backend/` | .NET solution: Domain, Application, Infrastructure, Api and tests |
| `frontend/` | React + TypeScript single-page app |
| `docs/` | Project documentation |
| `docker-compose.yml` | Local SQL Server |

## Documentation

See [`docs/`](docs/).
