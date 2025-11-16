#!/bin/bash

# FitTrack Pro - Development Helper Script

case "$1" in
  start)
    echo "Starting Docker services..."
    docker-compose up -d
    echo "Services started!"
    echo "- PostgreSQL: localhost:5432"
    echo "- Redis: localhost:6379"
    echo "- Seq: http://localhost:5341"
    echo "- Elasticsearch: http://localhost:9200"
    ;;
  
  stop)
    echo "Stopping Docker services..."
    docker-compose down
    ;;
  
  restart)
    echo "Restarting Docker services..."
    docker-compose restart
    ;;
  
  logs)
    docker-compose logs -f
    ;;
  
  migrate)
    echo "Running database migrations..."
    dotnet ef database update \
      --project src/FitTrackPro.Infrastructure \
      --startup-project src/FitTrackPro.API
    ;;
  
  seed)
    echo "Seeding database..."
    dotnet run --project src/FitTrackPro.API -- seed-data
    ;;
  
  test)
    echo "Running tests..."
    dotnet test
    ;;
  
  coverage)
    echo "Running tests with coverage..."
    dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
    reportgenerator \
      -reports:"**/coverage.cobertura.xml" \
      -targetdir:"coverage-report" \
      -reporttypes:Html
    echo "Coverage report generated in coverage-report/"
    ;;
  
  clean)
    echo "Cleaning build artifacts..."
    dotnet clean
    rm -rf **/bin **/obj
    ;;
  
  build)
    echo "Building solution..."
    dotnet build
    ;;
  
  run)
    echo "Running API..."
    cd src/FitTrackPro.API
    dotnet run
    ;;
  
  format)
    echo "Formatting code..."
    dotnet format
    ;;
  
  *)
    echo "FitTrack Pro Development Helper"
    echo ""
    echo "Usage: ./scripts/dev.sh [command]"
    echo ""
    echo "Commands:"
    echo "  start     - Start Docker services"
    echo "  stop      - Stop Docker services"
    echo "  restart   - Restart Docker services"
    echo "  logs      - View Docker logs"
    echo "  migrate   - Run database migrations"
    echo "  seed      - Seed database with initial data"
    echo "  test      - Run all tests"
    echo "  coverage  - Generate coverage report"
    echo "  clean     - Clean build artifacts"
    echo "  build     - Build solution"
    echo "  run       - Run API"
    echo "  format    - Format code"
    ;;
esac
