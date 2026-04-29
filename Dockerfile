# Stage 1: SDK stage for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Install Node.js 20.x
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs

# Install Hugo
ARG HUGO_VERSION=0.154.5
RUN wget -O hugo.tar.gz https://github.com/gohugoio/hugo/releases/download/v${HUGO_VERSION}/hugo_extended_${HUGO_VERSION}_linux-amd64.tar.gz && \
    tar -xzf hugo.tar.gz -C /usr/local/bin && \
    rm hugo.tar.gz

WORKDIR /src

# Copy solution and project files
COPY *.sln ./
COPY nuget.config ./
COPY btsx/*.csproj ./btsx/
COPY btsxcli/*.csproj ./btsxcli/
COPY btsxweb/*.csproj ./btsxweb/

# Copy remaining source files
COPY btsx/ ./btsx/
COPY btsxcli/ ./btsxcli/
COPY btsxweb/ ./btsxweb/
COPY docs/ ./docs/

# Build Hugo documentation
WORKDIR /src/docs
RUN bash setup-theme.sh
RUN npm install
RUN npm run build

# Install npm dependencies
WORKDIR /src/btsxweb
RUN npm install

# Build frontend
RUN npm run build-prd

# Build backend
WORKDIR /src
RUN dotnet restore BTSX.sln
RUN dotnet build BTSX.sln -c Release --no-restore

# Publish btsxweb
RUN dotnet publish btsxweb/btsxweb.csproj -c Release -o /app/publish --no-build

# Stage 2: Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime


# Copy Hugo output to /docs
WORKDIR /docs/public
COPY --from=build /src/docs/public .

WORKDIR /app

# Copy published application
COPY --from=build /app/publish .

# Set ASP.NET Core environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port
EXPOSE 8080

# Set entrypoint
ENTRYPOINT ["dotnet", "btsxweb.dll"]
