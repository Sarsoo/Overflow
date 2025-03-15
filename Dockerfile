FROM mcr.microsoft.com/dotnet/sdk:9.0 AS base

RUN dotnet workload install wasm-tools
RUN apt update && apt install -y python3

COPY *.sln .
COPY Overflow/*.csproj ./Overflow/
COPY Overflow.Web/*.csproj ./Overflow.Web/
COPY Overflow.Web.Client/*.csproj ./Overflow.Web.Client/
RUN dotnet restore ./Overflow.Web/Overflow.Web.csproj

COPY . ./

FROM base as publish
RUN dotnet publish Overflow.Web/Overflow.Web.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0
EXPOSE 80
WORKDIR /app
COPY --from=publish /app ./
ENV DOTNET_EnableDiagnostics=0

USER app

ENTRYPOINT ["dotnet", "Overflow.Web.dll"]
