FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-alpine AS build
WORKDIR /src
COPY ["ClamAv.Net.csproj", ""]
RUN dotnet restore "./ClamAv.Net.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "ClamAv.Net.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ClamAv.Net.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ClamAv.Net.dll"]