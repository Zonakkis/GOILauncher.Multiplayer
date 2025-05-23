# 使用 SDK 镜像进行构建
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["ConsoleGameServer/ConsoleGameServer.csproj", "ConsoleGameServer/"]
COPY ["Server.net9/Server.net9.csproj", "Server.net9/"]
COPY ["Shared.net9/Shared.net9.csproj", "Shared.net9/"]
COPY ["LiteNetLib.net9/LiteNetLib.net9.csproj", "LiteNetLib.net9/"]

RUN dotnet restore "ConsoleGameServer/ConsoleGameServer.csproj"

# 复制所有源代码
COPY . .

WORKDIR /src/ConsoleGameServer
RUN dotnet publish -c Release -o /app/publish

# 使用运行时镜像构建最终镜像
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ConsoleGameServer.dll"]
