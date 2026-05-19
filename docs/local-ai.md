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
       "TimeoutSeconds": 60
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

If the Web app is later run in Docker, change `AI:BaseUrl` to
`http://host.docker.internal:11434`. For local `dotnet run`, keep
`http://localhost:11434`.
