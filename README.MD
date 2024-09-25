# SalonX - Beauty Services Management and Booking Platform

This is a .NET backend project for the SalonX beauty services management and booking platform.

## Getting Started 

To get started with the project, follow these steps:

1. Clone the repository:
    ```sh
    git clone <repository-url>
    cd repository
    ```

2. Build the project:
    ```sh
    dotnet build
    ```

3. Run the project:
    ```sh
    dotnet run --project WebApp
    ```

## Project Overview

SalonX is a beauty services management and booking platform that uses .NET for the backend and Next.js for the frontend. The public view is hosted on Azure at [https://salonx-nextjs.azurewebsites.net/signin](https://salonx-nextjs.azurewebsites.net/signin). The backend Swagger documentation is available at [https://salonx-api.azurewebsites.net/swagger/index.html](https://salonx-api.azurewebsites.net/swagger/index.html).

The frontend GitHub project is available at [https://github.com/j-vinnal/Salon-X/](https://github.com/j-vinnal/Salon-X/).

The backend uses Clean Architecture. Check the [Codebase](#directory-structure) to see how the code is divided into layers: Domain, DAL, BLL, Public, and the use of generics and base classes.

### Technologies

- **.NET** - A free, cross-platform, open-source developer platform for building many different types of applications.
- **Entity Framework Core** - An object-database mapper for .NET.
- **xUnit** - A free, open-source, community-focused unit testing tool for the .NET Framework.
- **JWT Authentication** - JSON Web Token for secure authentication.
- **Docker** - For containerization and deployment.
- **PostgreSQL** - Open-source object-relational database system.

### Directory Structure

- **`/App`** - Application-specific code.
  - **`/BLL`** - Business Logic Layer.
  - **`/Contracts`** - Interfaces and contracts.
  - **`/DAL`** - Data Access Layer.
  - **`/Domain`** - Domain models.
  - **`/DTO`** - Data Transfer Objects.
  - **`/Public`** - Public-facing code.
- **`/Base`** - Base classes and shared code.
- **`/WebApp`** - The main web application project.

## Docker

### Build and run locally

Create the image:

```bash
docker build -t salonx-backend:latest .
```

Run the container:

```bash
docker run -d -p 8000:80 salonx-backend:latest
```

### Build to Docker Hub

Create the image:

```bash
docker buildx build --progress=plain -t salonx-backend:latest .
```

Create a tag:

```bash
docker tag salonx-backend <your-docker-hub-profile>/salonx-backend:latest
```

Push the image:

```bash
docker push <your-docker-hub-profile>/salonx-backend:latest
```

## Database Migrations

### Add migration

```bash
dotnet ef migrations add --context AppDbContext --project App.DAL.EF --startup-project WebApp initial
```

### Update DB

```bash
dotnet ef database update --context AppDbContext --project App.DAL.EF --startup-project WebApp
```

### Drop DB

```bash
dotnet ef database drop --project App.DAL.EF --startup-project WebApp
```

## Code Generator

### Install tooling

```bash
dotnet tool install --global dotnet-aspnet-codegenerator
dotnet tool update --global dotnet-aspnet-codegenerator
```

### Generate Identity UI

```bash
dotnet aspnet-codegenerator identity -dc DAL.EF.AppDbContext --userClass AppUser -f
```

### Generate REST Controllers

```bash
dotnet aspnet-codegenerator controller -m App.Domain.Company -name CompaniesController -outDir ApiControllers -api -dc AppDbContext -udl --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -m App.Domain.Service -name ServicesController -outDir ApiControllers -api -dc AppDbContext -udl --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -m App.Domain.Client -name ClientsController -outDir ApiControllers -api -dc AppDbContext -udl --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -m App.Domain.Booking -name BookingsController -outDir ApiControllers -api -dc AppDbContext -udl --referenceScriptLibraries -f
```

## Personal Note

This project is created for personal practice and learning of .NET. If you have any questions or suggestions, feel free to reach out!

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License.