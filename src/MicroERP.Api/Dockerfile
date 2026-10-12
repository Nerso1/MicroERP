# 1st stage -> build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1.1. firstly only project files -> restore goes to cache and doesn't repeat itself after changing the code
COPY ["src/MicroERP.Domain/MicroERP.Domain.csproj", "src/MicroERP.Domain/"]
COPY ["src/MicroERP.Application/MicroERP.Application.csproj", "src/MicroERP.Application/"]
COPY ["src/MicroERP.Infrastructure/MicroERP.Infrastructure.csproj", "src/MicroERP.Infrastructure/"]
COPY ["src/MicroERP.Api/MicroERP.Api.csproj", "src/MicroERP.Api/"]
RUN dotnet restore "src/MicroERP.Api/MicroERP.Api.csproj"

# 1.2. secondly: rest of the code
COPY src/ src/
RUN dotnet publish "src/MicroERP.Api/MicroERP.Api.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

# 2nd stage -> runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# The aspnet image has a built-in non-root user - we do not run the application as root
USER $APP_UID
EXPOSE 8080

ENTRYPOINT ["dotnet", "MicroERP.Api.dll"]
