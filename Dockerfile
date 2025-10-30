FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

COPY *.sln .
COPY src/*.csproj ./src/
COPY tests/UnitTests/*.csproj ./tests/UnitTests/
RUN dotnet restore

COPY . .

RUN dotnet build -c Release --no-restore
RUN dotnet test -c Release --no-build
WORKDIR /source/src
RUN dotnet publish -c Release --no-build -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "FoodDeliveryAppAPI.dll"]