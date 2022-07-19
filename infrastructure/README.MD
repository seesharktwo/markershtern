## Environment quick setup
### Prerequisites
* Docker [download](https://www.docker.com/get-started/)
* Docker-compose

### Run
```cmd
docker compose up -d
```

### Connection ports
```fix
- Zookeeper: 2181
- Kafka: 9092
```

### Set memory limits for JVM via env vars
- ZOO_MAX_MEM, default: 1024m
- KAFKA_MAX_MEM, default: 1024m
