FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/SummitUI.Docs/SummitUI.Docs.Client/SummitUI.Docs.Client.csproj", "src/SummitUI.Docs/SummitUI.Docs.Client/"]
RUN dotnet restore "src/SummitUI.Docs/SummitUI.Docs.Client/SummitUI.Docs.Client.csproj"
COPY . .
WORKDIR "/src/src/SummitUI.Docs/SummitUI.Docs.Client"
RUN dotnet build "./SummitUI.Docs.Client.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SummitUI.Docs.Client.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SummitUI.Docs.Client.dll"]
