# 1. Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Proje dosyalarını kopyala
COPY *.sln .
COPY HotelBookingAPI/*.csproj ./HotelBookingAPI/
RUN dotnet restore

# Tüm projeyi kopyala
COPY . .
WORKDIR /app/HotelBookingAPI
RUN dotnet publish -c Release -o out

# 2. Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/HotelBookingAPI/out .
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
ENV DOTNET_HOST_PATH=/usr/share/dotnet/dotnet

# JWT_KEY environment variable
ENV JWT_KEY=${JWT_KEY}

# Port
EXPOSE 5000
ENTRYPOINT ["dotnet", "HotelBookingAPI.dll"]
