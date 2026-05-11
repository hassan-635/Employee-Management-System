:: ==============================================================
::  SmartEPS Docker Deployment — Step-by-Step Commands
::  Save as deploy.bat and run from project root
:: ==============================================================

@echo off
setlocal

set IMAGE_NAME=smarteps
set CONTAINER_NAME=smarteps-container
set TAG=latest

echo.
echo ╔══════════════════════════════════════════════════════╗
echo ║   Smart EPS — Docker Deployment Process              ║
echo ╚══════════════════════════════════════════════════════╝
echo.

:: ── STEP 1: Switch Docker to Windows containers ───────────
echo [STEP 1] Switching Docker Desktop to Windows containers...
:: Right-click Docker tray icon → "Switch to Windows containers"
:: Or run:
:: "C:\Program Files\Docker\Docker\DockerCli.exe" -SwitchDaemon

:: ── STEP 2: Build the Docker image ────────────────────────
echo [STEP 2] Building Docker image...
docker build --platform windows/amd64 ^
             -f Docker/Dockerfile ^
             -t %IMAGE_NAME%:%TAG% ^
             .

if %errorlevel% neq 0 (
    echo ERROR: Docker build failed.
    exit /b 1
)
echo   ✅ Image built: %IMAGE_NAME%:%TAG%

:: ── STEP 3: Verify image was created ──────────────────────
echo.
echo [STEP 3] Verifying image...
docker images %IMAGE_NAME%

:: ── STEP 4: Run the container ─────────────────────────────
echo.
echo [STEP 4] Starting container...
:: WinForms needs RDP or an interactive session.
:: For Docker Desktop on Windows, run interactively:
docker run --rm ^
           --name %CONTAINER_NAME% ^
           --isolation process ^
           -v %cd%\models:/app/models ^
           -it %IMAGE_NAME%:%TAG%

:: ── STEP 5: Verify container is running ───────────────────
echo.
echo [STEP 5] List running containers...
docker ps

:: ── STEP 6: Inspect container logs ────────────────────────
echo.
echo [STEP 6] Container logs...
docker logs %CONTAINER_NAME%

:: ── CLEANUP ───────────────────────────────────────────────
echo.
echo To stop the container:
echo   docker stop %CONTAINER_NAME%
echo.
echo To remove the image:
echo   docker rmi %IMAGE_NAME%:%TAG%
echo.
endlocal