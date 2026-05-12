FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 10000
ENV ASPNETCORE_URLS=http://+:10000

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["RetakePortal.csproj", "."]
RUN dotnet restore "./RetakePortal.csproj"
COPY . .
RUN dotnet build "RetakePortal.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RetakePortal.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RetakePortal.dll"]
