FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["vki-schedule-telegram.csproj", "./"]
RUN dotnet restore "vki-schedule-telegram.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "vki-schedule-telegram.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "vki-schedule-telegram.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "vki-schedule-telegram.dll"]
