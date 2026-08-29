# Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY ["Server/Mephi.Cyber.AuthService.Server.csproj", "Server/"]
COPY ["Client/Mephi.Cyber.AuthService.Client.csproj", "Client/"]
COPY ["Shared/Mephi.Cyber.AuthService.Core.csproj", "Shared/"]

RUN dotnet restore "Server/Mephi.Cyber.AuthService.Server.csproj"

COPY . .
WORKDIR "/src/Server"
RUN dotnet build "Mephi.Cyber.AuthService.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Mephi.Cyber.AuthService.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app

COPY --from=publish /app/publish .

COPY --from=build /src/Server/Properties ./Properties

ARG PORT=8080
ENV ASPNETCORE_URLS=http://+:${PORT}
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE ${PORT}

ENTRYPOINT ["dotnet", "Mephi.Cyber.AuthService.Server.dll"]
