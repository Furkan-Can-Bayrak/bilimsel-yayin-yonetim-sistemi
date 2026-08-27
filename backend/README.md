# Blog API (Backend)

.NET 10 · CQRS (MediatR) · Onion Architecture · EF Core · MSSQL · JWT · Swagger

> Öğrenme rehberi (fazlar): üst klasördeki `README.md` (yerel monorepo notları; bu repo yalnızca backend).

## Özellikler

- Public: makale / araştırma alanı listesi ve detay (anonim)
- Yönetim: izin tabanlı JWT ile makale ve araştırma alanı CRUD + `GET /api/admin/manuscripts` (taslaklar dahil)
- Makale yayınlanınca: uygulama içi bildirim + e-posta (dev’de log)
- Bildirim listesi: `GET /api/notifications`
- Seed (Development): araştırma alanı **Bilgisayar Bilimleri**, örnek makaleler, demo kullanıcılar (şifre User Secrets)

## Gereksinimler

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- MSSQL (`BYYS`) — Windows Auth veya connection string

## Kurulum

```bash
cd backend

dotnet restore

# Local secret'lar (proje klasöründe değil; GitHub'a gitmez)
# Visual Studio: Blog.API sağ tık → Manage User Secrets
dotnet user-secrets set "Jwt:Key" "REPLACE_WITH_YOUR_OWN_LONG_RANDOM_JWT_KEY_MIN_32" --project src/Blog.API
dotnet user-secrets set "Seed:AdminEmail" "admin@yayin.local" --project src/Blog.API
dotnet user-secrets set "Seed:AdminPassword" "change-me-local-only" --project src/Blog.API
# Development'ta her açılışta yönetici dahil dört seed hesabı bu şifreye eşitlenir
dotnet user-secrets set "Seed:DemoPassword" "change-me-local-only" --project src/Blog.API

dotnet ef database update --project src/Blog.Infrastructure --startup-project src/Blog.API
dotnet run --project src/Blog.API
```

- API / Swagger: http://localhost:5254/swagger  
- (launchSettings `http` profili)

### Ortam / yapılandırma

Presentation katmanı (`src/Blog.API`):

| Dosya | Commit | İçerik |
|-------|--------|--------|
| `appsettings.json` | Evet | Ortak, secret olmayan ayarlar (`Jwt:Issuer`, `Audience`) |
| `appsettings.Development.json` | Evet | Lokal DB (Windows Auth). Şifre/JWT koyma |
| `appsettings.Production.json` | Evet | Canlı log seviyesi. Secret yok |

Hassas veri:

| Anahtar | Local | Production |
|---------|-------|------------|
| `Jwt:Key` (≥32 karakter) | User Secrets | `Jwt__Key` ortam değişkeni |
| `Seed:AdminPassword` | User Secrets | `Seed__AdminPassword` ortam değişkeni |
| SQL şifresi varsa `ConnectionStrings:DefaultConnection` | User Secrets | `ConnectionStrings__DefaultConnection` |

Visual Studio: **Blog.API** sağ tık → **Manage User Secrets**. Bu dosya `%APPDATA%\Microsoft\UserSecrets\` altındadır, repo’da yoktur.

## API özeti

| Method | Path | Auth |
|--------|------|------|
| POST | `/api/auth/login` | Anonim |
| GET | `/api/manuscripts?page&pageSize&search&researchAreaId`, `/api/manuscripts/{slug}` | Anonim |
| GET | `/api/admin/manuscripts?page&pageSize&search&researchAreaId&status`, `/api/admin/manuscripts/{id}` | Giriş; ViewAll ise tümü, değilse kendi makaleleri |
| POST | `/api/manuscripts/{id}/accept`, `/api/manuscripts/{id}/reject` | `Manuscript.Decide` |
| POST | `/api/manuscripts/{id}/publish` | `Manuscript.Publish` (yalnızca Accepted) |
| POST | `/api/manuscripts/{id}/unpublish` | `Manuscript.Unpublish` (Published → Accepted) |
| GET | `/api/reviews/candidates?manuscriptId` | `Review.Assign` |
| POST | `/api/reviews` | `Review.Assign` |
| GET | `/api/reviews/mine` | `Review.Submit` |
| GET | `/api/reviews/{id}` | Atanan hakem veya `Review.ViewAll` |
| POST | `/api/reviews/{id}/submit` | `Review.Submit` |
| GET | `/api/notifications` | `Notification.View` |
| POST | `/api/notifications/{id}/read` | `Notification.View` |
| POST/PUT/DELETE | `/api/manuscripts`… | İlgili `Manuscript.*` izinleri |
| GET | `/api/research-areas`… | Anonim |
| POST/PUT/DELETE | `/api/research-areas`… | `ResearchArea.Manage` |

Swagger’da **Authorize** → `Bearer <token>`.

## Testler

```bash
dotnet test
```

Örnek: `CreatePostCommandValidator`, `LoginCommandValidator` (FluentValidation).

## Frontend

Ayrı repo: [blog-frontend-angular21](https://github.com/Furkan-Can-Bayrak/blog-frontend-angular21)

## GitHub

[blog-backend-.net](https://github.com/Furkan-Can-Bayrak/blog-backend-.net)
