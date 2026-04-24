FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/UpperCube.Web/UpperCube.Web.csproj", "src/UpperCube.Web/"]
COPY ["src/UpperCube.Application/UpperCube.Application.csproj", "src/UpperCube.Application/"]
COPY ["src/UpperCube.Domain/UpperCube.Domain.csproj", "src/UpperCube.Domain/"]
COPY ["src/UpperCube.Infrastructure/UpperCube.Infrastructure.csproj", "src/UpperCube.Infrastructure/"]
RUN dotnet restore "src/UpperCube.Web/UpperCube.Web.csproj"
COPY . .
RUN dotnet build "src/UpperCube.Web/UpperCube.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "src/UpperCube.Web/UpperCube.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UpperCube.Web.dll"]
