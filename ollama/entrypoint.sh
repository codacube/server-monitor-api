#!/bin/sh

# Start Ollama server in background and capture its process ID
/bin/ollama serve &
OLLAMA_PID=$!

# Wait until Ollama API is responsive
until ollama list > /dev/null 2>&1; do
  sleep 1
done

# Pull the lightweight function-calling model automatically
echo "Loading llama3.2 model..."
ollama pull llama3.2

# Hand execution back to the background server process
wait $OLLAMA_PID