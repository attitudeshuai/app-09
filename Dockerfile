FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/KnowledgeBase.API/KnowledgeBase.API.csproj", "src/KnowledgeBase.API/"]
COPY ["src/KnowledgeBase.Application/KnowledgeBase.Application.csproj", "src/KnowledgeBase.Application/"]
COPY ["src/KnowledgeBase.Domain/KnowledgeBase.Domain.csproj", "src/KnowledgeBase.Domain/"]
COPY ["src/KnowledgeBase.Infrastructure/KnowledgeBase.Infrastructure.csproj", "src/KnowledgeBase.Infrastructure/"]
RUN dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org
RUN dotnet restore "src/KnowledgeBase.API/KnowledgeBase.API.csproj"
COPY . .
WORKDIR "/src/src/KnowledgeBase.API"
RUN dotnet build "KnowledgeBase.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KnowledgeBase.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KnowledgeBase.API.dll"]
