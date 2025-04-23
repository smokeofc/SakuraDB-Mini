FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["SakuraDB-Mini.csproj", "./"]
RUN dotnet restore "SakuraDB-Mini.csproj"

# Copy the rest of the files and build
COPY . .
RUN dotnet build "SakuraDB-Mini.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "SakuraDB-Mini.csproj" -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app

# Create a non-root user
RUN groupadd -r appuser && \
    useradd -r -g appuser -u 1000 appuser

# Create directories for data
RUN mkdir -p /data/watch/folder1 \
    /data/out/folder1 \
    /data/db \
    /data/logs \
    /data/data/db && \
    chown -R appuser:appuser /data

# Copy published app
COPY --from=publish /app/publish .

# Switch to non-root user
USER appuser

# Set entrypoint
ENTRYPOINT ["dotnet", "SakuraDB-Mini.dll"]