# Request Timeout Middleware in ASP.NET Core (.NET 10)

> **Never let a slow endpoint hang your API** — Global and per-endpoint timeout policies with `CancellationToken`-aware handlers and clean `504 Gateway Timeout` responses.

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-10.0-512BD4)](https://docs.microsoft.com/aspnet/core)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Visit CodingDroplets](https://img.shields.io/badge/Website-codingdroplets.com-blue?style=flat&logo=google-chrome&logoColor=white)](https://codingdroplets.com/)
[![YouTube](https://img.shields.io/badge/YouTube-CodingDroplets-red?style=flat&logo=youtube&logoColor=white)](https://www.youtube.com/@CodingDroplets)
[![Patreon](https://img.shields.io/badge/Patreon-Support%20Us-orange?style=flat&logo=patreon&logoColor=white)](https://www.patreon.com/CodingDroplets)
[![Buy Me a Coffee](https://img.shields.io/badge/Buy%20Me%20a%20Coffee-Support%20Us-yellow?style=flat&logo=buy-me-a-coffee&logoColor=black)](https://buymeacoffee.com/codingdroplets)
[![GitHub](https://img.shields.io/badge/GitHub-codingdroplets-black?style=flat&logo=github&logoColor=white)](http://github.com/codingdroplets/)

---

## 🚀 Support the Channel — Join on Patreon

If this sample saved you time, consider joining our Patreon community.
You'll get **exclusive .NET tutorials, premium code samples, and early access** to new content — all for the price of a coffee.

👉 **[Join CodingDroplets on Patreon](https://www.patreon.com/CodingDroplets)**

Prefer a one-time tip? [Buy us a coffee ☕](https://buymeacoffee.com/codingdroplets)

---

## 🎯 What You'll Learn

- How to configure **global request timeout policies** with `AddRequestTimeouts`
- How to apply **per-endpoint timeout overrides** with `WithRequestTimeout("policyName")`
- How to write **`CancellationToken`-aware endpoint handlers** that cancel cleanly
- How to return `504 Gateway Timeout` when a request exceeds its time limit
- How to write **integration tests** that verify timeout behaviour end-to-end

---

## 🗺️ Architecture Overview

```
Incoming HTTP Request
        │
        ▼
┌──────────────────────────────────────────────────────┐
│          ASP.NET Core Middleware Pipeline             │
│  ┌────────────────────────────────────────────────┐  │
│  │      app.UseRequestTimeouts()  ← EARLY         │  │
│  │  Starts a cancellation timer per policy        │  │
│  └──────────────────┬─────────────────────────────┘  │
│                     │                                │
│  ┌──────────────────▼─────────────────────────────┐  │
│  │         Minimal API Endpoint                   │  │
│  │  Receives CancellationToken from middleware     │  │
│  │                                                │  │
│  │  Within time limit?                            │  │
│  │  YES → 200 OK                                  │  │
│  │  NO  → CancellationToken fires → 504           │  │
│  └────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────┘
```

---

## 📋 Timeout Policies

| Policy | Timeout | Endpoint | Expected Result |
|--------|---------|----------|-----------------|
| `default` (global) | 3 seconds | `/demo/slow` (5s delay) | `504 Gateway Timeout` |
| `LongRunning` | 10 seconds | `/demo/slow-allowed` (5s delay) | `200 OK` |
| _(none)_ | — | `/demo/fast` | `200 OK` immediately |

---

## 📁 Project Structure

```
dotnet-request-timeout-middleware/
├── dotnet-request-timeout-middleware.sln
└── DotNetRequestTimeoutMiddleware/
    ├── Program.cs                  # Timeout policies + endpoint registration
    ├── Properties/
    │   └── launchSettings.json    # Swagger auto-launch configured
    └── DotNetRequestTimeoutMiddleware.csproj
```

---

## 🛠️ Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Any IDE: Visual Studio 2022+, VS Code, or JetBrains Rider

---

## ⚡ Quick Start

```bash
# Clone the repo
git clone https://github.com/codingdroplets/dotnet-request-timeout-middleware.git
cd dotnet-request-timeout-middleware

# Build and run
dotnet restore
dotnet run --project DotNetRequestTimeoutMiddleware

# Open Swagger UI → http://localhost:{port}/swagger
```

---

## 🔧 How It Works

### Step 1 — Register Timeout Policies

```csharp
builder.Services.AddRequestTimeouts(options =>
{
    // Global default: 3 seconds
    options.DefaultPolicy = new RequestTimeoutPolicy
    {
        Timeout = TimeSpan.FromSeconds(3),
        TimeoutStatusCode = 504
    };

    // Named policy for slow endpoints: 10 seconds
    options.AddPolicy("LongRunning", new RequestTimeoutPolicy
    {
        Timeout = TimeSpan.FromSeconds(10),
        TimeoutStatusCode = 504
    });
});
```

### Step 2 — Wire the Middleware

```csharp
// Must be placed early in the pipeline
app.UseRequestTimeouts();
```

### Step 3 — Apply Per-Endpoint Overrides

```csharp
// Uses global default (3s) — will timeout at 5s delay
app.MapGet("/demo/slow", async (CancellationToken ct) =>
{
    await Task.Delay(TimeSpan.FromSeconds(5), ct);
    return Results.Ok("Done");
});

// Override with LongRunning (10s) — succeeds at 5s delay
app.MapGet("/demo/slow-allowed", async (CancellationToken ct) =>
{
    await Task.Delay(TimeSpan.FromSeconds(5), ct);
    return Results.Ok("Done (long policy)");
}).WithRequestTimeout("LongRunning");
```

---

## 🧪 Running Tests

```bash
dotnet restore
dotnet build
dotnet test
```

Integration tests cover:
- `/demo/fast` → `200 OK`
- `/demo/slow` → `504 Gateway Timeout`
- `/demo/slow-allowed` → `200 OK` (long policy)

---

## 🤔 Key Concepts

### Why Use Request Timeout Middleware?

| Without Timeouts | With Timeouts |
|-----------------|---------------|
| Slow endpoints hold threads indefinitely | Threads released after deadline |
| Thread pool exhaustion under load | Predictable resource usage |
| Clients wait forever or reset | Clients get a clear `504` |
| Hard to test timeout behaviour | Named policies are testable |

### CancellationToken-Aware Design

The middleware cancels the token when the deadline is reached. Your handlers **must** pass this token to all async operations for clean cancellation:

```csharp
// ✅ Correct — respects cancellation
await Task.Delay(delay, cancellationToken);
await dbContext.Products.ToListAsync(cancellationToken);

// ❌ Wrong — ignores cancellation, thread stays blocked
await Task.Delay(delay);
```

---

## 📚 References

- [Request timeouts middleware in ASP.NET Core — Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/performance/timeouts)
- [CancellationToken — .NET API](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken)

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).

---

## 🔗 Connect with CodingDroplets

| Platform | Link |
|----------|------|
| 🌐 Website | https://codingdroplets.com/ |
| 📺 YouTube | https://www.youtube.com/@CodingDroplets |
| 🎁 Patreon | https://www.patreon.com/CodingDroplets |
| ☕ Buy Me a Coffee | https://buymeacoffee.com/codingdroplets |
| 💻 GitHub | http://github.com/codingdroplets/ |

> **Want more samples like this?** [Support us on Patreon](https://www.patreon.com/CodingDroplets) or [buy us a coffee ☕](https://buymeacoffee.com/codingdroplets) — every bit helps keep the content coming!
