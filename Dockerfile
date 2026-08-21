# Build and test stage using .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder

WORKDIR /src

# Install Chromium and system dependencies for PuppeteerSharp integration tests
RUN apt-get update && apt-get install -y --no-install-recommends \
    libnspr4 \
    libnss3 \
    libxss1 \
    libappindicator3-1 \
    libsecret-1-0 \
    xdg-utils \
    fonts-liberation \
    libvulkan1 \
    libu2f-udev \
    libgbm1 \
    libdrm2 \
    && rm -rf /var/lib/apt/lists/*

# Copy solution, project files, and OML files
COPY src/ ./
COPY oml/ /oml/

# Restore dependencies
RUN dotnet restore UltimatePDF_ExternalLogic.sln

# Build the solution
RUN dotnet build UltimatePDF_ExternalLogic.sln -c Release --no-restore

# Run unit tests
RUN dotnet test UltimatePDF_ExternalLogic.UnitTests/UltimatePDF_ExternalLogic.UnitTests.csproj -c Release --no-build --logger "console;verbosity=detailed"

# Run integration tests (with Chromium system dependencies installed)
RUN dotnet test UltimatePDF_ExternalLogic.IntegrationTests/UltimatePDF_ExternalLogic.IntegrationTests.csproj -c Release --no-build --logger "console;verbosity=detailed"

# Note: E2E tests are skipped as they require a live ODC tenant

# Publish the main project for deployment
RUN dotnet publish UltimatePDF_ExternalLogic/UltimatePDF_ExternalLogic.csproj -c Release -o /app/publish --no-build

# Runtime stage using aspnet image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

# Copy published artifacts from builder
COPY --from=builder /app/publish .

# Health check (basic - can be customized based on your service)
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD echo "Container is running"

CMD ["dotnet", "UltimatePDF_ExternalLogic.dll"]
