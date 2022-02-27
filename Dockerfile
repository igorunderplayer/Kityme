FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /source

COPY ./Kityme/Kityme.csproj ./
COPY ./Kityme/nuget.config ./
RUN dotnet restore

COPY ./Kityme .

RUN dotnet publish -c release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Kityme.dll"]
