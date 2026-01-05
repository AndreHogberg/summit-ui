# Dockerfile for SummitUI.Docs
# Build context: repository root
#
# Build: docker build -t summitui-docs .
# Run:   docker run -p 8080:8080 summitui-docs

# Stage 1: Build frontend assets (CSS/JS)
FROM node:22-alpine AS node-build

WORKDIR /src

# Copy package files for SummitUI (library)
COPY src/SummitUI/package*.json src/SummitUI/
RUN npm ci --prefix src/SummitUI

# Copy package files for SummitUI.Docs
COPY src/SummitUI.Docs/SummitUI.Docs/package*.json src/SummitUI.Docs/SummitUI.Docs/
RUN npm ci --prefix src/SummitUI.Docs/SummitUI.Docs

# Copy source files needed for npm builds
COPY src/SummitUI/Scripts src/SummitUI/Scripts
COPY src/SummitUI.Docs/SummitUI.Docs/Scripts src/SummitUI.Docs/SummitUI.Docs/Scripts
COPY src/SummitUI.Docs/SummitUI.Docs/Styles src/SummitUI.Docs/SummitUI.Docs/Styles

# Copy all source for Tailwind to scan (it needs to find class usage)
COPY src/ src/

# Build SummitUI JS bundle
RUN npm run build --prefix src/SummitUI

# Build Docs CSS and JS bundles
RUN npm run build --prefix src/SummitUI.Docs/SummitUI.Docs


# Stage 2: Build and publish .NET application
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build

ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files for restore (layer caching)
COPY src/SummitUI.Docs/SummitUI.Docs/SummitUI.Docs.csproj src/SummitUI.Docs/SummitUI.Docs/
COPY src/SummitUI.Docs/SummitUI.Docs.Client/SummitUI.Docs.Client.csproj src/SummitUI.Docs/SummitUI.Docs.Client/
COPY src/SummitUI.Docs.Design/SummitUI.Docs.Design.csproj src/SummitUI.Docs.Design/
COPY src/SummitUI/SummitUI.csproj src/SummitUI/

# Restore dependencies
RUN dotnet restore src/SummitUI.Docs/SummitUI.Docs/SummitUI.Docs.csproj

# Copy all source code
COPY src/ src/

# Copy pre-built frontend assets from node stage
COPY --from=node-build /src/src/SummitUI/wwwroot src/SummitUI/wwwroot
COPY --from=node-build /src/src/SummitUI.Docs/SummitUI.Docs/wwwroot src/SummitUI.Docs/SummitUI.Docs/wwwroot

# Publish the application
WORKDIR /src/src/SummitUI.Docs/SummitUI.Docs
RUN dotnet publish SummitUI.Docs.csproj \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    -p:RunNpmBuild=false \
    -p:UseAppHost=false


# Stage 3: Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final

WORKDIR /app
EXPOSE 8080

# Copy the published application
COPY --from=build /app/publish .

# Use the built-in non-root user
USER $APP_UID

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD wget -q --spider http://localhost:8080/ || exit 1

ENTRYPOINT ["dotnet", "SummitUI.Docs.dll"]
