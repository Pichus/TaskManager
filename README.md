
# Task Manager

Small Jira clone pet project


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

To run integration tests, run the following command. Ensure you have docker installed and running

```bash
  dotnet test tests/TaskManager.IntegrationTests
```

