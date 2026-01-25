FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY *.sln .
COPY src/*.csproj ./src/
RUN dotnet restore ./src/

WORKDIR /source/src
COPY ./src/ .

RUN dotnet build -c Release --no-restore
RUN dotnet publish -c Release --no-build -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "FoodDeliveryAppAPI.dll"]