# --- build ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore first (better layer caching)
COPY ["TaskTaskerAPI.csproj", "./"]
RUN dotnet restore "TaskTaskerAPI.csproj"

# Copy the rest and publish
COPY . .
RUN dotnet publish "TaskTaskerAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# --- runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# The app binds to $PORT (set by the host) via Program.cs.
ENTRYPOINT ["dotnet", "TaskTaskerAPI.dll"]
