# Deployment Guide — Employee Salaries Feature

> **Feature branch:** `feature/employee_salaries` (backend + frontend)
> **Migration ID:** `20260905164935_AddEmployeeSalaries`
> **Azure SQL script:** [`azure_migration_2026-09-05.sql`](./azure_migration_2026-09-05.sql)
> **Previous production migration:** `20260810120000_AddCostsTable`

---

## What this deploys

### 1. Employee salaries page (رواتب الموظفين) — new

A monthly payroll view at `/employee-salaries`, following the same page template as Costs and Payments.

- **Who is an employee:** every active system user, plus **external employees** — payroll-only people with a name and a salary, no account, no email, no role, no login.
- **Columns:** Name / Salary / Profit / Deduction / Net, with 4 summary cards (total salaries, profits, deductions, net) and an Excel export.
- **Period filter:** Year + Month dropdowns. Neither offers a future period — the month list stops at the current month for the current year.
- **Profit** = that user's own `Profit`-type `UserPayments` rows for the month × their `ProfitPercentage`. External employees have no payment history, so their profit is always 0.
- **Details page** per employee: employee info, salary breakdown, and that month's deductions newest-first. Salary is set and deductions are added/removed here — the roster itself carries only "Add External Employee".
- **Role split:** admins see the full roster. A plain user opening the page is redirected by a route guard straight to their own read-only row (`/employee-salaries/my-salary`) with every mutating action removed.

### 2. Client payment description (البيان) — change to existing behavior

A client payment (`Kind = Payment`) now accepts an **optional free-text description**, exactly like a service of type `Other`. It is stored in the existing `ClientOperation.CustomType` column and shown in the **البيان** column in place of the default `"دفعة من العميل"`. Leave it blank and the row reads exactly as it does today.

### 3. Styled Excel export — change to existing behavior

`csv-export.ts` reworked so every exported sheet gets a merged title banner, a colored bold header, bordered and zebra-striped rows, auto-sized columns, and a frozen header row. `ExportStats` gains an optional `title` field for the banner.

The **Summary sheet now follows the UI language** instead of being hard-coded English — labels run through `translate` on the Costs, Payments, and Client Operations pages, so an Arabic session exports an Arabic summary. Affects every existing export button in the app.

> **Not in this deployment:** the admin reset-password endpoint lives on its own
> branch, `feature/admin_reset`, and ships separately.

---

## Does this need DB changes?

**Yes** — for the salaries feature only. The other two need no schema change: the payment description reuses the existing `CustomType` column, and the export rework is entirely client-side.

Production currently sits on `20260810120000_AddCostsTable`.

| Object | Change | Backfill |
|---|---|---|
| `Users.Salary` | new `DECIMAL(18,2) NOT NULL DEFAULT 0` | All existing users get `0` |
| `ExternalEmployees` | new table (`Id`, `Name`, `Salary`, timestamps, soft-delete) | Empty |
| `IX_ExternalEmployees_Name` | new index | — |
| `EmployeeDeductions` | new table (`Id`, `Amount`, `Description`, `DeductionDate`, `UserId?`, `ExternalEmployeeId?`, timestamps, soft-delete) | Empty |
| `CK_EmployeeDeduction_User_XOR_External` | new check constraint — exactly one owner FK must be set | — |
| `FK_EmployeeDeductions_Users_UserId` | new FK (no cascade) | — |
| `FK_EmployeeDeductions_ExternalEmployees_ExternalEmployeeId` | new FK (cascade) | — |
| `IX_EmployeeDeductions_UserId`, `_ExternalEmployeeId`, `_DeductionDate` | new indexes | — |

**Safe — no data loss.** One additive column with a default, two brand-new tables. Nothing existing is altered or dropped.

---

## Pre-deploy checklist

### On your machine
- [ ] Both repos on `feature/employee_salaries`, working tree clean
  ```powershell
  cd "D:\Projects\Wefaaq Project\Wefaaq Backend"; git status
  cd "D:\Projects\Wefaaq Project\Wefaaq_front"; git status
  ```
- [ ] Backend builds: `dotnet build "Wefaaq Backend\Wefaaq.sln" --configuration Release`
- [ ] Frontend builds: `cd "Wefaaq_front"; npx ng build --configuration production`
- [ ] Smoke-test locally against the dev DB:
  - Open `/employee-salaries` as an admin → every active user is listed, salary/profit/deduction/net populated.
  - Add an external employee → appears in the list with the **External** badge and 0 profit.
  - Open a details page → Set Salary, then Add Deduction → the roster's Deduction and Net columns update for that month.
  - Delete a deduction → totals revert.
  - Switch Year to a past year → all 12 months offered. Switch back to this year → months stop at the current one.
  - Log in as a plain **User** → clicking رواتب الموظفين lands on their own row with **no flash of the admin roster**, and no Set Salary / Add Deduction / delete controls.
  - Record a client payment with a description → the البيان column shows the text. Record one without → it reads `دفعة من العميل`.
  - Export from **Costs, Payments, Client Operations and Salaries** → each file opens in Excel with a title banner, colored header and zebra rows; in an Arabic session the Summary sheet is Arabic.

