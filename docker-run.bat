@echo off
REM UltimatePDF Docker Build & Test script for Windows

setlocal enabledelayedexpansion

echo ======================================
echo UltimatePDF Docker Build ^& Test
echo ======================================

REM Check if Docker is running
docker info >nul 2>&1
if errorlevel 1 (
    echo Error: Docker daemon is not running
    exit /b 1
)

set COMMAND=%1
if "%COMMAND%"=="" set COMMAND=build

if "%COMMAND%"=="build" (
    echo Building Docker image...
    docker build -t ultimatepdf:latest -f Dockerfile .
    if errorlevel 1 exit /b 1
    echo Build completed successfully
    goto end
)

if "%COMMAND%"=="test" (
    echo Running tests in container...
    docker build -t ultimatepdf:test --target builder -f Dockerfile .
    if errorlevel 1 exit /b 1
    echo Tests completed
    goto end
)

if "%COMMAND%"=="run" (
    echo Starting container...
    docker run -it --rm ^
        -p 5000:80 ^
        -e ASPNETCORE_ENVIRONMENT=Development ^
        ultimatepdf:latest
    goto end
)

if "%COMMAND%"=="compose-up" (
    echo Starting services with docker-compose...
    docker-compose up -d
    if errorlevel 1 exit /b 1
    echo Services started
    echo Access the application at http://localhost:5000
    goto end
)

if "%COMMAND%"=="compose-down" (
    echo Stopping services...
    docker-compose down
    if errorlevel 1 exit /b 1
    echo Services stopped
    goto end
)

if "%COMMAND%"=="logs" (
    echo Showing container logs...
    docker-compose logs -f ultimatepdf
    goto end
)

if "%COMMAND%"=="shell" (
    echo Opening shell in container...
    docker run -it --rm ^
        -v "%cd%\src:/src" ^
        mcr.microsoft.com/dotnet/sdk:10.0 ^
        powershell
    goto end
)

if "%COMMAND%"=="clean" (
    echo Cleaning up Docker resources...
    docker-compose down -v >nul 2>&1
    docker rmi ultimatepdf:latest >nul 2>&1
    docker rmi ultimatepdf:test >nul 2>&1
    echo Cleanup completed
    goto end
)

if "%COMMAND%"=="help" (
    echo Usage: %0 [command]
    echo.
    echo Commands:
    echo   build         - Build Docker image
    echo   test          - Run tests in container
    echo   run           - Start the application container
    echo   compose-up    - Start services using docker-compose
    echo   compose-down  - Stop services using docker-compose
    echo   logs          - View container logs
    echo   shell         - Open an interactive shell in SDK container
    echo   clean         - Remove Docker images and containers
    echo   help          - Show this help message
    goto end
)

echo Unknown command: %COMMAND%
echo Run "%0 help" for usage information
exit /b 1

:end
endlocal
