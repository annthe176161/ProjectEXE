# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and project files
COPY ProjectEXE.sln .
COPY ProjectEXE/ProjectEXE.csproj ./ProjectEXE/
RUN dotnet restore

# Copy source code
COPY ProjectEXE/ ./ProjectEXE/

# Build and publish
WORKDIR /app/ProjectEXE
RUN dotnet publish -c Release -o /app/out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Set environment to Development for debugging
ENV ASPNETCORE_ENVIRONMENT=Development

EXPOSE 80
ENTRYPOINT ["dotnet", "ProjectEXE.dll"]