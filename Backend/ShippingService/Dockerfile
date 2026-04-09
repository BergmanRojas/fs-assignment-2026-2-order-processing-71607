FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Backend/ShippingService/ShippingService.csproj", "Backend/ShippingService/"]
COPY ["Backend/Shared.Contracts/Shared.Contracts.csproj", "Backend/Shared.Contracts/"]
RUN dotnet restore "Backend/ShippingService/ShippingService.csproj"

COPY . .
WORKDIR "/src/Backend/ShippingService"
RUN dotnet publish "ShippingService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ShippingService.dll"]