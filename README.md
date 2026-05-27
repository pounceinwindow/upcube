# UpperCube


## Запуск через dotnet run

1. Поднять инфраструктуру:

```bash
docker compose up -d --build
```

может долго пулиться из за ollama

3. Запустить Web:

```bash
dotnet run --project src/UpperCube.Web
```

4. Открыть сайт:

```text
http://localhost:5204
```


## Полезные адреса

- Сайт: `http://localhost:5204` или `http://localhost:8080` в Docker
- Estimator: `/Estimator`
- Compare: `/Compare`
- Admin: `/Admin`
- Ollama API: `http://localhost:11434`

## Тестовые аккаунты

```text
admin@uppercube.local / Admin123!
agent@uppercube.local / Agent123!
user@uppercube.local  / User1234!
```
