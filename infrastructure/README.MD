## Server Environment setup
### Prerequisites
* Docker [download](https://www.docker.com/get-started/)
* Docker-compose
* Ansible

### Add users
- Fill ansible/users_to_ssh.json

### Run
```cmd
    ansible-playbook ansible/initial-setup-playbook.yaml -l ubuntu -i ansible/inventory.ini -v -k
```

### Inventory groups
```cmd
- webui: loki + promtail + grafana + prometheus + kowl
- kafka: kafka + kraft
- mongo: mongodb
```

### Connection ports
```md
- Kafka: 9092
- Kraft: 9093
- Kowl: 8080
- Grafana: 3030
- Mongodb: 27017
```

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
