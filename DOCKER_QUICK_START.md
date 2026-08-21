# Docker Quick Start Guide

## One-Command Build & Test

```bash
# Linux/macOS
./docker-run.sh build

# Windows
.\docker-run.bat build
```

This will:
✓ Build the Docker image  
✓ Run unit tests  
✓ Run integration tests  
✓ Publish deployment artifacts  

## Run the Application

```bash
# Linux/macOS
./docker-run.sh compose-up

# Windows
.\docker-run.bat compose-up
```

Access at: `http://localhost:5000`

## Useful Commands

```bash
# Linux/macOS
./docker-run.sh logs          # View logs
./docker-run.sh shell         # Interactive shell
./docker-run.sh compose-down  # Stop services
./docker-run.sh clean         # Remove everything

# Windows
.\docker-run.bat logs
.\docker-run.bat shell
.\docker-run.bat compose-down
.\docker-run.bat clean
```

## Manual Docker Commands

```bash
# Build image
docker build -t ultimatepdf:latest .

# Run tests only (build stage)
docker build -t ultimatepdf:test --target builder .

# Start container
docker run -it -p 5000:80 ultimatepdf:latest

# Use docker-compose
docker-compose up -d
docker-compose logs -f
docker-compose down
```

## Troubleshooting

### Build Fails
- Ensure Docker daemon is running
- Check you have 4GB+ RAM available
- View full logs: `docker build -t ultimatepdf:latest .`

### Port 5000 Already in Use
- Change in `docker-compose.yml`: `"5001:80"`
- Or use: `docker run -p 5001:80 ultimatepdf:latest`

### Integration Tests Timeout
- Integration tests require browser resources
- They have error tolerance (`|| true`) and won't block the build

## See Also

- `DOCKER.md` - Complete Docker documentation
- `CLAUDE.md` - Project development guide
- `README.md` - UltimatePDF usage and API reference
