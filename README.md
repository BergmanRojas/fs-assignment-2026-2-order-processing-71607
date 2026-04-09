
Distributed Order Processing Platform

This project is a distributed order processing platform built with .NET, RabbitMQ and multiple backend services.

The purpose of the project is to simulate how a real e-commerce system could process orders asynchronously. Instead of everything happening in one step, the order goes through different services, and each service is responsible for one part of the workflow.

The system currently includes these projects:
	•	OrderApi
	•	InventoryService
	•	PaymentService
	•	ShippingService
	•	Shared.Contracts
	•	Blazor Customer Portal
	•	React Admin Dashboard

Each backend service listens for an event, processes it, and then publishes the next event using RabbitMQ.

⸻

System Architecture

The solution is split into multiple projects so that each one has a clear responsibility.

OrderApi

OrderApi is the main API of the system and works as the entry point.

Its responsibilities are:
	•	receive the checkout request
	•	create and store orders
	•	publish the OrderSubmitted event
	•	expose REST API endpoints
	•	track the order progress
	•	receive updates from the backend services
	•	update the final order status

InventoryService

InventoryService is a background worker that listens for submitted orders.

Its responsibilities are:
	•	consume the OrderSubmitted event
	•	simulate stock checking
	•	publish the InventoryConfirmed event

PaymentService

PaymentService handles the payment step of the process.

Its responsibilities are:
	•	consume the InventoryConfirmed event
	•	simulate payment approval
	•	publish the PaymentApproved event

ShippingService

ShippingService is responsible for shipment creation.

Its responsibilities are:
	•	consume the PaymentApproved event
	•	generate a shipment reference
	•	publish the ShippingCreated event

Shared.Contracts

Shared.Contracts is the shared library that contains the event contracts used between services.

Currently implemented events:
	•	OrderSubmitted
	•	InventoryConfirmed
	•	PaymentApproved
	•	ShippingCreated

⸻

Event Flow

The system currently works like this:
	1.	The customer places an order using POST /api/orders/checkout
	2.	OrderApi creates the order with status Submitted
	3.	OrderApi publishes the OrderSubmitted event
	4.	InventoryService consumes the event and publishes InventoryConfirmed
	5.	PaymentService consumes that event and publishes PaymentApproved
	6.	ShippingService consumes that event and publishes ShippingCreated
	7.	OrderApi receives the final event and updates the order

This creates an asynchronous workflow where services are separated and communicate through RabbitMQ.

⸻

Order Status

At the moment, the project uses a simplified status flow.

Statuses currently used include:
	•	Submitted
	•	Inventory Confirmed
	•	Payment Approved
	•	Completed

This is enough to show the full order lifecycle and demonstrate the distributed architecture.

⸻

Technologies Used

The project was built using:
	•	.NET 9
	•	ASP.NET Core Minimal API
	•	Worker Services
	•	RabbitMQ
	•	Serilog
	•	SQLite
	•	Docker
	•	Blazor
	•	React
	•	GitHub

⸻

Logging

The project uses Serilog for logging.

Some of the main actions being logged are:
	•	service startup
	•	message publishing
	•	message consumption
	•	order creation
	•	order updates
	•	order completion
	•	errors and exceptions

Logs are shown in the console and also saved into log files, which helps a lot when testing and debugging the application.

⸻

Frontend Applications

This project includes two frontend applications.

Customer Portal – Blazor

The customer side was built in Blazor.

Current features:
	•	login page
	•	product listing
	•	add to cart
	•	shopping cart
	•	checkout
	•	my orders
	•	tracking page

Admin Dashboard – React

The admin side was built in React.

Current features:
	•	admin login
	•	orders dashboard
	•	filter by order status
	•	search by order ID
	•	order details view
	•	failed orders page

⸻

API Endpoints Implemented in OrderApi

Health Check
	•	GET /

Orders
	•	POST /api/orders/checkout
	•	POST /api/orders/test-publish
	•	GET /api/orders
	•	GET /api/orders/{id}
	•	GET /api/orders/{id}/status
	•	GET /api/customers/{id}/orders

Products
	•	GET /api/products

⸻

How to Run the Project

Requirements

Before running the project, the following are needed:
	•	.NET 9 SDK
	•	Docker Desktop
	•	Node.js and npm
	•	Rider, Visual Studio, or VS Code

⸻

Run with Docker Compose

The easiest way to start the backend is with Docker Compose.

From the root of the solution, run:

docker compose down
docker compose up --build -d

This starts:
	•	RabbitMQ
	•	OrderApi
	•	InventoryService
	•	PaymentService
	•	ShippingService

To check that the containers are running:

docker ps

RabbitMQ management panel:

http://localhost:15672

Default login:

guest
guest


⸻

Run the Frontends

Blazor Customer Portal

cd Frontend/ClientApp
dotnet run

Runs on:

http://localhost:5177

React Admin Dashboard

cd Frontend/AdminApp
npm install
npm run dev

Runs on:

http://localhost:5173


⸻

Quick Test

Check if the API is running

curl http://localhost:5258/

Get all orders

curl http://localhost:5258/api/orders

Get all products

curl http://localhost:5258/api/products

Create a new order

curl -X POST http://localhost:5258/api/orders/checkout

Check the status of an order

curl http://localhost:5258/api/orders/{ORDER_ID}/status


⸻

General Solution Structure

FullStackAssignment4-71607.sln
├── Backend
│   ├── OrderApi
│   ├── InventoryService
│   ├── PaymentService
│   ├── ShippingService
│   └── Shared.Contracts
├── Frontend
│   ├── ClientApp
│   └── AdminApp
├── docker-compose.yml
└── README.md


⸻

Responsibilities by Service

OrderApi
	•	main entry point
	•	API endpoints
	•	order creation
	•	order tracking
	•	order status updates

InventoryService
	•	stock validation simulation
	•	inventory event publishing

PaymentService
	•	payment simulation
	•	payment event publishing

ShippingService
	•	shipment creation
	•	shipment reference generation

Shared.Contracts
	•	shared event contracts used between all services

⸻

CQRS with MediatR

A basic CQRS structure using MediatR was introduced in the OrderApi project.

This was added to improve code organisation and separate commands from queries.

Examples currently included:
	•	CheckoutOrderCommand
	•	GetOrdersQuery
	•	GetProductsQuery

This means the API is starting to follow a cleaner structure, where some business logic is moved out of Program.cs and into separate handlers.

⸻

Assumptions

Some assumptions were made to keep the project manageable:
	•	the workflow is simplified
	•	RabbitMQ runs locally
	•	the services communicate in a local development environment
	•	SQLite is used as the database
	•	the API runs on http://localhost:5258
	•	the frontends run on their own local ports

⸻

Current Limitations

At the moment, the project still has some limitations:
	•	authentication is only simulated
	•	there is no real payment gateway
	•	inventory checking is simplified
	•	not all failure scenarios are fully implemented
	•	automated tests are not included yet
	•	CQRS is only partially implemented
	•	AutoMapper is not implemented yet
	•	the business workflow is simplified for demonstration purposes

⸻

Future Improvements

If I continue improving this project, the next steps would be:
	•	full CQRS implementation across the API
	•	AutoMapper for DTO and response mapping
	•	more failure states such as:
	•	InventoryFailed
	•	PaymentRejected
	•	ShippingFailed
	•	unit and integration tests
	•	GitHub Actions for build and test automation
	•	stronger authentication and authorisation
	•	more admin monitoring features
	•	more realistic customer and payment data
	•	better deployment setup

⸻

Author

Bergman Rojas

Project developed for the Full Stack Development module.

