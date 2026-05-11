# ==============================================================
#  Dockerfile — Smart Employee Performance System
#  Multi-stage build: SDK build → Runtime image
#  NOTE: WinForms requires Windows containers.
#        Use: docker build --platform windows/amd64 -t smarteps .
# ==============================================================

# ── Stage 1: Build ────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0-windowsservercore-ltsc2022 AS build

WORKDIR /src

# Copy project file and restore dependencies first (layer caching)
COPY SmartEPS.csproj ./
RUN dotnet restore SmartEPS.csproj

# Copy all source files
COPY Console/  ./Console/
COPY WinForms/ ./WinForms/
COPY ML/       ./ML/

# Publish self-contained release build
RUN dotnet publish SmartEPS.csproj \
      --configuration Release \
      --runtime win-x64 \
      --self-contained true \
      --output /app/publish \
      /p:PublishSingleFile=true \
      /p:IncludeNativeLibrariesForSelfExtract=true

# ── Stage 2: Runtime ─────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/runtime:8.0-windowsservercore-ltsc2022 AS runtime

WORKDIR /app

# Copy published artefacts
COPY --from=build /app/publish ./

# ML model directory (model will be trained/stored here on first run)
RUN mkdir models

# Expose a health-check endpoint via stdout (WinForms apps don't bind ports)
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
  CMD tasklist | findstr "SmartEPS.exe" || exit 1

# Environment variable: point model path to container directory
ENV MODEL_PATH=models/performance_model.zip

# Launch the WinForms application
# Requires a display / RDP session on Windows containers
ENTRYPOINT ["SmartEPS.exe"]