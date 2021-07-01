#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["HC/HC.csproj", "HC/"]
COPY ["HC.DAL.MongoDb/HC.DAL.MongoDb.csproj", "HC.DAL.MongoDb/"]
COPY ["HC.Domain/HC.Domain.csproj", "HC.Domain/"]
COPY ["HC.Adapters.Telegram/HC.Adapters.Telegram.csproj", "HC.Adapters.Telegram/"]
COPY ["HC.ApplicationServices/HC.ApplicationServices.csproj", "HC.ApplicationServices/"]
COPY ["HC.DAL.LiteDB/HC.DAL.LiteDB.csproj", "HC.DAL.LiteDB/"]
RUN dotnet restore "HC/HC.csproj"
COPY . .
WORKDIR "/src/HC"
RUN dotnet build "HC.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HC.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HC.dll"]