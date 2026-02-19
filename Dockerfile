# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj and restore
COPY SortingProduct/SortingProduct.csproj SortingProduct/
RUN dotnet restore SortingProduct/SortingProduct.csproj

# copy everything else and publish
COPY . .
RUN dotnet publish SortingProduct/SortingProduct.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Railway sets PORT. ASP.NET Core will listen on this.
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

EXPOSE 8080

ENTRYPOINT ["dotnet", "SortingProduct.dll"]
