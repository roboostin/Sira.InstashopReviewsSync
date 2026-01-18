# Use official .NET 9 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 8080

# Use .NET 9 SDK to build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the project files
COPY ["API.csproj", "./"]
RUN dotnet restore "./API.csproj"

# Copy the entire application and build it
COPY . .
RUN dotnet publish "./API.csproj" -c Release -o /app/publish

# Final image with the built application
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV DOTNET_gcServer=1
ENV DOTNET_GCHeapHardLimit=800000000


ENTRYPOINT ["dotnet", "API.dll"]