### Production prerequisites
- [ ] Confirm production's last migration:
  ```sql
  SELECT TOP 1 MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId DESC;
  -- expected: 20260810120000_AddCostsTable
  ```
- [ ] Capture a database backup (Azure Portal → SQL database → Restore tab; trigger a manual snapshot for a known good point).
- [ ] Confirm Railway is watching `master`.

---

## Deployment order

> **Run the DB migration BEFORE the new code starts.** If the new code starts against the old schema it will fail the moment anyone opens the salaries page (`Invalid column name 'Salary'`, `Invalid object name 'ExternalEmployees'`) and can enter a crash loop. Migrating the DB first is harmless to the currently running old code — it never reads the new column or tables.

### Step 1 — Apply the migration on Azure SQL

Azure Portal → SQL databases → `WefaaqDb_Prod` → **Query editor (preview)**. Paste the contents of [`azure_migration_2026-09-05.sql`](./azure_migration_2026-09-05.sql) and click **Run**.

The script is **idempotent** — it checks `__EFMigrationsHistory`, then `sys.columns` / `sys.tables` / `sys.indexes` before each change, and contains no `GO` statements (Azure Query Editor rejects those). Re-running it is safe.

Expected output:
```
Migration 20260905164935_AddEmployeeSalaries applied successfully
Summary:
  + Users.Salary (decimal(18,2), default 0)
  + ExternalEmployees table + IX_ExternalEmployees_Name
  + EmployeeDeductions table + XOR check constraint + 3 indexes
```

Verify:
```sql
SELECT MigrationId FROM __EFMigrationsHistory
WHERE MigrationId = '20260905164935_AddEmployeeSalaries';
-- 1 row expected

SELECT name FROM sys.tables
WHERE name IN ('ExternalEmployees','EmployeeDeductions');
-- 2 rows expected

SELECT name FROM sys.columns
WHERE object_id = OBJECT_ID('Users') AND name = 'Salary';
-- 1 row expected
```

### Step 2 — Merge feature branch through development → master

```powershell
# Backend
cd "D:\Projects\Wefaaq Project\Wefaaq Backend"
git checkout development
git pull
git merge feature/employee_salaries
git push

git checkout master
git pull
git merge development
git push          # ← triggers Railway auto-deploy

# Frontend (same flow)
cd "D:\Projects\Wefaaq Project\Wefaaq_front"
git checkout development
git pull
git merge feature/employee_salaries
git push

git checkout master
git pull
git merge development
git push          # ← triggers Vercel auto-deploy
```

Railway auto-deploys the backend from `master` via the Dockerfile. Vercel builds the frontend with `ng build --configuration production` → `dist/wefaaq-admin/browser` (see [`vercel.json`](../../Wefaaq_front/vercel.json)). Kick either manually if auto-deploy doesn't fire.

### Step 3 — Watch the deploy logs

Railway dashboard → API service → **Deploy logs**. Look for:
- `Firebase initialized successfully …`
- No `Stopping Container` shortly after start
- No `Invalid column name 'Salary'` / `Invalid object name 'ExternalEmployees'` — either would mean Step 1 didn't run

---

## Post-deploy verification

### Backend sanity
- [ ] `GET /api/employee-salaries/by-month?year=2026&month=9` as an admin → 200, one row per active user, each with `salary`, `profit`, `deduction`, `net`.
- [ ] Same call as a plain **User** → **403**. This is the security boundary, not the hidden buttons.
- [ ] `GET /api/employee-salaries/my-details?year=2026&month=9` as a plain User → 200 with only their own row.
- [ ] `GET /api/client-operations/get-all` → payment rows carry `customType` and a `typeDisplay` matching it when set.

### Frontend smoke test (in production)
- [ ] As admin: `/employee-salaries` loads, 4 cards, table populated, Year/Month dropdowns offer no future period.
- [ ] Add an external employee → appears with the External badge, profit 0.
- [ ] Set a salary and add a deduction → roster totals update.
- [ ] As a plain user: the nav item lands directly on their own row — **no flash of the admin roster**, no mutating actions, no Actions column in the deductions table.
- [ ] Record a client payment with a description → البيان shows the text; the green **دفعة عميل** badge in النوع is unchanged.
- [ ] Switch language to Arabic → all new labels render (رواتب الموظفين، الراتب، الخصومات، صافي الراتب، راتبي).
- [ ] **Regression — every export button still works.** Export from Costs, Payments and Client Operations; each opens in Excel with two sheets, a title banner and styled rows, and the Summary sheet matches the UI language.

