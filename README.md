Roommate Expense Splitter (Backend)
A backend API for splitting shared expenses between roommates, inspired by apps like Splitwise.
Built with .NET 8, clean domain logic, and fully testable money calculations.
This project focuses on correct financial logic, clean architecture, and API-first design.
A frontend UI is planned for a later phase.


Features
* Create groups
* Add expenses (equal split with correct rounding)
* Track who paid and who owes
* Record payments between users
* Automatically calculate balances (who owes whom)
* Suggest settlement transfers
* Fetch a full group summary in one API call
* In-memory repositories (easy to replace with DB later)
* Unit-tested domain logic

Architecture Overview
The solution is structured using a clean separation of concerns:

rm-splitter/
├─ RoommateSplitter.Domain        * Core business logic (pure C#)
│  ├─ Expenses                    * Expense + split logic
│  ├─ Payments                    * Payment domain model
│  └─ Balances                    * Balance calculation + settlements
│
├─ RoommateSplitter.Domain.Tests  * xUnit tests for domain logic
│
└─ backend/RoommateSplitter.Api   * ASP.NET Web API
   ├─ Controllers                 * HTTP endpoints
   ├─ Contracts                   * Request/response DTOs
   └─ Repositories                * In-memory persistence

Key principles
* Domain logic is framework-agnostic
* API layer only orchestrates and maps data
* Financial calculations are fully unit-tested
* No UI or persistence assumptions baked into the domain

💰 How balances are calculated
For each group:
1. Expenses
    * Payer gets credited the full amount
    * Each participant gets debited their share
2. Payments
    * FromUserId gets credited (they owe less)
    * ToUserId gets debited (they are owed less)
3. Net balance
    * Positive → user should receive money
    * Negative → user owes money
4. Settlement suggestions
    * Debtors are matched to creditors
    * Minimal set of transfers is generated

All calculations use decimal and proper rounding.

API Endpoints
    * Health
        --> GET /api/health
    * Groups 
        --> POST /api/groups
        --> GET  /api/groups
        --> GET  /api/groups/{groupId}
    * Expenses
        --> POST /api/groups/{groupId}/expenses
        --> GET  /api/groups/{groupId}/expenses
    * Payments
        --> POST /api/groups/{groupId}/payments
        --> GET  /api/groups/{groupId}/payments
    * Balances
        --> GET /api/groups/{groupId}/balances
Returns:
* net balances per user
* suggested settlement transfers

Group Summary (UI-friendly)
    --> GET /api/groups/{groupId}/summary

Returns in one call:
    * group info
    * expenses
    * payments
    * balances + settlement suggestions
This endpoint is intended to power a future frontend.

Testing
All money logic is tested in the domain layer using xUnit:
    * Equal split calculations
    * Multiple expenses
    * Payments reducing balances
    * Overpayment edge cases
    * Net balance correctness

Tech Stack
    * .NET 8
    * ASP.NET Core Web API
    * xUnit
    * Swagger / OpenAPI
    * In-memory repositories (no DB yet)

Future Work
    * Frontend UI (React)
    * User & authentication support
    * Persistent database (PostgreSQL)
    * Currency support
    * Custom split types

Author
Built as a personal portfolio project to demonstrate:
    * backend architecture
    * domain-driven thinking
    * financial correctness
    * testable business logic






