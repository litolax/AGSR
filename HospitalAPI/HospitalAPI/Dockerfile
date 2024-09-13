FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

EXPOSE 8080
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY ["HospitalAPI/HospitalAPI.csproj", "HospitalAPI/"]
RUN dotnet restore "HospitalAPI/HospitalAPI.csproj"

COPY . .
WORKDIR "/src/HospitalAPI"
RUN dotnet build "HospitalAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HospitalAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "HospitalAPI.dll"]