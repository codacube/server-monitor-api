```bash
# Build
podman build -t custom-ollama ./ollama

# Run
podman run -d \
  --name server-monitor-ollama \
  -p 11434:11434 \
  -v ollama_models:/root/.ollama \
  custom-ollama

# Check startup progress
podman logs -f server-monitor-ollama

# Show running containers
podman ps
```
