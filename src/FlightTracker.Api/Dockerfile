# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["FlightTracker.Api/FlightTracker.Api.csproj", "FlightTracker.Api/"]
COPY ["FlightTracker.Core/FlightTracker.Core.csproj", "FlightTracker.Core/"]
COPY ["FlightTracker.Infrastructure/FlightTracker.Infrastructure.csproj", "FlightTracker.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "FlightTracker.Api/FlightTracker.Api.csproj"

# Copy remaining source code
COPY . .

# Build the application
WORKDIR "/src/FlightTracker.Api"
RUN dotnet build "FlightTracker.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "FlightTracker.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

# Copy published application
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "FlightTracker.Api.dll"]
