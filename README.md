> [!WARNING]
> This project is still under heavy development and is not ready to be used in production environments. 

**Logport** is a self-hosted log aggregation and analytics platform designed to be the central **port** for all your application logs.

Just like a real port connects ships from around the world, Logport connects logs from every corner of your system 
from lightweight services to enterprise-scale applications and brings them together in one place for analysis and insight.


## Docker Injection
LogPort supports tailing logs directly from Docker containers. To enable this feature, you need to enable the Docker injection when starting the agent and mark the containers you want to monitor with a specific label.

### Enabling Docker Injection
To enable Docker injection, set `LOGPORT_USE_DOCKER=true` when starting the Logport agent. This tells the agent to look for Docker containers with the appropriate label:
```bash
docker run -e LOGPORT_USE_DOCKER=true thiagomvas/logport:latest
```

### Labeling Containers
To have the Logport agent monitor a specific Docker container, you need to add the label `com.logport.monitor=true` to that container. You can do this when starting the container using the `--label` flag:
```bash
docker run --label com.logport.monitor=true your-container-image
```

Or updating your compose file:
```yaml
version: '3'
services:
  your-service:
    image: your-container-image
    labels:
      - "com.logport.monitor=true"
```

### Custom Extractors
If you need to customize how logs are extracted from your Docker containers, you can define custom extractors in the Logport configuration file. This allows you to specify patterns or formats that match your log output. Below is an example of how to define a custom extractor, the following works for elastic search:
```json
[
    {
    "ServiceName": "logport-elasticsearch",
    "ExtractionMode": "json",
    "MessageKey": "message",
    "TimestampKey": "@timestamp",
    "LogLevelKey": "log.level"
    }
]
```

It also supports regex based extractors:
```json
[
    {
    "ServiceName": "some-service",
    "ExtractionMode": "regex",
    "RegexPattern": "\\[(?P<timestamp>[^\\]]+)\\] \\[(?P<loglevel>[^\\]]+)\\] (?P<message>.+)",
    }
]
```

Make sure to adjust the `ServiceName`, `ExtractionMode`, and other keys according to your log format and requirements.