FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY [".", "SharpBot/"]
RUN dotnet restore "SharpBot/SharpBot.csproj"
#COPY . .
WORKDIR "/src/SharpBot"
RUN dotnet build "SharpBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SharpBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV TZ=Asia/Shanghai
#https://github.com/dotnet/aspnetcore/issues/32842
ENV ASPNETCORE_URLS=http://0.0.0.0:5115  
ENTRYPOINT ["dotnet", "SharpBot.dll"]
