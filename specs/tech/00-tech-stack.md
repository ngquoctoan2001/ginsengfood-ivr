# TECH-STACK — Standalone IVR Service

Trạng thái: `ACCEPTED_FOR_IMPLEMENTATION` · Target contract remains draft.

## Stack

| Thành phần | Công nghệ |
| --- | --- |
| API + Worker | .NET 10 / ASP.NET Core / Worker Service |
| Domain/contracts | C# nullable, versioned DTOs, provider ports |
| Persistence | PostgreSQL + EF Core migrations + Postgres outbox |
| Admin UI | Next.js + TypeScript strict |
| Contract | OpenAPI 3.1 + consumer-driven tests |
| Observability | OpenTelemetry + structured redacted logs |
| Deploy | Docker, Compose dev, Kubernetes/Helm target |

Sales Platform remains Java/Spring Boot. Separate repositories/stores are recommended and required by this plan; integration uses HTTP/OpenAPI, not shared code/database.

## Required ports/providers

- `ISalesTaskSource`/intake endpoint and deterministic fake producer;
- `IOrderCoreCallbackClient`: target generic + current Golden Hour compatibility adapters;
- `IDialTokenResolver` fake/real;
- `ISimGateway`: mock and vendor adapter;
- `ISpeechRenderer`/TTS abstraction;
- `IAttemptPolicyRegistry` with environment approval;
- auth token provider, audit/evidence and time/ID abstractions.

## Modes

`MOCK` → `LAB_REAL_SIM` (1 SIM, allowlist) → `PRODUCTION_REAL` (target 32 eSIM). Provider selection and channel count are configuration. No code path may infer real-call permission from adapter availability alone.

## Invariants

IVR never transitions order or sends notification; no raw phone/full address; program matrix is GH ONLINE/24-7 COD; target callback and speech summary are required for real integration; D-10 candidate is not production-locked.
