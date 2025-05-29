FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["eCHU.csproj", "."]
RUN dotnet restore "./eCHU.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "eCHU.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "eCHU.csproj" -c Release -o /app/publish

FROM runtime AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "eCHU.dll"]
