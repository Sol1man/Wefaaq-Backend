# Deployment Guide — User Payments Account Feature

> **Feature branch:** `feature/user_payments_account_v2` (backend + frontend)
> **Migration ID:** `20260510153613_AddPaymentTypeAndAccountAmounts`
> **Azure SQL script:** [`azure_migration_2026-05-16.sql`](./azure_migration_2026-05-16.sql)
> **Previous production migration:** `20260411151016_AddClientOperations` (applied 2026-04-20)

---

## What this deploys

A unified payment-management page where:
- Each user has an **InitialAccountAmount** (cumulative top-ups) and a **CurrentAccountAmount** (running balance).
- Every payment row is classified by `Type`: **Payment** (0) deducts from balance, **Profit** (1) is logged but doesn't deduct, **Initial** (2) is an admin top-up that adds to both balances.
- Admin top-ups are logged as `UserPayments` rows of `Type=Initial` for full audit traceability.
- The popup that admins use to top up an account is labeled "Top Up Account" and is cumulative — the entered amount is **added**, not "set as new value".
- Selected filter chips display with solid primary-blue fill; the "Current Account Amount" card uses solid primary-blue (no more gradient).

Full feature list:
- 6 summary cards (Initial, Current, Today's Payment, Today's Profit, Month Payment, Month Profit) — react to user-dropdown filter.
- Type filter with 4 chips: All / Payment / Profit / Initial.
- Date filter: All / Today / This Month + custom range picker.
- Per-user details accessible via `/payments/user/:userId` (pre-selects that user in the dropdown).
- Single dialog handles all payment submissions: payment amount + profit amount + optional description, with double-click guard.
- All times rendered in `Asia/Riyadh` regardless of viewer's browser timezone.

---

## Does this need DB changes?

**Yes.** Production currently sits on migration `20260411151016_AddClientOperations`. Three new columns + two indexes + one FK need to be added to support the feature.

| Object | Change | Backfill |
|---|---|---|
| `Users.InitialAccountAmount` | new `DECIMAL(18,2) NOT NULL DEFAULT 0` | All existing users get `0` |
| `Users.CurrentAccountAmount` | new `DECIMAL(18,2) NOT NULL DEFAULT 0` | All existing users get `0` |
| `UserPayments.Type` | new `INT NOT NULL DEFAULT 0` | All existing rows get `Type = 0` (= Payment) — preserves old semantics exactly |
| `UserPayments.RelatedPaymentId` | new `UNIQUEIDENTIFIER NULL` | All existing rows get `NULL` |
| `IX_UserPayments_Type` | new index | — |
| `IX_UserPayments_RelatedPaymentId` | new index | — |
| `FK_UserPayments_UserPayments_RelatedPaymentId` | new self-FK | — |

**Safe — no data loss.** Defaults cover every existing row.

> The new enum value `Initial = 2` does **not** need a DB change. The `Type` column is `INT` with no constraint, so any int can be stored. The enum is enforced in C# only.

---

## Pre-deploy checklist

### On your machine
- [ ] On `feature/user_payments_account_v2`, both repos, working tree clean
  ```powershell
  cd "D:\Wefaaq Project\Wefaaq Backend"; git status
  cd "D:\Wefaaq Project\Wefaaq_front"; git status
  ```
- [ ] Backend builds: `dotnet build "Wefaaq Backend/Wefaaq.sln" --configuration Release`
- [ ] Frontend builds: `cd "Wefaaq_front"; npx ng build --configuration=production`
- [ ] Run the new feature locally with the dev DB to smoke-test:
  - Top up a user from the dialog → verify a new `Type=Initial` row appears in the table with `+` and blue tag.
  - Submit a payment with both Payment and Profit values → verify two linked rows appear (Profit's `RelatedPaymentId` matches Payment's `Id`).
  - Delete an Initial row → verify both Initial and Current balance decrease by the row's amount.
  - Delete a Payment row → verify the Current balance is refunded by the row's amount.
  - Switch language to AR → verify the dialog and tags display correctly in Arabic.

### Production prerequisites
- [ ] Confirm production's last migration is `20260411151016_AddClientOperations`:
  ```sql
  SELECT TOP 1 MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId DESC;
  -- expected: 20260411151016_AddClientOperations
  ```
- [ ] Capture a database backup (Azure SQL portal → Database → "Restore" tab shows recent backups; trigger a manual snapshot if you want a known good point).
- [ ] Confirm Railway has the latest commit pushed to whichever branch auto-deploys to production (master).

---

## Deployment order

