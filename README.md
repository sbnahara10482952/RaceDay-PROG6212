# RaceDay Event Management System

## 1. Project Overview

RaceDay is a web-based event management system developed as part of the PROG6212 Programming 2B Portfolio of Evidence.

The system allows event organisers to manage sporting events, categories, participant enrolments and race results. Participants can register, log in, browse available events, enrol in events and view their personal race results.

The application consists of an ASP.NET Core Web API and an ASP.NET Core MVC web application. It uses Microsoft SQL Server for data storage and Azure Blob Storage for image storage.

## 2. Main Features

- User registration and login.
- Session-based authentication.
- Role-based access for Organisers and Participants.
- User profile viewing and updating.
- Event creation, viewing, editing and cancellation.
- Event category management.
- Participant event enrolment.
- Organiser access to event enrolments.
- Race result capture and viewing.
- Participant result viewing.
- Azure Blob Storage image integration.
- SQL Server database integration.
- Entity Framework Core.
- RESTful Web API.
- Docker containerisation.
- Automated unit testing.
- GitHub Actions CI/CD pipeline.

## 3. User Roles

### Organiser

Organisers can:

- Create events.
- Edit existing events.
- Cancel events.
- Manage event categories.
- View participant enrolments.
- Capture race results.
- View event results.

### Participant

Participants can:

- Register an account.
- Log in and log out.
- View and update their profile.
- Browse available events.
- View event categories.
- Enrol in an event category.
- View their own enrolments.
- View their personal race results.

## 4. Technologies Used

The project uses the following technologies:

- C#
- ASP.NET Core Web API
- ASP.NET Core MVC
- .NET 10
- Entity Framework Core
- Microsoft SQL Server
- Azure Blob Storage
- HTML5
- CSS3
- Bootstrap 5
- JavaScript
- Visual Studio 2026
- Docker and Docker Compose
- Git and GitHub
- GitHub Actions
- xUnit

## 5. Database

RaceDay uses Microsoft SQL Server as its database platform.

Entity Framework Core Code-First is used to manage the database models and migrations.

The main database entities include:

- User
- Event
- Category
- EventCategory
- Enrolment
- Result

The database uses primary keys, foreign keys, unique constraints, validation rules and relationships between entities.

## 6. REST API

The application uses an ASP.NET Core Web API to handle communication between the MVC application and the database.

Examples of API endpoints include:

| Method | Endpoint                             | Description              |
| ------ | ------------------------------------ | ------------------------ |
| POST   | /api/auth/register                   | Register a user          |
| POST   | /api/auth/login                      | Log in                   |
| POST   | /api/auth/logout                     | Log out                  |
| GET    | /api/users/me                        | View user profile        |
| PUT    | /api/users/me                        | Update user profile      |
| GET    | /api/events                          | View events              |
| GET    | /api/events/{eventId}                | View event details       |
| POST   | /api/events                          | Create an event          |
| PUT    | /api/events/{eventId}                | Update an event          |
| DELETE | /api/events/{eventId}                | Cancel an event          |
| POST   | /api/enrolments                      | Enrol in an event        |
| GET    | /api/enrolments/me                   | View personal enrolments |
| POST   | /api/enrolments/{enrolmentId}/result | Record a race result     |
| GET    | /api/results/me                      | View personal results    |

Access to protected functionality is controlled using the logged-in user's session and role.

The API can also be explored through Swagger.

## 7. Authentication and Security

RaceDay uses session-based authentication.

Passwords are not stored as plain text. Passwords are protected using PBKDF2 hashing with a randomly generated salt.

The application checks the user's role before allowing access to organiser or participant functionality.

Sensitive configuration information, including database credentials and Azure Storage credentials, is kept outside the source code.

The root `.env` file is excluded from Git using `.gitignore`.

## 8. Azure Blob Storage

Azure Blob Storage is integrated into the application to support image storage and retrieval.

The application uses a private storage container named:

`raceday-images`

The Blob Storage service handles image uploads and retrieval.

Azure Storage configuration is supplied through environment variables and local development secrets rather than committing credentials to the GitHub repository.

## 9. Docker Implementation

Docker is used to containerise the RaceDay API and MVC applications.

The project contains:

- `RaceDay.API/RaceDay.API/Dockerfile.api`
- `RaceDay.API/RaceDay.MVC/Dockerfile.mvc`
- `docker-compose.yml`

