FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Backend/OrderApi/OrderApi.csproj", "Backend/OrderApi/"]
COPY ["Backend/Shared.Contracts/Shared.Contracts.csproj", "Backend/Shared.Contracts/"]
RUN dotnet restore "Backend/OrderApi/OrderApi.csproj"

COPY . .
WORKDIR "/src/Backend/OrderApi"
RUN dotnet publish "OrderApi.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OrderApi.dll"]