# -RaceDay-POE
# RaceDay - Event Management System

## System Description

RaceDay is a full-stack, web-based event management system built for the South African road running, walking, and cycling community. The platform allows Event Organisers to create and manage events, categories, and participant results, while Participants can browse upcoming events, enter events, track their personal performance history, and prepare for race day using live weather and route information.

This repository contains the Part 1 submission: system planning and database design.

## Roles

- **Organiser** - creates and manages events and categories, and captures participant results.
- **Participant** - browses events, enrols in categories, and views their own enrolment and results history.

Roles are enforced via a `Role` column on the `User` table and checked at the API level (planned in Part 2).

## Part 1 Contents (`/docs`)

- `raceday_erd.png` / `raceday_erd.pdf` - Entity Relationship Diagram (6 entities, with primary keys, foreign keys, and cardinality).
- `RaceDay_API_Endpoint_Plan.md` - Full API endpoint plan covering authentication, user profile, events, categories, event enrolments, and results.
- `RaceDay_Database_Script.sql` - SQL Server script creating and seeding the full database schema.

## SQL Script Execution Evidence

The script was run successfully against a clean `RaceDayDB` instance in SQL Server Management Studio. Rows affected match the seed data exactly: 4 Users, 3 Events, 6 Categories, 3 RouteInfo, 4 EventEnrolments, 2 Results.

![SQL script executed successfully](docs/sql-execution-screenshot.png)

## CI/CD

A GitHub Actions workflow (`.github/workflows/validate-structure.yml`) validates that the `/docs` folder exists and contains the required files on every push and pull request.




**PRESENTATION
The Presentation walks through the planning documents, ERD design decisions, endpoint plan choices, and runs the SQL script live in SSMS.
