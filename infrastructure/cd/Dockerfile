FROM mcr.microsoft.com/dotnet/sdk:6.0
WORKDIR /app

COPY . ./
RUN dotnet build $SRC_PATH -c Release -o build

ENTRYPOINT ["dotnet", "build/$SERVICE_NAME.dll"]