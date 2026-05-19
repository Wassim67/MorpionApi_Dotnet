FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MorpionApi_DotNet/MorpionApi_DotNet.csproj", "MorpionApi_DotNet/"]
RUN dotnet restore "MorpionApi_DotNet/MorpionApi_DotNet.csproj"
COPY . .
WORKDIR "/src/MorpionApi_DotNet"
RUN dotnet build "./MorpionApi_DotNet.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MorpionApi_DotNet.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MorpionApi_DotNet.dll"]
