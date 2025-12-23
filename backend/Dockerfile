# Simple development stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS final
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

EXPOSE 5000

WORKDIR /src/src/Api
ENTRYPOINT ["dotnet", "run", "--urls", "http://0.0.0.0:5000"]
