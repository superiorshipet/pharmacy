FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["backend/DawaeeBackend/DawaeeBackend.csproj", "backend/DawaeeBackend/"]
RUN dotnet restore "backend/DawaeeBackend/DawaeeBackend.csproj"
COPY . .
WORKDIR "/src/backend/DawaeeBackend"
RUN dotnet build "DawaeeBackend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DawaeeBackend.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DawaeeBackend.dll"]