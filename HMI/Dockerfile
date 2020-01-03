FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim-arm32v7 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["HMI/HMI.csproj", "HMI/"]
RUN dotnet restore "HMI/HMI.csproj"
COPY . .
WORKDIR "/src/HMI"
RUN dotnet build "HMI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HMI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HMI.dll"]
