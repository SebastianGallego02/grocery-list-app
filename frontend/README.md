# Grocery List – frontend

React + TypeScript single-page app built with Vite and Sass. See the root `README.md` for the
whole project.

## Scripts (run from `frontend/`)

| Command | What it does |
|---|---|
| `npm run dev` | Dev server on http://localhost:5173; `/api` is proxied to the API on http://localhost:5079 |
| `npm run build` | Type-check and production build into `dist/` |
| `npm run lint` | Oxlint |
| `npm test` | Unit and component tests (Vitest + Testing Library, jsdom) |

## Configuration

| Variable | Default | Purpose |
|---|---|---|
| `VITE_API_BASE_URL` | empty (same origin) | Base URL of the API, baked in at build time (the Docker image sets it) |
| `API_PROXY_TARGET` | `http://localhost:5079` | Dev server only: where `/api` is proxied |