> **Run the DB migration BEFORE the new code starts.** If the new code starts first against the old schema, it will fail when querying `Users.InitialAccountAmount` / `UserPayments.Type` and could enter a crash loop. If the DB is migrated first and the old code is still running for a few seconds, the old code is unaffected (it doesn't read those columns).

### Step 1 — Apply the migration on Azure SQL

Open Azure Portal → SQL databases → `WefaaqDb_Prod` → **Query editor (preview)**.
Sign in with the SQL credentials and paste the contents of [`azure_migration_2026-05-16.sql`](./azure_migration_2026-05-16.sql). Click **Run**.

The script is **idempotent**: it checks `__EFMigrationsHistory` and `sys.columns`/`sys.indexes`/`sys.foreign_keys` before each `ALTER`/`CREATE`. Re-running it is safe.

Expected output:
```
Migration 20260510153613_AddPaymentTypeAndAccountAmounts applied successfully
Summary:
  + Users.InitialAccountAmount  decimal(18,2) NOT NULL default 0
  + Users.CurrentAccountAmount  decimal(18,2) NOT NULL default 0
  + UserPayments.Type           int NOT NULL default 0 (existing rows = Payment)
  + UserPayments.RelatedPaymentId  uniqueidentifier NULL
  + IX_UserPayments_Type, IX_UserPayments_RelatedPaymentId
  + FK_UserPayments_UserPayments_RelatedPaymentId (self-FK)
```

Verify:
```sql
SELECT MigrationId FROM __EFMigrationsHistory
WHERE MigrationId = '20260510153613_AddPaymentTypeAndAccountAmounts';
-- 1 row expected

SELECT name, system_type_id, is_nullable
FROM sys.columns
WHERE object_id = OBJECT_ID('UserPayments')
  AND name IN ('Type', 'RelatedPaymentId');
-- 2 rows expected
```

### Step 2 — Merge feature branch through development → master

```powershell
# Backend
cd "D:\Wefaaq Project\Wefaaq Backend"
git checkout development
git pull
git merge feature/user_payments_account_v2
git push

git checkout master
git pull
git merge development
git push          # ← triggers Railway auto-deploy

# Frontend (same flow)
cd "D:\Wefaaq Project\Wefaaq_front"
git checkout development
git pull
git merge feature/user_payments_account_v2
git push

git checkout master
git pull
git merge development
git push
```

Railway auto-deploys the backend when `master` updates. Frontend deployment depends on your frontend host — kick it manually if it doesn't auto-deploy.

### Step 3 — Watch Railway deploy logs

Railway dashboard → API service → **Deploy logs**. Look for:
- `? Firebase initialized successfully …`
- No `Stopping Container` shortly after start
- No errors mentioning `InvalidColumnException` / `Invalid column name 'Type'` (would mean DB migration didn't run)

Then hit any payment endpoint and confirm you see the diagnostic log lines:
```
info: Wefaaq.Api.Controllers.UserPaymentController[0]
      [UserPayments] HTTP POST /add traceId=… userId=… amount=… ip=…
info: Wefaaq.Bll.Services.UserPaymentService[0]
      [UserPayments] CreateAsync ENTRY userId=… amount=… descriptionLength=…
info: Wefaaq.Bll.Services.UserPaymentService[0]
      [UserPayments] CreateAsync SAVED paymentId=… userId=… amount=…
```

If `Information` lines don't appear, check that [`appsettings.Production.json`](../Wefaaq.Api/appsettings.Production.json) has `"Default": "Information"` (this branch should already include that change).

---

## Post-deploy verification

### Backend sanity
- [ ] `GET /api/user-payments/user-summaries` returns 200 with `initialAccountAmount`, `currentAccountAmount`, `todaysProfit`, `currentMonthProfit` fields populated for every user.
- [ ] `GET /api/user-payments/get-all` includes `type` and `relatedPaymentId` on every row.

### Frontend smoke test (in production)
- [ ] Navigate to `/payments` → page loads, 6 cards visible, table populated.
- [ ] Switch the user dropdown → cards update to that user's totals.
- [ ] Click **Top Up Account** → dialog opens with field labeled "Amount to Add", default empty, optional description.
- [ ] Submit `100 SAR` → success toast, new row in the table with blue **Initial** tag and `+100 SAR` in primary blue. The user's `Current Account Amount` card increases by 100.
- [ ] Submit a payment of `30 SAR` from the pay popup → red row in the table, current decreases by 30, today's payments increases by 30.
- [ ] Click a type filter chip (Payment / Profit / Initial) → button fills with primary blue + white text; table filters correctly.
- [ ] Delete an Initial row → balance returns to prior state. Confirm via card values.
- [ ] Switch language to Arabic → "إيداع" / "ربح" / "دفع" appear correctly; dialog hint reads in Arabic.

### Data sanity check via SQL
```sql
-- Every user has the new columns
SELECT TOP 10 Id, Email, InitialAccountAmount, CurrentAccountAmount
FROM Users
ORDER BY Id DESC;

-- Every existing payment row got Type = 0 (Payment)
SELECT [Type], COUNT(*) AS Rows
FROM UserPayments
GROUP BY [Type];
-- Expected before first top-up: Type=0 → all existing rows
-- After top-ups: Type=2 → new top-up rows

-- After a test top-up, the Initial row exists and balances match
SELECT TOP 5 Id, UserId, Amount, [Type], CreatedAt
FROM UserPayments
WHERE [Type] = 2
ORDER BY CreatedAt DESC;
```

---

## Rollback

### If you spot trouble before users start submitting

```powershell
# 1. Revert the master merge
cd "D:\Wefaaq Project\Wefaaq Backend"
git checkout master
git revert -m 1 HEAD     # if you used --no-ff merge, this reverts the merge commit
git push                 # Railway redeploys the old code

# Same on Wefaaq_front.
```

The DB migration **can stay** — the old code doesn't read the new columns, so the new schema is harmless to it.

### If you must also reverse the schema

The reverse SQL is essentially the migration's `Down()` method translated to idempotent T-SQL. Only do this if no top-ups have been created since deploy — once Initial rows exist with linked profits, dropping `RelatedPaymentId` loses that linkage permanently.

```sql
-- Only after confirming no Initial rows / linked profits exist
ALTER TABLE [UserPayments] DROP CONSTRAINT [FK_UserPayments_UserPayments_RelatedPaymentId];
DROP INDEX [IX_UserPayments_RelatedPaymentId] ON [UserPayments];
DROP INDEX [IX_UserPayments_Type] ON [UserPayments];
ALTER TABLE [UserPayments] DROP COLUMN [RelatedPaymentId];
ALTER TABLE [UserPayments] DROP CONSTRAINT [DF_UserPayments_Type];
ALTER TABLE [UserPayments] DROP COLUMN [Type];
ALTER TABLE [Users] DROP CONSTRAINT [DF_Users_CurrentAccountAmount];
ALTER TABLE [Users] DROP COLUMN [CurrentAccountAmount];
ALTER TABLE [Users] DROP CONSTRAINT [DF_Users_InitialAccountAmount];
ALTER TABLE [Users] DROP COLUMN [InitialAccountAmount];
DELETE FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260510153613_AddPaymentTypeAndAccountAmounts';
```

---

## Known behavioral changes

1. **Top-up is cumulative, not "set".** Pre-feature behavior was "set the initial value to N, reset current". Now the dialog accepts an amount to **add** on top of the existing balance, and logs a row in `UserPayments`.
2. **Existing users start at 0 / 0.** When the migration runs, every existing user gets `InitialAccountAmount = 0` and `CurrentAccountAmount = 0`. Admin must explicitly top up each user to set a starting balance.
3. **Old `UserPayments` rows are all classified as Payment.** Default for the new `Type` column is `0`, so historical entries continue to behave as deductions in any aggregation that filters by `Type`.
4. **Profit doesn't affect balance.** This was already the design — restating for the deploy log.
5. **Deleting a row reverses its balance impact.** Payment deletion refunds Current; Initial deletion subtracts from both Initial and Current; Profit deletion does nothing to balances.

---

## Quick reference — files touched on this branch

### Backend
- `Wefaaq.Dal/Entities/UserPaymentType.cs` — adds `Initial = 2`
- `Wefaaq.Dal/Entities/UserPayment.cs` — adds `Type`, `RelatedPaymentId`, nav
- `Wefaaq.Dal/Entities/User.cs` — adds `InitialAccountAmount`, `CurrentAccountAmount`
- `Wefaaq.Dal/WefaaqContext.cs` — model configuration + EF UTC value converters
- `Wefaaq.Dal/Conventions/UtcDateTimeConverter.cs` — new
- `Wefaaq.Dal/Migrations/20260510153613_AddPaymentTypeAndAccountAmounts.cs` — schema migration
- `Wefaaq.Bll/Services/UserPaymentService.cs` — cumulative top-up, Initial row, balance math
- `Wefaaq.Bll/Interfaces/IUserPaymentService.cs` — signature update
- `Wefaaq.Bll/DTOs/UserPaymentDto.cs` — new fields, `UserPaymentSummaryDto`, `UpdateUserAccountAmountDto.Description`
- `Wefaaq.Bll/Mappings/MappingProfile.cs` — relatedPayment ignore
- `Wefaaq.Bll/Validators/UserPaymentValidators.cs` — description optional, type rules
- `Wefaaq.Api/Controllers/UserPaymentController.cs` — `add-operation`, `user-summaries`, `set-initial-amount`, logging
- `Wefaaq.Api/appsettings.Production.json` — default log level → `Information`

### Frontend
- `src/app/shared-files/services/payment.service.ts` — types + endpoints
- `src/environments/api-url.ts` — endpoint URLs
- `src/app/shared-files/components/payment-dialog/*` — combined Payment + Profit + optional description, double-click guard, logging
- `src/app/shared-files/interceptors/token-refresh.interceptor.ts` — `take(1)` + diagnostic logging
- `src/app/shared-files/core/auth/*` — Firebase hydration fix, session policy, idle warning
- `src/app/shared-files/functions/business-timezone.ts` — new
- `src/app/admin-end/modules/payments/component/payments-management/*` — unified view, 6 cards, filters, Initial tag, filled selected chips, solid primary card
- `src/app/admin-end/modules/payments/component/set-account-amount-dialog/*` — Top-Up dialog (cumulative)
- `public/i18n/en.json`, `public/i18n/ar.json` — new translation keys

---

*Document created: 2026-05-16. Update the date and previous-migration reference when the next migration ships.*
