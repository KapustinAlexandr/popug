# popug

# Запуск

## Docker Desktop

1. Сначала поднимаем инфраструктуру. `kav-async-arch` стек
    ```
    docker compose -f "infra\docker-compose.yml" up -d --build
    ```
2. Далее собираем и запускаем сервисы. `kav-async-arch-apps` стек.
   ```
   
   docker compose -f "src\docker-compose.yml" -f "src\docker-compose.override.yml" --project-name kav-async-arch-apps up -d --build
   ```
3. Ждём когда докер поднимет все контейнеры и настроится инфраструктура.

- **Auth Service** - http://localhost:8080 (Keycloak сервер)
  - Авторизация и регистрация пользователя http://localhost:8080/realms/popug/account
  - Админка сервера http://localhost:8080/admin/master/console/
    - Пользователь: admin/admin
- **Kafka UI** - http://localhost:39090/ui/clusters/popug/all-topics
  - [auth-admin-events](http://localhost:39090/ui/clusters/popug/all-topics/auth-admin-events) - топик для админских событий (смена группы/роли)
  - [auth-events](http://localhost:39090/ui/clusters/popug/all-topics/auth-events) - CUD пользователя и Login|Logout
- **Task Service** - http://localhost:31000/swagger/index.html
