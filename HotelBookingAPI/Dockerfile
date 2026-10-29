# 1. Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Proje dosyasını kopyala ve restore et
COPY HotelBookingAPI.csproj ./
RUN dotnet restore

# Proje dosyalarını kopyala
COPY . .
RUN dotnet publish HotelBookingAPI.csproj -c Release -o out

# 2. Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/out .

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "HotelBookingAPI.dll"]
