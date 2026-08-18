# Blog API (Backend)

.NET 10 · CQRS (MediatR) · Onion Architecture · EF Core · MSSQL · JWT · Swagger

> Öğrenme rehberi (fazlar): üst klasördeki `README.md` (yerel monorepo notları; bu repo yalnızca backend).

## Özellikler

- Public: yazı / kategori listesi ve detay (anonim)
- Admin: JWT ile yazı/kategori CRUD + `GET /api/posts/admin` (taslaklar dahil)
- Yazı yayınlanınca: uygulama içi bildirim + e-posta (dev’de log)
- Admin bildirim listesi: `GET /api/notifications`
- Seed (Development): kategori **Genel**, örnek yazılar, admin kullanıcı (şifre User Secrets içindeki `Seed:AdminPassword`)

## Gereksinimler

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- MSSQL (`BlogDb`) — Windows Auth veya connection string

## Kurulum

```bash
cd backend

dotnet restore

# Local secret'lar (proje klasöründe değil; GitHub'a gitmez)
# Visual Studio: Blog.API sağ tık → Manage User Secrets
dotnet user-secrets set "Jwt:Key" "REPLACE_WITH_YOUR_OWN_LONG_RANDOM_JWT_KEY_MIN_32" --project src/Blog.API
dotnet user-secrets set "Seed:AdminUsername" "admin" --project src/Blog.API
dotnet user-secrets set "Seed:AdminPassword" "change-me-local-only" --project src/Blog.API

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
| GET | `/api/posts?page&pageSize&search&categoryId`, `/api/posts/{slug}` | Anonim |
| GET | `/api/posts/admin?page&pageSize&search&categoryId&isPublished`, `/api/posts/admin/{id}` | JWT |
| GET | `/api/notifications` | JWT |
| POST | `/api/notifications/{id}/read` | JWT |
| POST/PUT/DELETE | `/api/posts`… | JWT |
| GET | `/api/categories`… | Anonim |
| POST/PUT/DELETE | `/api/categories`… | JWT |

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
