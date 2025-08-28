# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# csproj'ları kopyala ve restore et
COPY ["Eticaret.WebUI/Eticaret.WebUI.csproj", "Eticaret.WebUI/"]
COPY ["Eticaret.Service/Eticaret.Service.csproj", "Eticaret.Service/"]
COPY ["Eticaret.Data/Eticaret.Data.csproj", "Eticaret.Data/"]
COPY ["Eticaret.Core/Eticaret.Core.csproj", "Eticaret.Core/"]
RUN dotnet restore "Eticaret.WebUI/Eticaret.WebUI.csproj"

# tüm kaynakları kopyala ve publish et
COPY . .
RUN dotnet publish "Eticaret.WebUI/Eticaret.WebUI.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Render konteynerlerinin iç portu (en sorunsuz ayar)
ENV PORT=10000
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}
ENV DOTNET_ENVIRONMENT=Production
EXPOSE 10000

ENTRYPOINT ["dotnet", "Eticaret.WebUI.dll"]

