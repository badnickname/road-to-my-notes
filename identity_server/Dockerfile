FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 5010

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["MyNotes.Identity/MyNotes.Identity.csproj", "MyNotes.Identity/"]
RUN dotnet restore "MyNotes.Identity/MyNotes.Identity.csproj"
COPY . .
WORKDIR "/src/MyNotes.Identity"
RUN dotnet build "MyNotes.Identity.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MyNotes.Identity.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyNotes.Identity.dll"]
