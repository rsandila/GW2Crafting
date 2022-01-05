#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["GW2Crafting/GW2Crafting.csproj", "GW2Crafting/"]
RUN dotnet restore "GW2Crafting/GW2Crafting.csproj"
COPY . .
WORKDIR "/src/GW2Crafting"
RUN dotnet build "GW2Crafting.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GW2Crafting.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GW2Crafting.dll"]