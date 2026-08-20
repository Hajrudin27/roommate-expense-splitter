# Roommate Expense Splitter API

A backend REST API for splitting shared expenses between roommates, inspired by apps like **Splitwise**.

This project is built with **.NET 8**, follows **Clean Architecture**, and uses a real **PostgreSQL database running in Docker** for persistent storage.

It focuses on:

- Correct financial calculations  
- Clean separation of domain logic  
- Database-backed persistence with EF Core  
- API-first design ready for frontend integration  

---

## Features

- Create roommate groups  
- Add shared expenses with equal split logic  
- Track who paid and who owes  
- Record payments between roommates  
- Automatically calculate balances per user  
- Suggest settlement transfers (minimal transactions)  
- Retrieve full group summaries in one API call  
- PostgreSQL persistence using Docker + EF Core  
- Unit-tested business logic in the domain layer  

---

## Database Setup (PostgreSQL + Docker)

This project uses **PostgreSQL 16** in a Docker container.

### Start the Database

Run:
docker compose up -d

This will create:
* Database: rm_splitter
* User: postgres
* Password: postgres
* Port: 5433 → 5432
A persistent Docker volume is also created:
* rm_splitter_pgdata

Running the API
Once PostgreSQL is running, start the backend API:
cd backend/RoommateSplitter.Api
dotnet run

The API will launch locally and expose Swagger documentation.

Returns everything in one call:
    * Group details
    * Expenses
    * Payments
    * Net balances
    * Settlement suggestions 

## Balance Calculation Logic
The system calculates balances as follows:
    * Expenses
        --> Payer is credited the full amount
        --> Each participant is debited their share
    * Payments
        --> Sender owes less
        --> Receiver is owed less
    * Net Balance
        --> Positive → user should receive money
        --> Negative → user owes money

Settlement Suggestions
* Debtors are matched with creditors to generate the smallest number of transfers.
* All calculations use decimal for financial accuracy.

## Testing
Domain logic is tested with xUnit, including:
    * Equal expense splits
    * Multiple expenses in a group
    * Payments reducing debts
    * Edge cases (overpayment, rounding)
    * Correct settlement suggestions
Run tests with:
dotnet test

## Future Improvements
    --> Frontend UI (React or Angular)
    --> Authentication + user accounts
    --> Custom split types (percent, unequal)
    --> Multi-currency support
    --> Deployment with Docker + Cloud hosting

## Author
Built as a portfolio project to demonstrate:
    --> Clean backend architecture
    --> Domain-driven financial logic
    --> PostgreSQL integration
    --> Testable and scalable API design


