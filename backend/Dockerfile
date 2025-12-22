# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY CollegeFootballGamePredictor.sln ./
COPY src/Api/Api.csproj src/Api/
COPY src/DataRetrieval/DataRetrieval.csproj src/DataRetrieval/
COPY src/Processing/Processing.csproj src/Processing/
COPY src/WebScraping/WebScraping.csproj src/WebScraping/
COPY tests/unit/Unit.Tests.csproj tests/unit/
COPY tests/integration/Integration.Tests.csproj tests/integration/

# Restore dependencies
RUN dotnet restore

# Copy all source code
COPY . .

# Build the API project
WORKDIR /src/src/Api
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install EF Core tools for migrations
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"

EXPOSE 5000

ENTRYPOINT ["dotnet", "Api.dll"]
