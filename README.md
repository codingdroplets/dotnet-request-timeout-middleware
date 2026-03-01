# ASP.NET Core Request Timeout Middleware Demo (.NET 10)

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Build](https://img.shields.io/badge/build-passing-brightgreen)](#)

Beginner-friendly ASP.NET Core Minimal API sample that demonstrates request timeout middleware, endpoint-specific timeout policies, and cancellation-token-aware handlers with Swagger/OpenAPI.

## Key Features
- Global and per-endpoint request timeout policies
- CancellationToken-aware endpoint handlers
- Timeout demo endpoint (`/demo/slow`) returning `504 Gateway Timeout`
- Long-policy endpoint (`/demo/slow-allowed`) that succeeds
- Swagger/OpenAPI for interactive testing

## Architecture Overview
- Minimal API structure for easy learning
- `AddRequestTimeouts` for global + named policies
- `UseRequestTimeouts` middleware for runtime enforcement
- Endpoint-level overrides via `WithRequestTimeout("policyName")`

## Tech Stack
- .NET 10
- ASP.NET Core Minimal API
- OpenAPI/Swagger
- xUnit integration tests

## Getting Started
```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project DotNetRequestTimeoutMiddleware
```

Swagger opens automatically at `/swagger` in local launch profiles.

## Use Cases
- Prevent APIs from hanging on long-running requests
- Apply SLA-specific timeout limits per endpoint
- Teach cancellation-aware API design to beginners

## Maintenance Signals
- MIT `LICENSE` included
- `CHANGELOG.md` included
- Initial release tag: `v1.0.0`

## Author / Maintainer
- Visit Now: https://codingdroplets.com
- Join our Patreon to Learn & Level Up: https://www.patreon.com/codingdroplets
