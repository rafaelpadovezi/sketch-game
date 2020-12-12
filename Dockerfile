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

# CD to the main project as dotnet 2.x publish requires this to send compiled files to the out folder
# PublishSingleFile and PublishTrimmed are ignored by the 2.x compiler (only available on 3.x)
WORKDIR ./src
RUN dotnet publish --runtime alpine-x64 --configuration Release --output out \
    -p:PublishSingleFile=true -p:PublishTrimmed=true

# Final layer based on Alpine Linux (ultra light-weight ~ 5MB)
FROM alpine:3.9.4 AS runtime-env
ARG MAIN_PROJECT_NAME
ARG DOTNETCORE_VERSION

# Installing some libraries required by .NET Core on Alpine Linux
RUN apk add --no-cache libstdc++ libintl icu curl bash

# Copies from the build environment the compiled files of the out folder
WORKDIR /app
COPY --from=build-env /app/src/out .

ENV ASPNETCORE_URLS=http://0.0.0.0:80

HEALTHCHECK CMD curl --fail http://localhost:8080/health/ready || exit 1

# Enable SSH https://docs.microsoft.com/en-us/azure/app-service/configure-custom-container?pivots=container-linux#enable-ssh
RUN apk add openssh \
     && echo "root:Docker!" | chpasswd
COPY sshd_config /etc/ssh/
EXPOSE 2222 80

COPY ./scripts/start-app.sh ./start-app.sh
RUN chmod u+x ./start-app.sh
ENTRYPOINT ["./start-app.sh"]