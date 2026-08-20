# Roommate Expense Splitter API

A REST API for splitting shared costs between flatmates and working out who owes who. Built with .NET 8 and PostgreSQL.

## Why I built it

Four of us share a flat and we were tracking rent, groceries and bills in a group chat, which worked about as well as you'd expect. Splitwise exists and does this fine, but I wanted to see whether I could build the logic myself, because the interesting part isn't recording an expense. It's what happens after: once fifteen expenses have been paid by four different people, who actually owes what, and what's the fewest number of transfers that settles it?

I also wanted a project where getting the numbers *nearly* right wasn't good enough. Money is unforgiving that way, which makes it a good thing to practise on.

## What it does

- Create a group and add flatmates to it
- Log an expense, who paid it, and who it's split between
- Record payments between people when they settle up
- Work out each person's net balance
- Suggest which transfers would settle the group
- Return the whole picture for a group in a single call

There's Swagger on the API, so the easiest way to see it is to run it and click around.

## How the balances work

Every expense does two things: it credits the person who paid the full amount, and it debits each participant their share. A payment between two people moves the balance back the other way.

```
Net balance = (what you paid out) - (what you consumed) + (what you sent) - (what you received)

Positive  ->  the group owes you
Negative  ->  you owe the group
```

The invariant that matters is that all the balances have to add up to zero. Money can move between people but it can't appear from nowhere, so if the total isn't zero, something is wrong. I ended up using that as the main test for the whole thing.

## Settling up

This turned out to be the most interesting part of the project.

Once you have everyone's balance, there are lots of combinations of transfers that would settle the group, and most of them involve more transfers than necessary. If four people owe each other in a circle, you don't want four separate MobilePay transfers when one or two would do.

What I implemented is a greedy match: sort the debtors and the creditors, repeatedly take the largest debtor and the largest creditor, and transfer whichever amount is smaller. That clears at least one person per step, so it terminates with at most `n-1` transfers for `n` people.

Worth being precise about this though: greedy gives you a *good* answer, not a provably minimal one. The actual minimum-transfers problem is a harder one than it first looks, and my version can be beaten on some inputs. For four flatmates it doesn't matter, and I preferred something I could reason about over something clever I couldn't. But I'd rather say that than claim it's optimal.

## Getting the rounding right

Split 100 kr between three people and each share is 33.333... Round each one and you get 99.99, which means the shares no longer add up to the expense. It's a rounding error of one øre and it feels like nothing, but do it a few hundred times over a year and the books stop balancing.

Two decisions handle it:

- All money is `decimal`, never `double`. Binary floating point can't represent 0.1 exactly, which is exactly the wrong property for currency.
- The remainder from a split is assigned to one participant rather than dropped, so the shares always sum to the original amount.

The domain logic is tested with xUnit, covering equal splits, several expenses in one group, payments reducing debt, overpayment, rounding edge cases, and the settlement suggestions.

```bash
dotnet test
```

## Running it

The database is PostgreSQL 16 in Docker:

```bash
docker compose up -d
```

That creates the `rm_splitter` database on port 5433 and a persistent volume so the data survives a restart.

Then the API:

```bash
cd backend/RoommateSplitter.Api
dotnet run
```

Swagger comes up with it.

## How it's structured

It follows Clean Architecture, so the domain layer holds the balance and settlement logic and doesn't know anything about EF Core, PostgreSQL or HTTP. That sounds like ceremony for a project this size, and honestly it partly is. But it paid off in one specific way: the money logic is all in plain C# classes with no database anywhere near them, so the tests run instantly and I could work on the settlement algorithm without spinning anything up.

## What I'd do next

- **Uneven splits.** Everything is split equally right now. Percentages and shares-per-person would make it actually usable, and it changes the maths more than you'd think.
- **Accounts and auth.** There's no concept of a logged-in user yet, so anyone can post as anyone.
- **A frontend.** It's API-only, which means it's only usable by me and Swagger.
- **Multiple currencies**, which mostly means deciding what an exchange rate means for a debt from three months ago.

## Related

I later built a [Power BI dashboard](https://github.com/Hajrudin27/expense-analytics-dashboard) on top of this data model, to see what a year of the data actually looks like. The rounding invariant from this project turns up there too, as a reconciliation check on the report itself.

## Built with

.NET 8, ASP.NET Core, Entity Framework Core, PostgreSQL, Docker, xUnit
