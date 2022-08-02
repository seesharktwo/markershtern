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
docker compose --profile profile_name [--profile another_profile] up -d

Always starts up:
- kafka, mongo1 (primary), mongo2, mongo3

Profiles avail:
- monitor: node_exporter + prometheus + grafana
- logging: loki + promtail
```

### Connection ports
```md
- Kafka: 9092
- Grafana: 3030
- Mongodb: 27017 (primary), 27018, 27019
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

### Logging
- [About stack](https://grafana.com/oss/loki/)
- [More about Loki](https://habr.com/ru/company/badoo/blog/507718/)

#### Docker setup
If you add new container and want to see it with proper info in Grafana,
you have to run all docker containers with these args:

```cmd
--log-driver json-file --log-opt tag="{{.ImageName}}|{{.Name}}|{{.ImageFullID}}|{{.FullID}}"
```

#### Promtail + Loki
- [Basics](https://grafana.com/docs/loki/latest/clients/promtail/)
- Promtail url: http://promtail:9080

#### Grafana + Loki
- [Basics](https://youtu.be/HDpE9v1Syz8)
- Loki url: http://loki:3100
