FROM docker.io/ollama/ollama:latest

# Copy startup script
COPY entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

# Expose Ollama default REST port
EXPOSE 11434

ENTRYPOINT ["/entrypoint.sh"]