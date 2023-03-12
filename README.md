## Сводка
Сервис менеджмента тренировок.

## Сервисы

```shell
# запуск всех сервисов
docker-compose up
```
```shell
# запуск отдельного сервиса
docker-compose --profile identity up
```

| Путь | Описание | Профиль docker-compose |
| ------ |--------|------------------------|
| identity_server  | Сервис Авторизации и аутентификации (OAuth 2.0/OpenID Connect) | identity               |