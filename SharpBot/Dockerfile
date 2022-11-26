FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
RUN apt-get update
RUN apt-get install python3 -y
RUN apt-get install python3-pip -y
RUN pip install requests
RUN apt-get purge python3-pip -y
RUN apt-get autoremove -y
RUN apt-get clean
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
