This project implements a distributed order processing platform using .NET, RabbitMQ and multiple backend services.

The system simulates an asynchronous order processing flow similar to a real-world e-commerce scenario. When a customer checks out, the order is sent to RabbitMQ and processed by various independent services:

- **OrderApi**
- **InventoryService**
- **PaymentService**
- **ShippingService**

Each service consumes an event, performs its task and publishes the next event in the flow.

---

## System architecture

The solution comprises the following projects:

### 1. OrderApi
The system’s main API.

Responsibilities:
- receive the order checkout
- create and store orders in memory
- publish the `OrderSubmitted` event
- expose REST endpoints to query orders
- listen for the final `ShippingCreated` event
- update the order status to `Completed`

### 2. InventoryService
A background service that consumes order-submitted events.

Responsibilities:
- consume `OrderSubmitted`
- simulate inventory validation
- publish `InventoryConfirmed`

### 3. PaymentService
A background service that processes the order payment.

Responsibilities:
- consume `InventoryConfirmed`
- simulate payment approval
- publish `PaymentApproved`

### 4. ShippingService
Background service that generates the shipment.

Responsibilities:
- consume `PaymentApproved`
- generate shipment reference
- publish `ShippingCreated`

### 5. Shared.Contracts
Shared library containing the event contracts used between services.

Implemented events:
- `OrderSubmitted`
- `InventoryConfirmed`
- `PaymentApproved`
- `ShippingCreated`

---

## Event flow

The current flow is as follows:

1. The customer checks out via `POST /api/orders/checkout`
2. `OrderApi` creates the order with status `Submitted`
3. `OrderApi` publishes the `OrderSubmitted` event
4. `InventoryService` consumes `OrderSubmitted` and publishes `InventoryConfirmed`
5. `PaymentService` consumes `InventoryConfirmed` and publishes `PaymentApproved`
6. `ShippingService` consumes `PaymentApproved` and publishes `ShippingCreated`
7. `OrderApi` consumes `ShippingCreated` and updates the order to `Completed` status

---

## Order statuses

Currently, the order uses a simplified status flow:

- `Submitted`
- `Completed`

The status is stored in memory within `OrderStore`.

---

## Technologies used

- .NET 9
- ASP.NET Core Minimal API
- Worker Services
- RabbitMQ
- Serilog
- Docker
- GitHub

---

## Logging

The project uses Serilog for structured logging.

Currently, important information is logged, such as:
    •    service start-up
    •    message reception
    •    event publication
    •    order creation
    •    order completion

The logs are displayed in the console and are also saved in log files organised by service.

## Endpoints implemented in OrderApi

Health / root

GET /

Create order / checkout

POST /api/orders/checkout

Create test order

POST /api/orders/test-publish

Get all orders

GET /api/orders

Get an order by ID

GET /api/orders/{id}

Get order status

GET /api/orders/{id}/status

---

## How to run the project

Prerequisites
    •    .NET 9 SDK
    •    Docker Desktop
    •    RabbitMQ running in Docker
    •    JetBrains Rider or Visual Studio Code / Visual Studio

Run RabbitMQ

Run the following command:

docker run -d –hostname rabbitmq-dev –name rabbitmq-dev -p 5672:5672 -p 15672:15672 rabbitmq:3-management

RabbitMQ administration panel:

http://localhost:15672

Default credentials:

guest
guest

## Run the solution

The following projects must be run simultaneously:
    •    OrderApi
    •    InventoryService
    •    PaymentService
    •    ShippingService

The workflow only functions fully if all are running alongside RabbitMQ.

Quick system test

Create an order:

curl -X POST http://localhost:5258/api/orders/checkout

List all orders:

curl http://localhost:5258/api/orders

Check the status of an order:

curl http://localhost:5258/api/orders/{ORDER_ID}/status

General structure of the solution

FullStackAssignment4-71607.sln
├── OrderApi
├── InventoryService
├── PaymentService
├── ShippingService
└── Shared.Contracts

___

## Responsibilities by service

**OrderApi**
    •    main entry point to the system
    •    REST endpoints
    •    order tracking
    •    updating final status

**InventoryService**
    •    simulated stock validation

**PaymentService**
    •    simulated payment approval

**ShippingService**
    •    simulated shipment creation

**Shared.Contracts**
    •    shared event classes

**Project assumptions**
    •    the system uses in-memory storage for orders
    •    a persistent database has not yet been implemented
    •    inventory is always confirmed
    •    payment is always approved
    •    shipments are always generated correctly
    •    services communicate via local RabbitMQ
	•    The current port for OrderApi is http://localhost:5258

**Current limitations**
    •    There is no real database
    •    There is no authentication or authorisation
    •    The Blazor frontend has not yet been implemented
	•    The dashboard has not yet been implemented in React or Angular
    •    Automated tests have not yet been added
    •    Order status is lost if the application is restarted
    •    The business model is simplified to demonstrate distributed architecture and asynchronous messaging

**Future improvements**
    •    Persistence using SQLite or SQL Server
    •    Full implementation of products, customers and order items
	•    Customer Portal with Blazor
    •    Admin Dashboard with React or Angular
    •    unit and integration tests
    •    Docker Compose for the entire solution
    •    GitHub Actions for build and test
    •    support for error states such as InventoryFailed, PaymentRejected 
    •    CQRS with MediatR
    •    AutoMapper for DTOs and responses

___

## Author

**Bergman Rojas**

Project developed for the Full Stack Development course.
