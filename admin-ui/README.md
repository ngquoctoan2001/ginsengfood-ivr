# IVR Admin UI

Next.js App Router application for the GinsengFood IVR administration surface.
P0-1 contains only a static mock-mode safety page; authentication, API clients,
orders, calls, and operational controls are intentionally absent.

## Local commands

From the repository root:

```powershell
npm --prefix admin-ui run dev
npm --prefix admin-ui run lint
npm --prefix admin-ui run build
npm --prefix admin-ui run start
```

The local development server is available at `http://localhost:3000` by
default. The application uses strict TypeScript and does not load remote fonts
or provider data.

Deployment and GitLab CI configuration are not part of P0-1. They are governed
by later prompts; do not add a Vercel or GitHub CI path here.
