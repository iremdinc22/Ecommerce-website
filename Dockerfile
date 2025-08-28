# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj ve restore
COPY ["Eticaret.WebUI/Eticaret.WebUI.csproj", "Eticaret.WebUI/"]
COPY ["Eticaret.Service/Eticaret.Service.csproj", "Eticaret.Service/"]
COPY ["Eticaret.Data/Eticaret.Data.csproj", "Eticaret.Data/"]
COPY ["Eticaret.Core/Eticaret.Core.csproj", "Eticaret.Core/"]
RUN dotnet restore "Eticaret.WebUI/Eticaret.WebUI.csproj"

# copy full solution ve publish
COPY . .
RUN dotnet publish "Eticaret.WebUI/Eticaret.WebUI.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "Eticaret.WebUI.dll"]

