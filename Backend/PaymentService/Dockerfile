FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Backend/PaymentService/PaymentService.csproj", "Backend/PaymentService/"]
COPY ["Backend/Shared.Contracts/Shared.Contracts.csproj", "Backend/Shared.Contracts/"]
RUN dotnet restore "Backend/PaymentService/PaymentService.csproj"

COPY . .
WORKDIR "/src/Backend/PaymentService"
RUN dotnet publish "PaymentService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PaymentService.dll"]