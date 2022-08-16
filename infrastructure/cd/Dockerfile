FROM mcr.microsoft.com/dotnet/sdk:6.0
WORKDIR /app

ARG CSPROJ_PATH
ARG EXECUTABLE_NAME
ARG Mongo_ConnectionString
ARG KafkaSettings_BootstrapServers

ENV EXECUTABLE_NAME $EXECUTABLE_NAME
ENV Mongo_ConnectionString "mongodb://mongo1:27017"
ENV KafkaSettings_BootstrapServers "kafka:9093"

COPY . ./
RUN dotnet build $CSPROJ_PATH -c Release -o build

WORKDIR /app/build

ENTRYPOINT dotnet ${EXECUTABLE_NAME}