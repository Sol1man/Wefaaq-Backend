# Deployment Guide — User Permissions (Phase 1)

> **Feature branch:** `feature/user_permissions` (backend + frontend)
> **Migration ID:** `20261118202228_AddUserPagePermissions`
> **Azure SQL script:** [`azure_migration_2026-11-18.sql`](./azure_migration_2026-11-18.sql)
> **Previous production migration:** `20261013095731_AddRents`

---

## What this deploys

### 1. User Permissions page (صلاحيات المستخدمين) — new, admin only

`/user-permissions`, under a new **إدارة النظام** group in the side menu (admins only).

- Pick a user from the searchable list, then tick **View / Add-Edit / Delete** for each page and **Save**.
- Unticking **View** clears the other two; ticking Add-Edit or Delete ticks View.
- Cells shown as **—** are actions that stay admin-only on that page (hover: "متاح للمدير فقط").
- **Reset to defaults** removes a user's custom permissions.
- Admins always have full access; their row is read-only.
- Each user shows a badge: **صلاحيات افتراضية** (never customized) or **صلاحيات مخصصة**.
- Changes hit the API on the user's **next request**. Their menu and buttons update on their next navigation, within about a minute, with no re-login needed.

### 2. The permission matrix

| Page | View | Add / Edit | Delete | Default (every existing and new user) |
|---|---|---|---|---|
| بيانات العميل — Clients | ✓ | ✓ | ✓ | View + Add/Edit |
| المؤسسات — Organizations | ✓ | ✓ | ✓ | View + Add/Edit |
| عمليات العملاء — Client operations | ✓ | ✓ | ✓ | View + Add/Edit + Delete |
| المدفوعات — Payments | ✓ (own account) | — | — | View |
| المصروفات — Costs | ✓ | ✓ | ✓ | nothing |
| رواتب الموظفين — Employee salaries | ✓ (own salary) | — | — | View |
| الإيجارات — Rents | ✓ | ✓ | ✓ | View + Add/Edit + Delete |

**The defaults are exactly what the old "User" role could do**, so nothing changes for anyone until an admin edits them. No rows are seeded: a page without a saved row uses its default.

Payments and Employee salaries show a non-admin **only their own data**. Managing other users' payments and salaries stays admin-only; that needs "see all / specific users' data", which is phase 2.

### 3. Enforcement

- **API (the real boundary):** every endpoint now requires a page permission (`[RequirePermission]`) or stays `AdminOnly`. Permissions are re-read from the DB on every request, in the same query that already loaded the user's role. A unit test fails the build if a future endpoint ships with neither.
- **Shared lookups:** the client, branch and organization lists that the Rents / Client operations / Organizations forms use for their dropdowns accept View on **any** of the pages that need them.
- **UI:** the menu, route guards (including the add/edit URLs) and the add/edit/delete buttons all follow the same permissions.

### 4. Behavior changes to know about

1. **Client operations — "كل العملاء" now loads for non-admins.** The `get-all` endpoint was admin-only, so a plain user opening the page got "حدث خطأ أثناء تحميل العمليات" until they picked a client. It now needs View on Client operations. These users could already see the same operations one client at a time.
2. **Deactivated accounts (`Users.IsActive = 0`) are blocked on every request**, not only at sign-in. Before, someone already signed in kept working until their session ended.
3. **First-login fix:** a brand-new account's first sign-in returned an empty role, and the app sent them back to the login page. It now returns the role and default permissions.
4. On the client details page, the operations-history panel only shows for users with View on Client operations. The "Add organization" buttons only show for users with Add/Edit on Organizations, because those buttons open the Organizations form.

---

## Does this need DB changes?

**Yes**: one new, empty table.

| Object | Change | Backfill |
|---|---|---|
| `UserPagePermissions` | new table (`Id`, `UserId`, `PageKey`, `CanView`, `CanAddEdit`, `CanDelete`, `UpdatedAt`) | **Empty on purpose**: no row = page default |
| `FK_UserPagePermissions_Users_UserId` | new FK, cascade delete with the user | — |
| `IX_UserPagePermissions_UserId_PageKey` | new unique index (one row per user per page) | — |

**Safe, no data loss.** Purely additive; nothing existing is altered.

---

## Pre-deploy checklist

### On your machine
- [ ] Both repos on `feature/user_permissions`, changes committed
- [ ] Backend builds and tests pass:
  ```powershell
  cd "D:\Projects\Wefaaq Project\Wefaaq Backend"
  dotnet build Wefaaq.sln --configuration Release
  dotnet test Wefaaq.Tests
  ```
