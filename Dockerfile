# 1. Base image cho runtime (đổi :8.0 thành 7.0 hoặc 6.0 nếu project của bạn dùng bản .NET cũ hơn)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
# Port mặc định cho .NET 8 trong container thường là 8080. Railway sẽ tự động map port này.
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# 2. Build image với SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs && \
    rm -rf /var/lib/apt/lists/*

# Verify Node and NPM are installed
RUN node --version && npm --version

# CLI 
RUN dotnet tool install -g Volo.Abp.Cli
ENV PATH="${PATH}:/root/.dotnet/tools"

# Copy toàn bộ mã nguồn vào container
COPY . .

RUN abp install-libs

# Di chuyển đến thư mục chứa project Host
WORKDIR "/src/src/SupplyCoreERP.HttpApi.Host"

# Restore dependencies
RUN dotnet restore "SupplyCoreERP.HttpApi.Host.csproj"

# Build source code
RUN dotnet build "SupplyCoreERP.HttpApi.Host.csproj" -c Release -o /app/build

# 3. Publish ứng dụng
FROM build AS publish
RUN dotnet publish "SupplyCoreERP.HttpApi.Host.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 4. Final stage - Copy file đã publish sang base runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY src/SupplyCoreERP.HttpApi.Host/openiddict.pfx .

# Định nghĩa lệnh chạy khi container start
ENTRYPOINT ["dotnet", "SupplyCoreERP.HttpApi.Host.dll"]