# rabbit
Rabbit :rabbit: is playground for trying out RabbitMQ

# Scenario

We have long running distributed process called: "Generation". Generation process has three individual steps: 
1. Generate preview image
2. Generate metadata
3. Calculate weight

Requester can either request one of the steps, or all of them, and if a requests sends a request for all threee steps, all three steps will be processed in parallel.
There is additional component called "Supervisor". Supervisor receives requests from Requesters and resolves their requirements in terms of which steps should be executed.
Every step has a dedicated component:
1. Preview Image Generator
2. Metadata Generator
3. Weight Calculator
All threee "step" components will upon finished processing send a message that will be picked up by Supervisor.
Supervisor will deterimine when requested Generation is completed and will send a message that will be picked up by the Requester that initiated everything.

All component will be subscribed to the same exchange: `generation`, but will listen to different topics.

# Use case: Request generation for all three steps

RMQ setup:
* Exchange: `generation`
* Topics: `generation.requests`, `generation.responses`, `generation.step-completed`, `generation.previews`, `generation.weights`, `generation.metadata`

Supervisor subscribes to: `generation.requests.#` and `generation.responses`
Preview Image Generator subscribes to: `generation.previews`
Metadata Generator subscribed to: `generation.metadata`
Weight Calculator subscribes to: `generation.weights`
Requester send a message to exchange: `generation` and topic: `generation.requests.request-id`
Requester subscribes to topic: `generation.responses.request-id`
Supervisor will catch the message sent to `generation.requests.request-id`
Supervises execute logic required to determine which steps should be executed and sends them to appropriate topics. (Message will contain `request-id` property)
Each of the steps components receives a message and processes them. Upon processing they send a message to `generation.step-completed`. (Message will contain the `request-id` property)
Supervisor is subscribed to `generation.responses` and it also keeps track or steps that need to be completed to have the generation job deemed finished. Tracking is done using `request-id` property, so in our case Supervisor is awaiting for three response messages (from each of the step components) that have specific `request-id`. Once all the steps are finished and Supervisor receives all three response messages it will send a message to: `generation.responses.request-id`.
Requester will receive a message about generation job completition which also concludes the processing flow.
