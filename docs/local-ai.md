# Local AI setup

UpperCube uses the existing valuation pipeline for numeric estimates. The local
LLM only explains the calculated range and receives a small structured context.

1. Install Ollama.
2. Pull the local model:

   ```bash
   ollama pull llama3.1
   ```

3. Start Ollama:

   ```bash
   ollama serve
   ```

   If you use the Ollama desktop app, make sure it is running instead.

4. Check the local API:

   ```bash
   curl http://localhost:11434/api/tags
   ```

5. Check `src/UpperCube.Web/appsettings.Development.json`:

   ```json
   {
     "AI": {
       "Enabled": true,
       "Provider": "Ollama",
       "BaseUrl": "http://localhost:11434",
       "Model": "llama3.1",
       "TimeoutSeconds": 300,
       "MaxTokens": 128,
       "ContextLength": 1024,
       "Temperature": 0.2
     }
   }
   ```

6. Run the app:

   ```bash
   dotnet run --project src/UpperCube.Web
   ```

7. Open `/Estimator`.

If Ollama is disabled or unavailable, `/Estimator` still returns the numeric
valuation and shows an AI explanation fallback message.

## Docker Compose

`compose.yaml` includes an `ollama` service and a one-shot
`ollama-pull-llama31` service. The Web container uses:

```json
"AI": {
  "BaseUrl": "http://ollama:11434",
  "Model": "llama3.1",
  "TimeoutSeconds": 300,
  "MaxTokens": 128,
  "ContextLength": 512,
  "Temperature": 0.2
}
```

Run the full stack:

```bash
docker compose up -d --build
```

The first run downloads `llama3.1` into the `ollama_data` Docker volume. That
can take time, and the Web container waits until the model pull finishes before
starting. To watch the download:

```bash
docker compose logs -f ollama-pull-llama31
```

After the pull completes, check the model list:

```bash
docker compose exec ollama ollama list
```

For local `dotnet run`, keep `AI:BaseUrl` as `http://localhost:11434`.
