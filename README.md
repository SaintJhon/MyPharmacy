# MyPharmacy

An ASP.NET Core MVC pharmacy management application built with **.NET 10**, **SQL Server**, **Entity Framework Core**, and **ASP.NET Core Identity**.

The application includes authentication, profile management, user administration, role and permission management, and CRUD modules for products, branches, and suppliers. The user interface and project documentation are in English.

## Requirements

- .NET 10 SDK
- SQL Server LocalDB, SQL Server Express, or a SQL Server instance
- Visual Studio 2026 or Visual Studio Code

## Getting started

1. Extract the ZIP file.
2. Open the `MyPharmacy` folder in Visual Studio or VS Code.
3. Review the connection string in `appsettings.json`.
4. Restore packages and run the application:

```bash
dotnet restore
dotnet build
dotnet run
```

Open `https://localhost:7180` (or the HTTPS URL shown in the terminal). If the local HTTPS certificate is not trusted, run `dotnet dev-certs https --trust` and restart the application. The authentication cookie requires HTTPS.

The application creates a new database named `MyPharmacy_English` and seeds the initial permissions, roles, and demo users on first startup.

> This project uses `EnsureCreated()` for a clean academic/demo database. If you already have a database with an older schema, use a new database name or replace this initialization with EF Core migrations. `EnsureCreated()` does not update an existing schema.

## Demo accounts

| Role | Email | Password |
|---|---|---|
| Administrator | `admin@mypharmacy.local` | `Admin12345` |
| Employee | `employee@mypharmacy.local` | `Employee12345` |

Change these demo passwords before using the application outside a local academic environment.

### Default role permissions

- **Administrator** receives every seeded permission.
- **Employee** receives `products.view`, `products.create`, `products.edit`, `branches.view`, and `suppliers.view`.

You can create additional users and roles from the application.

## Authentication and sign-out

- ASP.NET Core Identity validates the email and password.
- Passwords are stored using Identity's password hasher; plain-text passwords are not stored in the database.
- Successful sign-in issues an authentication cookie.
- The cookie is `HttpOnly`, uses HTTPS, and has a 30-minute sliding expiration.
- Five failed sign-in attempts trigger a 10-minute lockout.
- Sign-out is a POST request protected by an anti-forgery token. `SignInManager.SignOutAsync()` clears the authentication cookie.
- Login and access-denied endpoints are explicitly marked `[AllowAnonymous]`.

## Authorization model

- `ApplicationUser` represents application users and extends Identity.
- `ApplicationRole` represents roles.
- Identity stores user-to-role relationships in `AspNetUserRoles`.
- `Permission` stores the permission catalog.
- `RolePermission` connects roles with multiple permissions.
- `[HasPermission("products.create")]` protects an endpoint with a permission policy.
- `PermissionAuthorizationHandler` checks the current user, verifies that the account is active, and queries the role-permission relationship in the database.
- The fallback authorization policy requires an authenticated, active user for every controller endpoint unless it is explicitly marked `[AllowAnonymous]`. The sign-out endpoint remains available so a deactivated user can still clear their authentication cookie.

Hiding a navigation link is not treated as security. Authorization is enforced on the server for protected actions.

## Included modules

- Account: sign in, sign out, and access denied.
- Profile: view and update personal information; change password.
- Users: list, create, edit, assign roles, and deactivate accounts.
- Roles: list, create, edit, and assign multiple permissions.
- Permissions: view the permission catalog.
- Products: create, view, edit, and delete.
- Branches: create, view, edit, and delete.
- Suppliers: create, view, edit, and delete.

## Permission catalog

- `users.view`, `users.create`, `users.edit`, `users.delete`
- `roles.view`, `roles.create`, `roles.edit`
- `products.view`, `products.create`, `products.edit`, `products.delete`
- `branches.view`, `branches.create`, `branches.edit`, `branches.delete`
- `suppliers.view`, `suppliers.create`, `suppliers.edit`, `suppliers.delete`

## Important testing checklist

1. Sign in with the Administrator account and verify access to all modules.
2. Sign out and confirm that protected pages redirect to the sign-in page.
3. Sign in with the Employee account and verify access to its assigned modules.
4. Try to open a protected action without its permission and confirm that access is denied.
5. Create a role with multiple permissions and edit its permissions.
6. Create a user, assign a role, edit the user, and deactivate a test account.
7. Update the current user's profile and change password.
8. Verify that sign-out is performed through the POST form in the navigation bar.

## Scope and validation note

This ZIP is a complete academic starter implementation for the second-delivery rubric. It has not been compiled or run in this environment because the .NET 10 SDK is unavailable here. Run `dotnet build` and test it against your SQL Server setup before committing it to Git.
