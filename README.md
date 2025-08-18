
# Task Manager

A simplified task management system inspired by Jira


## Run Locally

Clone the project

```bash
  git clone https://github.com/Pichus/TaskManager.git
```

Go to the project directory

```bash
  cd TaskManager
```

Ensure you have docker and docker compose installed 

Run the project using docker compose

```bash
  docker compose up
```


## Running Tests

To run all tests, run the following command

```bash
  dotnet test
```

To run unit tests, run the following command

```bash
  dotnet test tests/TaskManager.UnitTests
```

To run integration tests, run the following command. Ensure you have docker installed and running.
(In progress)

```bash
  dotnet test tests/TaskManager.IntegrationTests
```

## Roadmap

- Add integration tests

- Improve code quality and maintainability

- Refine Domain-Driven Design practices:
    - Strengthen layer isolation (remove unnecessary references from use cases to infrastructure)
    - Implement domain events

- Introduce MediatR and adopt the CQRS pattern

- Introduce AutoMapper to reduce mapping boilerplate

