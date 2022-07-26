## Environment quick setup
### Prerequisites
* Docker [download](https://www.docker.com/get-started/)
* Docker-compose

### Run
```cmd
docker compose up -d
```

### Profiles
```cmd
docker compose --profile profile_name up -d

Always starts up:
- kafka

Profiles avail:
- monitor: node_exporter + prometheus + grafana
```

### Connection ports
```md
- Kafka: 9092
- Grafana: 3030
```

### Set memory limits for JVM via env vars
- KAFKA_MAX_MEM, default: 1024m
