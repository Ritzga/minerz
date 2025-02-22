# Build
FROM cakebuild/cakesdk:8.0 AS build

COPY .publish/ ./

# WORKDIR /app


ENTRYPOINT ["dotnet", "api.dll"]