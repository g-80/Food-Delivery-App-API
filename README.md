# Food Delivery App API

A food delivery app API/backend developed with ASP.NET Core.

This project is still in progress.

## Features

### Authentication & User Management
- User registration and login with role-based access control (customer, food_place, driver)
- Authentication using JWT
- JWT access tokens with automatic renewal using refresh tokens

### Food Place & Menu Management
- Create and manage food places (restaurants) with location data
- Menu item creation with pricing, descriptions, and availability
- Search for nearby food places using location-based queries (PostGIS)
- Text search for food places by name or category
- Distance-based filtering with configurable radius

### Order Management & Payments
- Shopping cart with add/remove/update quantity operations
- Order creation with comprehensive status tracking (pending → preparing → ready → delivering → completed)
- Real-time order confirmation and rejection workflow for food places via WebSocket
- Stripe integration for payment processing
- Automatic payment capture and refund handling
- Order cancellation with payment refunds

### Real-Time Delivery System
- Driver online/offline status management via SignalR WebSocket
- Automatic delivery assignment to drivers within configured radius of food place
- Real-time delivery offers with specified timeout and auto-retry
- Driver acceptance/rejection of delivery offers
- Live driver location tracking with accuracy, speed, and heading metadata
- Driver location stored in Redis for real-time performance
- Delivery route calculation using MapBox Directions API with turn-by-turn instructions
- Distance and duration-based driver payment calculation
- 4-digit delivery confirmation codes for secure handoff


## Tech Stack

### Backend Framework & Architecture
- .NET 10
- ASP.NET Core Web Api with SignalR for WebSocket communication
- Clean Architecture (Domain, Application, API, Infrastructure layers)
- CQRS pattern for command/query separation

### Databases & Caching
- PostgreSQL 17 with PostGIS extension for geospatial queries
- Redis 8 for driver location caching and session management
- Dapper micro-ORM for database operations
- dbup for database migrations

### External Services & APIs
- Stripe for payment processing
- MapBox Directions API for route calculation
- MapBox Geocoding API for address-to-coordinates conversion

### Infrastructure & DevOps
- Docker for containerisation
- Docker Compose for local development
- Terraform for Infrastructure as Code
- AWS services:
  - ECS for container orchestration
  - EC2 with Auto Scaling Groups
  - RDS PostgreSQL with multi-AZ support and automatic backups
  - ElastiCache Redis
  - Application Load Balancer with health checks
  - ECR with image scanning
  - CloudWatch for logging and monitoring
  - Secrets Manager for credential management

### Testing
- xUnit testing framework
- Moq for mocking
- FluentAssertions for readable test assertions