- [ ] Frontend builds: `cd "D:\Projects\Wefaaq Project\Wefaaq_front"; npx ng build --configuration production`
- [ ] Smoke-test locally (the migration auto-applies to the dev DB when the API starts in Development):
  - As admin: **إدارة النظام → صلاحيات المستخدمين** opens; every user is listed; non-admins show **صلاحيات افتراضية**.
  - Pick a user, untick **Clients → Delete** and **Rents → View**, Save. The badge becomes **صلاحيات مخصصة**.
  - Sign in as that user in a private window: no delete buttons on بيانات العميل; الإيجارات is gone from the menu; typing `/rents` redirects to the dashboard with "ليس لديك صلاحية…".
  - Back as admin: tick **Costs → View + Add/Edit**, Save. In the user's window, navigate once: المصروفات appears, with the add button and no delete buttons.
  - **Reset to defaults** → the user is back to the old behavior.
  - Change a checkbox, then click another user → the "unsaved changes" prompt appears.

### Production prerequisites
- [ ] Confirm production's last migration:
  ```sql
  SELECT TOP 1 MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId DESC;
  -- expected: 20261013095731_AddRents
  ```
- [ ] Take a database backup (Azure Portal → SQL database → Restore / manual snapshot).

---

## Deployment order: DB → backend → frontend

> **The DB migration MUST run before the new backend starts.** The new backend reads
> `UserPagePermissions` on every request (and at login). Against the old schema every request
> fails with `Invalid object name 'UserPagePermissions'`, which locks everyone out, admins included.
> The old code never touches the new table, so migrating first is harmless.
>
> Backend ↔ frontend order is forgiving. The old frontend works with the new backend (the defaults
> equal the old roles), and the new frontend falls back to the old role behavior until the new
> backend is live. Still, deploying the backend first and waiting for it to be healthy is cleanest.

### Step 1: Apply the migration on Azure SQL

Azure Portal → SQL databases → `WefaaqDb_Prod` → **Query editor**. Paste [`azure_migration_2026-11-18.sql`](./azure_migration_2026-11-18.sql) and **Run**. The script is idempotent: re-running it is safe, and it contains no `GO` statements.

Expected output:
```
Migration 20261118202228_AddUserPagePermissions applied successfully
Summary:
  + UserPagePermissions table (Id, UserId, PageKey, CanView, CanAddEdit, CanDelete, UpdatedAt)
  + FK_UserPagePermissions_Users_UserId (cascade)
  + IX_UserPagePermissions_UserId_PageKey (unique)
```

Verify:
```sql
SELECT MigrationId FROM __EFMigrationsHistory
WHERE MigrationId = '20261118202228_AddUserPagePermissions';   -- 1 row

SELECT COUNT(*) FROM UserPagePermissions;                      -- 0
```

### Step 2: Backend (merge feature → development → master)

```powershell
cd "D:\Projects\Wefaaq Project\Wefaaq Backend"
git checkout development; git pull; git merge feature/user_permissions; git push
git checkout master;      git pull; git merge development;              git push   # ← Railway deploys
```

Railway → API service → **Deploy logs**: `Firebase initialized successfully …`, no restart loop, no `Invalid object name 'UserPagePermissions'` (that would mean Step 1 didn't run).

### Step 3: Frontend (after the backend is healthy)

```powershell
cd "D:\Projects\Wefaaq Project\Wefaaq_front"
git checkout development; git pull; git merge feature/user_permissions; git push
git checkout master;      git pull; git merge development;              git push   # ← Vercel deploys
```

---

## Post-deploy verification

### Backend
- [ ] `GET /api/auth/me` as any user → the response now has a `permissions` array with 7 pages.
- [ ] `GET /api/user-permissions/overview` as admin → 200 with `pages` + `users`; as a plain user → **403**.
- [ ] A plain user (defaults) calling `DELETE /api/clients/delete/{id}` → **403** (same as before: delete was admin-only).

### Frontend
- [ ] Admin sees **إدارة النظام → صلاحيات المستخدمين**; plain users don't.
- [ ] A plain user with defaults sees exactly the menu and buttons they had before the release.
- [ ] Repeat the "Clients → Delete" / "Rents → View" round-trip from the local smoke test on one real account, then **Reset to defaults**.
- [ ] Arabic and English labels render on the new page.

---

## Rollback

### Code only (usual)
Revert the merge on `master` in both repos and push. The old code ignores the new table, so it can stay. Saved permissions remain stored but unused, and come back if you redeploy.

### Full (also drop the table)
Run the commented **ROLLBACK** block at the end of [`azure_migration_2026-11-18.sql`](./azure_migration_2026-11-18.sql). This deletes every saved permission.

---

## Not in this release (phase 2)

- **Data scope:** "see only own data / all users / specific users" per page. That needs a `CreatedByUserId` on Clients, Organizations, Costs, Rents, ExternalWorkers and ClientBranches, plus filtering on every read path.
- Until then, **Add/Edit on Clients still lets a user remove organizations, branches or workers inside the client edit form**. That was already true for the old User role.
