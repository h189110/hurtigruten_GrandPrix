FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["backend/GrandPrix.Api/GrandPrix.Api.csproj", "backend/GrandPrix.Api/"]
RUN dotnet restore "backend/GrandPrix.Api/GrandPrix.Api.csproj"
COPY backend/GrandPrix.Api/ backend/GrandPrix.Api/
RUN dotnet publish "backend/GrandPrix.Api/GrandPrix.Api.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GrandPrix.Api.dll"]
