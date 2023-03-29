FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["StudyOrganizer/StudyOrganizer.csproj", "StudyOrganizer/"]
COPY ["YandexSpeechKitApi/YandexSpeechKitApi.csproj", "YandexSpeechKitApi/"]
RUN dotnet restore "StudyOrganizer/StudyOrganizer.csproj"
COPY . .
WORKDIR "/src/StudyOrganizer"
RUN dotnet build "StudyOrganizer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StudyOrganizer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StudyOrganizer.dll"]
