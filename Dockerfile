FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["CineFlow.Domain/CineFlow.Domain.csproj", "CineFlow.Domain/"]
COPY ["CineFlow.Application/CineFlow.Application.csproj", "CineFlow.Application/"]
COPY ["CineFlow.Infrastructure/CineFlow.Infrastructure.csproj", "CineFlow.Infrastructure/"]
COPY ["CineFlow.Api/CineFlow.Api.csproj", "CineFlow.Api/"]

RUN dotnet restore "CineFlow.Api/CineFlow.Api.csproj"

COPY . .
WORKDIR "/src/CineFlow.Api"
RUN dotnet publish "CineFlow.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "CineFlow.Api.dll"]
