
# Standing Order Case

This project is a .NET Core API for managing standing orders. It includes an implementation of the following features:

-   Create a standing order for a user
-   Get a standing order by ID
-   Get all standing orders for a user
-   Get all notifications for a standing order
-   Cancel a standing order

## Running the Project

To run the project, you will need to have [Docker](https://www.docker.com/) installed on your machine.

1.  Navigate to the project directory:


`cd standing-order-case` 

2.  Build and start the Docker containers:


`docker-compose up -d` 

This will start the API and RabbitMQ instances in the background.

## API Documentation

You can view the API documentation and test the endpoints using the Swagger UI at `http://localhost:8080/swagger`.

## RabbitMQ Management

You can manage the RabbitMQ instance using the RabbitMQ Management UI at `http://localhost:15672`. The default credentials are `guest:guest`.

I hope this helps! Let me know if you have any questions.