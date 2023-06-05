FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

COPY src/ServiceB/ServiceB.csproj src/ServiceB/
RUN dotnet restore "src/ServiceB/ServiceB.csproj"

COPY . .

WORKDIR /src/src/ServiceB
RUN dotnet publish -c Release -o /app

FROM build as publish

FROM base as final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ServiceB.dll"]