### Data sanity check via SQL
```sql
-- Every user got the new column, defaulted to 0
SELECT TOP 10 Id, Email, Salary FROM Users ORDER BY Id DESC;

-- After adding a test external employee + deduction
SELECT * FROM ExternalEmployees WHERE IsDeleted = 0;

SELECT d.DeductionDate, d.Amount, d.Description, d.UserId, d.ExternalEmployeeId
FROM EmployeeDeductions d
WHERE d.IsDeleted = 0
ORDER BY d.DeductionDate DESC;
-- exactly one of UserId / ExternalEmployeeId is non-null on every row
```

---

## Rollback

### Code only (the usual case)

```powershell
cd "D:\Projects\Wefaaq Project\Wefaaq Backend"
git checkout master
git revert -m 1 HEAD     # reverts the merge commit
git push                 # Railway redeploys the old code

# Same on Wefaaq_front.
```

**Leave the DB migration in place.** The old code never reads `Users.Salary` or the two new tables, so the new schema is completely inert to it.

### If you must also reverse the schema

Only worth doing if no salaries or deductions have been entered — dropping the tables destroys them permanently.

```sql
DROP TABLE [EmployeeDeductions];
DROP TABLE [ExternalEmployees];
ALTER TABLE [Users] DROP CONSTRAINT [DF_Users_Salary];
ALTER TABLE [Users] DROP COLUMN [Salary];
DELETE FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260905164935_AddEmployeeSalaries';
```

---

## Known behavioral changes

1. **Every user starts at salary 0.** The migration defaults `Users.Salary` to 0, so the roster shows 0 for everyone until an admin sets each salary from the details page.
2. **Salary is not historical.** It is a single current value per employee, so changing the month changes profit and deductions but shows the same salary. Per-month salary snapshots would need a further schema change.
3. **A deduction's month is the month of its `DeductionDate`,** which defaults to today. Adding a deduction while viewing an older month lands it in the *current* month unless the date is changed in the dialog.
4. **External employees never show profit.** Profit is derived from `UserPayments`, and they have none.
5. **The salaries page is now visible to plain users** in the nav — but only ever as their own read-only row.
6. **Client payments display their description** in البيان when one is entered. Existing payment rows have `CustomType = NULL` and are unaffected.

---

## Quick reference — files touched on this branch

### Backend
- `Wefaaq.Dal/Entities/ExternalEmployee.cs` — new
- `Wefaaq.Dal/Entities/EmployeeDeduction.cs` — new
- `Wefaaq.Dal/Entities/User.cs` — adds `Salary`
- `Wefaaq.Dal/Entities/ClientOperation.cs` — `CustomType` doc comment covers the payment description
- `Wefaaq.Dal/WefaaqContext.cs` — two DbSets, model config, XOR constraint, timestamp handling
- `Wefaaq.Dal/Migrations/20260905164935_AddEmployeeSalaries.cs` (+ `.Designer.cs`, snapshot)
- `Wefaaq.Bll/DTOs/EmployeeSalaryDto.cs` — new
- `Wefaaq.Bll/Interfaces/IEmployeeSalaryService.cs` — new
- `Wefaaq.Bll/Services/EmployeeSalaryService.cs` — new
- `Wefaaq.Bll/Validators/EmployeeSalaryValidators.cs` — new
- `Wefaaq.Bll/DTOs/ClientOperationDto.cs` — payment `Description`
- `Wefaaq.Bll/Services/ClientOperationService.cs` — stores the description, uses it as the display label
- `Wefaaq.Bll/Mappings/MappingProfile.cs` — employee mappings
- `Wefaaq.Api/Controllers/EmployeeSalaryController.cs` — new (admin-only except `my-details`)
- `Wefaaq.Api/Program.cs` — DI registration
- `docs assets/azure_migration_2026-09-05.sql` — new

### Frontend
- `src/app/admin-end/modules/employee-salaries/**` — new module (roster, details, 3 dialogs, route guard)
- `src/app/shared-files/services/employee-salary.service.ts` — new
- `src/environments/api-url.ts` — endpoint URLs
- `src/app/shared-files/data/app-routes.ts` — local routes
- `src/app/app-routing.module.ts` — lazy route, `[Admin, User]`
- `src/app/shared-files/core/navigation/navigation.data.ts` — nav entries
- `src/app/admin-end/modules/client-operations/**` — payment description (model, form, details page)
- `public/i18n/en.json`, `public/i18n/ar.json` — 63 new keys

---

*Document created: 2026-09-05. Update the date and previous-migration reference when the next migration ships.*
