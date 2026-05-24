FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/FishingTrips.Api/FishingTrips.Api.csproj", "src/FishingTrips.Api/"]
RUN dotnet restore "src/FishingTrips.Api/FishingTrips.Api.csproj"
COPY . .
RUN dotnet publish "src/FishingTrips.Api/FishingTrips.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENV ConnectionStrings__Default="Data Source=/data/fishingtrips.db"
RUN mkdir -p /data
VOLUME ["/data"]
EXPOSE 8080
ENTRYPOINT ["dotnet", "FishingTrips.Api.dll"]
