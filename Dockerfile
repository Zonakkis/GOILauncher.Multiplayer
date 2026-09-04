# =========================
# Build stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

COPY . .

RUN dotnet restore "src/ConsoleServer/ConsoleServer.csproj" \
    /p:TargetNetStandard20=true

RUN dotnet publish "src/ConsoleServer/ConsoleServer.csproj" \
    -c Release \
    -f net8.0 \
    -o /app/publish \
    --no-restore \
    /p:TargetNetStandard20=true


# =========================
# Runtime stage
# =========================
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 9027/udp

ENTRYPOINT ["dotnet", "ConsoleServer.dll"]