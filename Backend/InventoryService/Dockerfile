FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Backend/InventoryService/InventoryService.csproj", "Backend/InventoryService/"]
COPY ["Backend/Shared.Contracts/Shared.Contracts.csproj", "Backend/Shared.Contracts/"]
RUN dotnet restore "Backend/InventoryService/InventoryService.csproj"

COPY . .
WORKDIR "/src/Backend/InventoryService"
RUN dotnet publish "InventoryService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "InventoryService.dll"]