# =========================
# Web UI build stage
# =========================
FROM node:24-alpine AS webui

WORKDIR /web

COPY src/DedicatedServer/React/ ./

RUN corepack enable \
    && pnpm install --frozen-lockfile \
    && VITE_OUT_DIR=/webui-dist pnpm build


# =========================
# Build stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

COPY . .

COPY --from=webui /webui-dist ./src/DedicatedServer/wwwroot/

RUN dotnet restore "src/DedicatedServer/DedicatedServer.csproj"

RUN dotnet publish "src/DedicatedServer/DedicatedServer.csproj" \
    -c Release \
    -f net8.0 \
    -o /app/publish \
    --no-restore


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
# Web UI（ASP.NET Core，端口由 GOI_WEB_PORT 环境变量覆盖，默认 9028）
EXPOSE 9028/tcp

ENTRYPOINT ["dotnet", "GOILauncher.Multiplayer.DedicatedServer.dll"]
