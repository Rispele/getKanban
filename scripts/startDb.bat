docker pull postgres:latest
docker run --name get-kanban-postgres --env=POSTGRES_USER=usr --env=POSTGRES_PASSWORD=pwd --env=POSTGRES_DB=db -p 5432:5432 -d postgres:latest