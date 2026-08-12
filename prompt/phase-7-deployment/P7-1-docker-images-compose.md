# PROMPT P7-1 — Docker Images & Dev Compose

## 0. Meta
| | |
| --- | --- |
| **ID** | `P7-1` · **Phase** 7 — Deployment |
| **Prereq** | `P2-*`, `P3-*` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | Docker (multi-stage) · .NET 10 · Next.js |

## 1. ROLE
Bạn là **Senior Platform/DevOps Engineer**. Bạn đóng gói 3 thành phần IVR thành image Docker nhỏ, an toàn, reproducible (multi-stage, non-root, distroless/chiseled), và dựng compose dev đầy đủ để chạy toàn hệ local. Bạn tối ưu build cache và bảo mật image.

## 2. CONTEXT
Trước khi lên K8s (P7-2), cần image chuẩn cho `ivr-api`, `ivr-worker`, `ivr-admin-ui` + compose dev (api/worker/ui/postgres/otel). Đây là đơn vị triển khai; image sạch = deploy an toàn.

## 3. SOURCE SPECS (đọc trước)
- `specs/tech/00-tech-stack.md`, `specs/architecture/04-deployment-architecture.md`
- `prompt/README-governance.md` §3 (layout), §6 (env ladder)
- `plan/ivr-orther/decisions-log.md` §DTS-04, §DO-06 (health), §D-05 (secret)

## 4. DECISIONS & CONSTRAINTS
- **DTS-04:** 3 deployable riêng (api/worker/ui).
- **Image an toàn:** multi-stage, **non-root**, base tối thiểu (chiseled/distroless cho .NET; node slim/standalone cho Next.js), không secret trong layer, pin version.
- **Health:** expose `/health/*` cho probe (DO-06).
- **12-factor:** config qua env; `REAL_CUSTOMER_CALL_ALLOWED` + `IVR_ADAPTER_MODE` là env, default an toàn.

## 5. INPUTS / DEPENDENCIES
- Build output P0-P6; OTel collector config (P6-1).
- Container registry (`NEED_CONFIRMATION` — theo hạ tầng platform).

## 6. BUILD STEPS
1. **Dockerfile `ivr-api`**: multi-stage (SDK build → chiseled runtime), non-root user, healthcheck, expose port, `ASPNETCORE_*` env.
2. **Dockerfile `ivr-worker`**: tương tự (worker không expose HTTP trừ health/metrics).
3. **Dockerfile `ivr-admin-ui`**: Next.js standalone build, non-root, chỉ static+server cần thiết.
4. **`.dockerignore`** cho mỗi context; tối ưu cache layer (restore trước copy source).
5. **`docker-compose.dev.yml`** (mở rộng P0-1): api+worker+ui+postgres+otel-collector; env dev; volume; healthcheck; mạng nội bộ.
6. **Image scan** (Trivy/Grype) trong build; fail High/Critical (nối P5-4). SBOM (optional).
7. Tài liệu build/run + tag convention (semver + git sha).

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `deploy/docker/Dockerfile.api|worker|ui` | 3 image multi-stage non-root |
| `.dockerignore` (per context) | Giảm context |
| `docker-compose.dev.yml` | Stack dev đầy đủ |
| `deploy/docker/README.md` | Build/run/tag/scan |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `IT-IMG-BUILD-01` | ci | 3 image build pass; non-root (USER ≠ root). |
| `IT-IMG-HEALTH-02` | ci | container healthcheck `/health/live` → healthy. |
| `IT-IMG-COMPOSE-03` | ci | compose up → toàn hệ chạy MOCK, smoke 1 luồng qua. |
| `IT-IMG-SCAN-04` | ci | Trivy scan fail nếu High/Critical; pass khi sạch. |

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] non-root; [ ] base tối thiểu; [ ] không secret trong image; [ ] health probe; [ ] scan gate.
**Reviewer:** cache tối ưu; tag reproducible; env default an toàn (REAL_CALL=NO).

## 10. EVIDENCE EXPECTED
Build log 3 image, non-root proof, compose smoke run, Trivy scan report, image size.

## 11. FORBIDDEN
- ❌ Chạy root. ❌ Secret nướng vào image (D-05). ❌ `REAL_CUSTOMER_CALL_ALLOWED=YES` default. ❌ Base image không pin.

## 12. DEFINITION OF DONE
- [ ] 3 image + compose dev + scan; 4 test §8 xanh; evidence §10 đủ.
