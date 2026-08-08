FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY StudyAI.sln .
COPY backend/src backend/src
RUN dotnet restore backend/src/StudyAI.Api/StudyAI.Api.csproj
RUN dotnet publish backend/src/StudyAI.Api/StudyAI.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "StudyAI.Api.dll"]
