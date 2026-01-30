FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "Directory.Packages.props", "./"]
COPY ["src/Services/Order/Api/Order.Api/Order.Api.csproj", "src/Services/Order/Api/Order.Api/"]
COPY ["src/Services/Order/Core/Order.Application/Order.Application.csproj", "src/Services/Order/Core/Order.Application/"]
COPY ["src/Shared/BuildingBlocks/BuildingBlocks.csproj", "src/Shared/BuildingBlocks/"]
COPY ["src/Shared/Common/Common.csproj", "src/Shared/Common/"]
COPY ["src/Services/Order/Core/Order.Domain/Order.Domain.csproj", "src/Services/Order/Core/Order.Domain/"]
COPY ["src/Services/Order/Core/Order.Infrastructure/Order.Infrastructure.csproj", "src/Services/Order/Core/Order.Infrastructure/"]
COPY ["src/Shared/Contracts/Discount.Contract/Discount.Contract.csproj", "src/Shared/Contracts/Discount.Contract/"]
COPY ["src/Shared/Contracts/Catalog.Contract/Catalog.Contract.csproj", "src/Shared/Contracts/Catalog.Contract/"]
RUN dotnet restore "src/Services/Order/Api/Order.Api/Order.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/Order/Api/Order.Api"
RUN dotnet build "./Order.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Order.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

USER root
RUN apt-get update && apt-get install -y \
    curl \
    net-tools \
    libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*
USER $APP_UID
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Order.Api.dll"]
