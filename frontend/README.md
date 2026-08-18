# Blog Frontend (Angular 21)

Public blog + admin paneli (JWT). Backend: [.NET Blog API](https://github.com/Furkan-Can-Bayrak/blog-backend-.net).

## Gereksinimler

- Node.js + npm
- Angular CLI 21 (`npm i -g @angular/cli`)
- Çalışan backend: `http://localhost:5254`

## Kurulum

```bash
cd frontend
npm install
ng serve
```

| URL | Ne |
|-----|-----|
| http://localhost:4200 | Public yazı listesi |
| http://localhost:4200/posts/:slug | Yazı detay |
| http://localhost:4200/admin/login | Admin giriş |

**Demo hesap:** backend User Secrets içindeki `Seed:AdminUsername` / `Seed:AdminPassword` (Visual Studio: Blog.API → Manage User Secrets).

### API URL

`src/environments/environment.ts` → `apiBaseUrl: 'http://localhost:5254/api'`

Backend CORS politikası `AngularDev` bu origin’e izin verir (`http://localhost:4200`).

## Klasörler

| Yol | Ne? |
|-----|-----|
| `src/environments/` | API base URL |
| `src/app/core/services/` | Auth, Post, Category HTTP |
| `src/app/core/interceptors/` | JWT `Authorization` header |
| `src/app/core/guards/` | Admin route koruması |
| `src/app/features/posts/` | Public liste + detay |
| `src/app/features/auth/` | Login |
| `src/app/features/admin/` | Yazı CRUD + yayın toggle |
| `src/app/app.routes.ts` | URL → sayfa |

## Admin akışı

1. Login → `POST /api/auth/login` → token `localStorage`
2. Interceptor her isteğe `Authorization: Bearer …` ekler
3. Guard: token yoksa `/admin/login`
4. CRUD + `GET /api/posts/admin` (taslaklar dahil)

## GitHub

[blog-frontend-angular21](https://github.com/Furkan-Can-Bayrak/blog-frontend-angular21)
