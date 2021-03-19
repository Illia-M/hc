#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:5.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:5.0-buster AS build
WORKDIR /src
COPY ["hc/HC.csproj", "hc/"]
COPY ["HC.Adapters.Telegram/HC.Adapters.Telegram.csproj", "HC.Adapters.Telegram/"]
COPY ["HC.ApplicationServices/HC.ApplicationServices.csproj", "HC.ApplicationServices/"]
RUN dotnet restore "hc/HC.csproj"
COPY . .
WORKDIR "/src/hc"
RUN dotnet build "HC.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HC.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HC.dll"]