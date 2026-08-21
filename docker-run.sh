#!/bin/bash

set -e

echo "======================================"
echo "UltimatePDF Docker Build & Test"
echo "======================================"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check if Docker is running
echo "Checking Docker daemon..."
if ! docker info > /dev/null 2>&1; then
    echo -e "${RED}Error: Docker daemon is not running${NC}"
    exit 1
fi

COMMAND=${1:-build}

case $COMMAND in
    build)
        echo -e "${YELLOW}Building Docker image...${NC}"
        docker build -t ultimatepdf:latest -f Dockerfile .
        echo -e "${GREEN}✓ Build completed successfully${NC}"
        ;;

    test)
        echo -e "${YELLOW}Running tests in container...${NC}"
        docker build -t ultimatepdf:test --target builder -f Dockerfile .
        echo -e "${GREEN}✓ Tests completed${NC}"
        ;;

    run)
        echo -e "${YELLOW}Starting container...${NC}"
        docker run -it --rm \
            -p 5000:80 \
            -e ASPNETCORE_ENVIRONMENT=Development \
            ultimatepdf:latest
        ;;

    compose-up)
        echo -e "${YELLOW}Starting services with docker-compose...${NC}"
        docker-compose up -d
        echo -e "${GREEN}✓ Services started${NC}"
        echo "Access the application at http://localhost:5000"
        ;;

    compose-down)
        echo -e "${YELLOW}Stopping services...${NC}"
        docker-compose down
        echo -e "${GREEN}✓ Services stopped${NC}"
        ;;

    logs)
        echo -e "${YELLOW}Showing container logs...${NC}"
        docker-compose logs -f ultimatepdf
        ;;

    shell)
        echo -e "${YELLOW}Opening shell in container...${NC}"
        docker run -it --rm \
            -v "$(pwd)/src:/src" \
            mcr.microsoft.com/dotnet/sdk:10.0 \
            bash
        ;;

    clean)
        echo -e "${YELLOW}Cleaning up Docker resources...${NC}"
        docker-compose down -v 2>/dev/null || true
        docker rmi ultimatepdf:latest 2>/dev/null || true
        docker rmi ultimatepdf:test 2>/dev/null || true
        echo -e "${GREEN}✓ Cleanup completed${NC}"
        ;;

    help)
        echo "Usage: $0 {build|test|run|compose-up|compose-down|logs|shell|clean|help}"
        echo ""
        echo "Commands:"
        echo "  build         - Build Docker image"
        echo "  test          - Run tests in container"
        echo "  run           - Start the application container"
        echo "  compose-up    - Start services using docker-compose"
        echo "  compose-down  - Stop services using docker-compose"
        echo "  logs          - View container logs"
        echo "  shell         - Open an interactive shell in SDK container"
        echo "  clean         - Remove Docker images and containers"
        echo "  help          - Show this help message"
        ;;

    *)
        echo -e "${RED}Unknown command: $COMMAND${NC}"
        echo "Run '$0 help' for usage information"
        exit 1
        ;;
esac
