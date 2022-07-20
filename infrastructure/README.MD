## Environment quick setup
### Prerequisites
* Docker [download](https://www.docker.com/get-started/)
* Docker-compose

### Run
```cmd
docker compose up -d
```

### Connection ports
```md
- Kafka: 9092
```

### Set memory limits for JVM via env vars
- KAFKA_MAX_MEM, default: 1024m
