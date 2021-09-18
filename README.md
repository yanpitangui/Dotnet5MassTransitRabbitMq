# Masstransit + Rabbitmq + ElasticSearch + Kibana
In this project, you will see an implementation of pub/sub and request/response using MassTransit and RabbitMQ as a broker.

## How to run
1. Run this command on the root folder: 
    ```csharp
    dotnet dev-certs https -ep https/aspnetapp.pfx -p yourpassword
    ```
    Replace "yourpassword" with something else in this command and the docker-compose.override.yml file. This creates the https certificate.


2. Run docker-compose up -d in the root directory, or, in visual studio, set the docker-compose project as startup and run. This should start both the consoleapp and the webapi, and also RabbitMQ, ElasticSearch and Kibana.

3. Visit https://localhost:5001/swagger/index.html to access the application's swagger.

4. To see your logs on Kibana, go to http://localhost:5601/.
    1. Click on the Hamburger button (☰) on the upper left, then go to Discover, under Analytics.
    2. Click on `Create index pattern`. On the text field, enter `consumerapp-*` and press `Next step`.
    3. On the time field, select `@timestamp` and press `Create index pattern`.
    4. On the left side, click on `Index Patterns`, under `Kibana`.
    5. Repeat steps 2 and 3, but on the text field, enter `webapi-*`.
    6. After that, click on Hamburger button (☰) and go to discover. You should see some logs. You can select each application logs by changing the dropdown on the left, changing the index pattern.