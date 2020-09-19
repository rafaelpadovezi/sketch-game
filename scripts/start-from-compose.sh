#!/bin/bash
PROJECT_NAME=Sketch
CSPROJ_PATH=./src/$PROJECT_NAME.csproj

echo "[PRE-RUN]: Building project..."
dotnet build $CSPROJ_PATH

echo "[PRE-RUN]: Watching and running project..."
dotnet watch --project $CSPROJ_PATH run --urls http://$ASPNETCORE_SOCKET_BIND_ADDRESS:$ASPNETCORE_SOCKET_BIND_PORT