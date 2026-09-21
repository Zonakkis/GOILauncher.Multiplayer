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
# 必须是 aspnet 而不是 runtime：dotnet/runtime 只含 Microsoft.NETCore.App，
# ASP.NET Core 共享框架（Kestrel/Razor）不在里面，用 runtime 镜像会在启动时
# 直接报 "It was not possible to find any compatible framework version"。
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 9027/udp
# Web UI（ASP.NET Core，端口由 WEB_PORT 环境变量覆盖，默认 9028）
EXPOSE 9028/tcp

ENTRYPOINT ["dotnet", "ConsoleServer.dll"]