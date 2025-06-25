FROM mcr.microsoft.com/dotnet/sdk:9.0 AS dev

WORKDIR /app

COPY hub.csproj ./

RUN dotnet restore hub.csproj

COPY . ./

WORKDIR /app

EXPOSE 5000

ENTRYPOINT ["dotnet", "watch", "run", "--urls=http://0.0.0.0:5000"]
