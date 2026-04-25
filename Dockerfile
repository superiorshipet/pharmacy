FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY backend/*.csproj backend/
RUN dotnet restore "backend/DawaeeBackend.csproj" --runtime linux-x64

# Copy everything and build
COPY backend/ backend/
WORKDIR "/src/backend"
RUN dotnet publish "DawaeeBackend.csproj" -c Release -o /app/publish -r linux-x64 --self-contained false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .

# Set environment variable for Railway
ENV ASPNETCORE_URLS=http://+:8080
ENV RAILWAY_ENVIRONMENT=true

ENTRYPOINT ["dotnet", "DawaeeBackend.dll"]
