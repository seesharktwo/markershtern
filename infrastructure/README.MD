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
- kafka, 3 repl of mongodb

Profiles avail:
- monitor: node_exporter + prometheus + grafana
```

### Connection ports
```md
- Kafka: 9092
- Grafana: 3030
- Mongodb: 27017, 27018, 27019
```

### Default credentials
```md
- Mongo: admin:admin
```

### Set memory limits for JVM via env vars
- KAFKA_MAX_MEM, default: 1024m

### Monitoring
[About stack](https://habr.com/ru/amp/post/652185/)

#### Node_exporter + Prometheus
- [Basics](https://prometheus.io/docs/guides/node-exporter/)
- Node_exporter url: node_exporter:9100

#### Grafana + Prometheus
- [Basics](https://prometheus.io/docs/visualization/grafana/)
- Prometheus url: http://prometheus:9090
