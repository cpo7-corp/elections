@echo off
echo Starting Docker containers...
cd /d "%~dp0"
docker-compose up --build -d

echo Waiting for the UI to be ready...
timeout /t 5 /nobreak > NUL

echo Opening the UI in your default browser...
start http://localhost:8085

echo.
echo Application is running in the background. 
echo To see logs, run: docker-compose logs -f
echo To stop, run: docker-compose down
