FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src
EXPOSE 80

# copy csproj and restore as distinct layers
COPY *.props .
COPY *.sln .

# copy over all the projects from host to image
# Base

COPY Base.BLL/*.csproj ./Base.BLL/
COPY Base.Contracts/*.csproj ./Base.Contracts/
COPY Base.Contracts.BLL/*.csproj ./Base.Contracts.BLL/
COPY Base.Contracts.DAL/*.csproj ./Base.Contracts.DAL/
COPY Base.Contracts.Domain/*.csproj ./Base.Contracts.Domain/
COPY Base.DAL/*.csproj ./Base.DAL/
COPY Base.DAL.EF/*.csproj ./Base.DAL.EF/
COPY Base.Domain/*.csproj ./Base.Domain/
COPY Base.Helpers/*.csproj ./Base.Helpers/
COPY Base.Resources/*.csproj ./Base.Resources/
COPY Base.Tests/*.csproj ./Base.Tests/

# App
COPY App.BLL/*.csproj ./App.BLL/
COPY App.Contracts.BLL/*.csproj ./App.Contracts.BLL/
COPY App.Contracts.DAL/*.csproj ./App.Contracts.DAL/
COPY App.Contracts.WebApp/*.csproj ./App.Contracts.WebApp/
COPY App.DAL.EF/*.csproj ./App.DAL.EF/
COPY App.Domain/*.csproj ./App.Domain/
COPY App.DTO.BLL/*.csproj ./App.DTO.BLL/
COPY App.DTO.DAL/*.csproj ./App.DTO.DAL/
COPY App.DTO.Public/*.csproj ./App.DTO.Public/
COPY App.Public/*.csproj ./App.Public/
COPY App.Resources/*.csproj ./App.Resources/
COPY App.Tests/*.csproj ./App.Tests/
COPY Tests/*.csproj ./Tests/
COPY WebApp/*.csproj ./WebApp/

RUN dotnet restore

# copy everything else and build app
# Base

COPY Base.BLL/. ./Base.BLL/
COPY Base.Contracts/. ./Base.Contracts/
COPY Base.Contracts.BLL/. ./Base.Contracts.BLL/
COPY Base.Contracts.DAL/. ./Base.Contracts.DAL/
COPY Base.Contracts.Domain/. ./Base.Contracts.Domain/
COPY Base.DAL/. ./Base.DAL/
COPY Base.DAL.EF/. ./Base.DAL.EF/
COPY Base.Domain/. ./Base.Domain/
COPY Base.Helpers/. ./Base.Helpers/
COPY Base.Resources/. ./Base.Resources/
COPY Base.Tests/. ./Base.Tests/

# App
COPY App.BLL/. ./App.BLL/
COPY App.Contracts.BLL/. ./App.Contracts.BLL/
COPY App.Contracts.DAL/. ./App.Contracts.DAL/
COPY App.Contracts.WebApp/. ./App.Contracts.WebApp/
COPY App.DAL.EF/. ./App.DAL.EF/
COPY App.Domain/. ./App.Domain/
COPY App.DTO.BLL/. ./App.DTO.BLL/
COPY App.DTO.DAL/. ./App.DTO.DAL/
COPY App.DTO.Public/. ./App.DTO.Public/
COPY App.Public/. ./App.Public/
COPY App.Resources/. ./App.Resources/
COPY App.Tests/. ./App.Tests/
COPY Tests/. ./Tests/
COPY WebApp/. ./WebApp/

# Copy seed data
COPY App.DAL.EF/Seeding/SeedData/ /src/App.DAL.EF/Seeding/SeedData/
COPY App.DAL.EF/Seeding/SeedData/ /app/App.DAL.EF/Seeding/SeedData/

# Set IsTestEnvironment to true and run tests
#ENV Testing__TestingIsTestEnvironment=true
#ENV ConnectionStrings__TestConnection="Host=host.docker.internal;Database=beautyhub_test;Username=postgres;Password=postgres"
#RUN dotnet test App.Tests/App.Tests.csproj

# Set IsTestEnvironment to false and build the application

#ENV Testing__TestingIsTestEnvironment=false
WORKDIR /src/WebApp
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 80

COPY --from=build /src/WebApp/out ./

ENV TZ=Etc/UTC

ENTRYPOINT ["dotnet", "WebApp.dll"]
