# Build context: backend/  (docker compose sets it; manually: docker build -t grocerylist-api backend)

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first, using only the project files, so the layer is cached until dependencies change.
COPY GroceryList.slnx ./
COPY src/GroceryList.Domain/GroceryList.Domain.csproj src/GroceryList.Domain/
COPY src/GroceryList.Application/GroceryList.Application.csproj src/GroceryList.Application/
COPY src/GroceryList.Infrastructure/GroceryList.Infrastructure.csproj src/GroceryList.Infrastructure/
COPY src/GroceryList.Api/GroceryList.Api.csproj src/GroceryList.Api/
RUN dotnet restore src/GroceryList.Api/GroceryList.Api.csproj

COPY src/ src/
RUN dotnet publish src/GroceryList.Api/GroceryList.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# The official images ship a non-root "app" user; its uid is exposed as APP_UID.
USER $APP_UID

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "GroceryList.Api.dll"]
