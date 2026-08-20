FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the solution file
COPY *.sln ./

# Copy all the csproj files to their respective folders
COPY src/AuthService.Api/*.csproj ./src/AuthService.Api/
COPY src/AuthService.Domain/*.csproj ./src/AuthService.Domain/
COPY src/AuthService.Application/*.csproj ./src/AuthService.Application/
COPY src/AuthService.Persistence/*.csproj ./src/AuthService.Persistence/

# Restore dependencies
RUN dotnet restore src/AuthService.Api/AuthService.Api.csproj

# Copy the rest of the code
COPY . .

# Publish the application
WORKDIR /app/src/AuthService.Api
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Build the final image using the ASP.NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Expose port (Vercel typically uses process.env.PORT but hardcoding EXPOSE 10000 and the ENV is fine as per your request)
EXPOSE 10000
ENV ASPNETCORE_URLS=http://+:10000

# Copy the published output from the build stage
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AuthService.Api.dll"]