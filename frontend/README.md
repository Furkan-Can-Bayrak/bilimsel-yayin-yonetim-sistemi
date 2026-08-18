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
| http://localhost:4200 | Public makale listesi |
| http://localhost:4200/manuscripts/:slug | Makale detay |
| http://localhost:4200/admin/login | Yönetim girişi |

**Demo hesap:** backend User Secrets içindeki `Seed:AdminEmail` / `Seed:AdminPassword` (Visual Studio: Blog.API → Manage User Secrets).

### API URL

`src/environments/environment.ts` → `apiBaseUrl: 'http://localhost:5254/api'`

Backend CORS politikası `AngularDev` bu origin’e izin verir (`http://localhost:4200`).

## Klasörler

| Yol | Ne? |
|-----|-----|
| `src/environments/` | API base URL |
| `src/app/core/services/` | Auth, Manuscript, ResearchArea HTTP |
| `src/app/core/interceptors/` | JWT `Authorization` header |
| `src/app/core/guards/` | Admin route koruması |
| `src/app/features/manuscripts/` | Public liste + detay |
| `src/app/features/auth/` | Login |
| `src/app/features/admin/` | Makale CRUD + yayın toggle |
| `src/app/app.routes.ts` | URL → sayfa |

## Admin akışı

1. Login → `POST /api/auth/login` (e-posta) → oturum `localStorage` (`byys_session`)
2. Interceptor her isteğe `Authorization: Bearer …` ekler; 401 olursa oturumu kapatır
3. Guard: token yoksa `/admin/login`; izin yoksa ana sayfa
4. Menü ve butonlar `permissions` listesine göre görünür

## GitHub

[blog-frontend-angular21](https://github.com/Furkan-Can-Bayrak/blog-frontend-angular21)
