# RaceDay – Event Management System

## 1. Project Overview

RaceDay is an event management system developed to support the management of sporting events, participant registrations, event categories and race results.

The system uses an ASP.NET Core Web API for backend functionality and an ASP.NET Core MVC application for the user interface.

The project demonstrates the use of RESTful APIs, database integration, authentication, role-based access and software testing.

## 2. Technologies Used

- C#
- ASP.NET Core Web API
- ASP.NET Core MVC
- Entity Framework Core
- Microsoft SQL Server
- HTML and CSS
- Bootstrap
- Swagger / OpenAPI
- xUnit
- Git and GitHub
- Visual Studio 2022

## 3. System Structure

The Part 3 solution contains three main projects:

| Project | Description |
|---|---|
| RaceDay.Api | Backend REST API |
| RaceDay.Api.Tests | Automated API tests |
| RaceDay.Mvc | MVC web application |

### Backend API

The API handles the application's main operations, including:

- User registration and login.
- User authentication and session management.
- Event management.
- Category management.
- Participant enrolments.
- Race results.
- User profile management.

### MVC Application

The MVC application provides the web interface through which users interact with the RaceDay system.

## 4. User Roles

The system supports two user roles:

**Participant**
- Create an account.
- Log in to the system.
- Browse available events.
- Participate in event enrolment activities.
- Access relevant event and result information.

**Organiser**
- Log in to the system.
- Manage sporting events.
- Manage event categories.
- Access event-related management functionality.
- Manage race results.

## 5. Authentication and Security

RaceDay implements authentication through the API.

Security features include:

- Password hashing using PBKDF2.
- Server-side session authentication.
- Role-based authorization.
- Protection of restricted API endpoints.
- Duplicate email registration prevention.

## 6. Database

The application uses Entity Framework Core to communicate with Microsoft SQL Server.

The database stores information relating to:

- Users
- Events
- Categories
- Event enrolments
- Results
- Route information

Database migrations are used to manage database schema changes.

## 7. Running the Project

### Requirements

Make sure the following software is installed:

- Visual Studio 2022
- .NET SDK
- Microsoft SQL Server or SQL Server LocalDB

### Setup Instructions

1. Clone or download the RaceDay repository.
2. Open the Part 3 solution file in Visual Studio.
3. Check the database connection string in the API's `appsettings.json`.
4. Restore NuGet packages.
5. Build the solution.
6. Configure the API and MVC projects to run together.
7. Start the API and MVC applications.

### API Address

The API is configured to run at:

`http://localhost:5050`

Swagger documentation can be accessed at:

`http://localhost:5050/swagger`

### MVC Application

The MVC application runs using its configured launch profile.

## 8. API Endpoints

| HTTP Method | Endpoint | Purpose |
|---|---|---|
| POST | `/api/auth/register` | Register a user |
| POST | `/api/auth/login` | Log in |
| POST | `/api/auth/logout` | Log out |
| GET | `/api/events` | Retrieve events |
| GET | `/api/categories` | Retrieve categories |
| GET | `/api/enrolments` | Retrieve enrolments |
| GET | `/api/results` | Retrieve results |

Additional endpoints support the management of events, categories, enrolments, results and user profiles.

## 9. Automated Testing

The `RaceDay.Api.Tests` project contains automated tests for API functionality.

Testing covers areas such as:

- User registration.
- User login.
- Event management.
- Category management.
- Participant enrolments.
- Role-based access restrictions.
- Duplicate registration handling.

Tests can be executed through Visual Studio Test Explorer or the .NET CLI.

## 10. Version Control

Git and GitHub are used to manage the development process.

The project repository:

https://github.com/Given19/-RaceDay-POE

Git commits document the development and changes made throughout the project.

## 11. Conclusion

RaceDay demonstrates the development of a web-based sporting event management system using modern .NET technologies.

The project combines backend API development, MVC architecture, database management, authentication, authorization and automated testing.

The overall goal is to provide a structured platform for managing sporting events and supporting participants and organisers.
