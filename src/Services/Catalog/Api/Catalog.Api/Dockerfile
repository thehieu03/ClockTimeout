# Build stage - Tối ưu để giảm kích thước và RAM
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files for dependency restoration
COPY ["Directory.Build.props", "Directory.Packages.props", "./"]
COPY ["src/Shared/Common/Common.csproj", "src/Shared/Common/"]
COPY ["src/Services/Catalog/Core/Catalog.Domain/Catalog.Domain.csproj", "src/Services/Catalog/Core/Catalog.Domain/"]
COPY ["src/Services/Catalog/Core/Catalog.Application/Catalog.Application.csproj", "src/Services/Catalog/Core/Catalog.Application/"]
COPY ["src/Services/Catalog/Core/Catalog.Infrastructure/Catalog.Infrastructure.csproj", "src/Services/Catalog/Core/Catalog.Infrastructure/"]
COPY ["src/Services/Catalog/Api/Catalog.Api/Catalog.Api.csproj", "src/Services/Catalog/Api/Catalog.Api/"]

# Restore dependencies
RUN dotnet restore "src/Services/Catalog/Api/Catalog.Api/Catalog.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/Services/Catalog/Api/Catalog.Api"
# Restore lại sau khi copy để đảm bảo tất cả packages được restore đầy đủ
RUN dotnet restore "Catalog.Api.csproj" \
    && dotnet build "Catalog.Api.csproj" -c Release -o /app/build \
    && dotnet clean

# Publish stage - Cleanup sau khi publish
FROM build AS publish
RUN dotnet publish "Catalog.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false \
    && rm -rf /src/bin /src/obj

# Runtime stage - Sử dụng image chuẩn (có curl cho healthcheck)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

EXPOSE 80
EXPOSE 443

# Copy published files
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Catalog.Api.dll"]
