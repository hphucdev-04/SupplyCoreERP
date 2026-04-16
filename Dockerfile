# Stage 1: Build .NET
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy các file config NuGet và props trước
COPY ["NuGet.Config", "./"]
COPY ["common.props", "./"]

# Copy các project files để restore (Tận dụng Docker Cache)
COPY ["src/SupplyCoreERP.HttpApi.Host/SupplyCoreERP.HttpApi.Host.csproj", "src/SupplyCoreERP.HttpApi.Host/"]
COPY ["src/SupplyCoreERP.Application/SupplyCoreERP.Application.csproj", "src/SupplyCoreERP.Application/"]
COPY ["src/SupplyCoreERP.Application.Contracts/SupplyCoreERP.Application.Contracts.csproj", "src/SupplyCoreERP.Application.Contracts/"]
COPY ["src/SupplyCoreERP.Domain/SupplyCoreERP.Domain.csproj", "src/SupplyCoreERP.Domain/"]
COPY ["src/SupplyCoreERP.Domain.Shared/SupplyCoreERP.Domain.Shared.csproj", "src/SupplyCoreERP.Domain.Shared/"]
COPY ["src/SupplyCoreERP.EntityFrameworkCore/SupplyCoreERP.EntityFrameworkCore.csproj", "src/SupplyCoreERP.EntityFrameworkCore/"]

RUN dotnet restore "src/SupplyCoreERP.HttpApi.Host/SupplyCoreERP.HttpApi.Host.csproj"

# Copy toàn bộ code và build
COPY . .
WORKDIR "/app/src/SupplyCoreERP.HttpApi.Host"
RUN dotnet publish "SupplyCoreERP.HttpApi.Host.csproj" -c Release -o /publish

# Stage 2: Run
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /publish .

# Railway dùng biến $PORT, mặc định thường là 8080
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SupplyCoreERP.HttpApi.Host.dll"]