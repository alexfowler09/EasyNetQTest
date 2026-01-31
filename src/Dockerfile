FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# COPY ["EasyNetQTest.csproj", "EasyNetQTestv8/"]
# WORKDIR /src/EasyNetQTestv8
COPY . .
RUN dotnet restore "EasyNetQTest.csproj"
# WORKDIR "EasyNetQTestv8"
RUN dotnet build "EasyNetQTest.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EasyNetQTest.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EasyNetQTest.dll"]