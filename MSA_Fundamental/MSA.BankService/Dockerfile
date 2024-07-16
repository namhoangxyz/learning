FROM mcr.microsoft.com/dotnet/nightly/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 5004
EXPOSE 15004

RUN mkdir -p startup
COPY ./aspnet/https/update-ca-certificate-bank.sh /startup/
RUN chmod +x /startup/update-ca-certificate-bank.sh

FROM mcr.microsoft.com/dotnet/sdk:7.0-jammy AS build
WORKDIR /src
COPY ["./MSA.BankService/MSA.BankService.csproj","src/MSA.BankService/"]
COPY ["./MSA.Common.Contracts/MSA.Common.Contracts.csproj","src/MSA.Common.Contracts/"]
COPY ["./MSA.Common.Mongo/MSA.Common.Mongo.csproj","src/MSA.Common.Mongo/"]
COPY ["./MSA.Common.Security/MSA.Common.Security.csproj","src/MSA.Common.Security/"]
RUN dotnet restore "src/MSA.BankService/MSA.BankService.csproj"
COPY . .
WORKDIR "/src/MSA.BankService"
RUN dotnet build "MSA.BankService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MSA.BankService.csproj" -c Release -o /app/publish --self-contained false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["/startup/update-ca-certificate-bank.sh"]