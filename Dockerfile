# Use official .NET 8 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["Wefaaq.Api/Wefaaq.Api.csproj", "Wefaaq.Api/"]
COPY ["Wefaaq.Bll/Wefaaq.Bll.csproj", "Wefaaq.Bll/"]
COPY ["Wefaaq.Dal/Wefaaq.Dal.csproj", "Wefaaq.Dal/"]

# Restore dependencies
RUN dotnet restore "Wefaaq.Api/Wefaaq.Api.csproj"

# Copy all source files
COPY . .

# Build and publish
WORKDIR "/src/Wefaaq.Api"
RUN dotnet publish "Wefaaq.Api.csproj" -c Release -o /app/publish

# Use official .NET 8 runtime for running
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expose port (Railway will set PORT environment variable)
EXPOSE 5000

# Set entry point
ENTRYPOINT ["dotnet", "Wefaaq.Api.dll"]
