FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Tracker.Web/Tracker.Web.csproj", "Tracker.Web/"]
RUN dotnet restore "Tracker.Web/Tracker.Web.csproj"

COPY . .
WORKDIR /src/Tracker.Web
RUN dotnet publish "Tracker.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Tracker.Web.dll"]
