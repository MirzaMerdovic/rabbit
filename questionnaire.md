Questionnaire

1. Does it have managed deployment to Azure? Yes, CloudAMQP has direct deployment to Azure.
2. Does it allow multiple cusumers/subscribers? Yes
3. Does it support topics? Yes
4. Message durability (Messages don't get lost after restart)? Yes, by default.
5. Logging? All sort including metrics and exporting to well known systems like DataDog and Splunk
6. Can it bee run locally? It can be run as an exe, or in a Dockercontainer, and there is also free plan on Cloud
7. .Net support? RabbitMQ offers official .Net client which is actively updated, and also offers good client [documentation](https://rabbitmq.com/dotnet-api-guide.html)
8. High availability? 
   It can be deployed as a single instance or a cluster (2 or 3 nodes deployed in different availability zone in one region). 
   One ca protect the setup against a region-wide outage by setting up two clusters in different regions and use federation between them.
   Federation is a method in which a software system can benefit from having multiple RabbitMQ brokers distributed on different machines. 
   You can migrate from one cluster to another without the need to stopp all consumers/producers using queue federation.
9. Pricing? The cheapest cluster goes from 500 SEK
10. Sagas or similar? Nothing out of the box, except if you go for MassTransit + RMQ 
11. Supproted protocols? AMQP, STOMP, MQTT, HTTP, WEB-STOMP (cross between STOMP and Web Sockets so that web applications can consume RMQ)
12. Authentication? Via username/password or X.509 certificate or external using SAML plugin ([example for Azure AD auth](https://www.cloudamqp.com/docs/saml.html))
