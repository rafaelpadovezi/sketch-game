# Main variables required for the Docker layers
ARG MAIN_PROJECT_NAME=Sketch
ARG DOTNETCORE_VERSION=3.1

# Starting layer point using build image with dotnet SDK (very heavy image ~ 2GB)
FROM mcr.microsoft.com/dotnet/core/sdk:$DOTNETCORE_VERSION AS build-env
ARG MAIN_PROJECT_NAME
ARG DOTNETCORE_VERSION

WORKDIR /app

# Restores (downloads) all NuGet packages from all projects of the solution (Test is ignored)
COPY . ./
RUN dotnet restore ./src/$MAIN_PROJECT_NAME.csproj

# Ef tools installation
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"