FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

RUN apt-get update && \
    apt-get install -y libgdiplus && \
    if [ ! -f /usr/lib/libgdiplus.so ]; then \
        ln -s /usr/lib/x86_64-linux-gnu/libgdiplus.so /usr/lib/; \
    fi && \
    rm -rf /var/lib/apt/lists/*

EXPOSE 80
ENTRYPOINT ["dotnet", "WebApplication2.dll"]