Docker Compose manages the two application services:

| Service | Container   | Port |
| ------- | ----------- | ---- |
| API     | raceday-api | 8081 |
| MVC     | raceday-mvc | 8085 |

The MVC application communicates with the API through the Docker network.

### Running with Docker

1. Install Docker Desktop.
2. Make sure Docker Desktop is running.
3. Create a local `.env` file in the repository root containing the required database and Azure configuration.
4. Do not upload the `.env` file to GitHub.
5. Open PowerShell in the repository root.
6. Run:

```powershell
docker compose up --build -d
```

To check the running containers:

```powershell
docker compose ps
```

To stop the containers:

```powershell
docker compose down
```

### Application URLs

When running through Docker:

- MVC application: `http://localhost:8085`
- API Swagger: `http://localhost:8081/swagger`

The SQL Server database must be accessible to the Docker containers.

## 10. Testing

The project contains a separate `RaceDay.API.Tests` project.

Unit tests are written using xUnit.

The tests cover important functionality, including password hashing and password verification.

The following command runs the unit tests:

```powershell
dotnet test .\RaceDay.API.Tests\RaceDay.API.Tests.csproj --configuration Release
```

The final local test run completed successfully:

- Passed: 5
- Failed: 0
- Skipped: 0

The complete solution was also built using the Release configuration with zero build errors.

## 11. GitHub Actions and CI/CD

GitHub Actions is used to automatically build and test the project when changes are pushed to GitHub.

The workflow is located at:

```text
.github/workflows/dotnet.yml
```

The workflow performs the following tasks:

1. Checks out the repository.
2. Sets up .NET 10.
3. Restores project dependencies.
4. Builds the complete solution.
5. Runs the automated unit tests.

The Part 3 workflow completed successfully, indicated by a green check on GitHub Actions.

## 12. Project Structure

```text
RaceDay-PROG6212
│
├── .github
│   └── workflows
│       └── dotnet.yml
│
├── docs
│   ├── ERD documentation
│   ├── API Endpoint Plan
│   └── SQL documentation
│
├── RaceDay.API
│   ├── PasswordTool
│   │
│   ├── RaceDay.API
│   │   ├── Controllers
│   │   ├── Data
│   │   ├── Migrations
│   │   ├── Models
│   │   └── Services
│   │
│   └── RaceDay.MVC
│       ├── Controllers
│       ├── Models
│       ├── Services
│       ├── ViewModels
│       ├── Views
│       └── wwwroot
│
├── RaceDay.API.Tests
│   └── PasswordServiceTests.cs
│
├── docker-compose.yml
├── .gitignore
└── README.md
```

## 13. Running the Project Locally

To run the project using Visual Studio:

1. Open `RaceDay.API.slnx` in Visual Studio 2026.
2. Ensure the SQL Server database is available.
3. Configure the database connection string using local development configuration.
4. Configure the required Azure Storage credentials using local development secrets.
5. Restore the project dependencies.
6. Build the complete solution.
7. Configure the API and MVC projects to start together.
8. Run the application.
9. Access the MVC application and use Swagger to explore the API.

## 14. Git Branches and Version Control

The project uses Git and GitHub for source control.

The development branches include:

- `main` – main project branch.
- `part2-api` – Part 2 API development.
- `part3-mvc` – Part 3 MVC, Azure Storage and Docker development.

The Part 3 implementation was committed and pushed to the `part3-mvc` branch.

The project includes more than the required 20 meaningful commits.

## 15. Part 1 Documentation

The `docs` folder contains planning and database documentation completed for Part 1, including:

- Entity Relationship Diagram.
- API Endpoint Plan.
- SQL Server database script.
- Database verification screenshots.

## 16. Demonstration Videos

Part 2 YouTube demonstration:

https://youtube/0vyP-kBiOXI

Part 3 YouTube demonstration:

https://youtube.com/shorts/by0BsE4IASg?si=JMPjZHISZ5t2zSqB

## 17. AI Use Disclosure

AI tools were used as development support during the project. They assisted with explanations, troubleshooting, code suggestions and documentation support.

The application was reviewed, implemented and tested by the student. AI assistance was used as a support tool and not as a replacement for understanding the project or verifying its functionality.
