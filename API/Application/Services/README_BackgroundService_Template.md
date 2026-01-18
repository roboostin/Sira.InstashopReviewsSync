# Background Service Template Documentation

This document describes the structure and patterns used in `ProcessNonProcessedReviewsBackgroundService` to help you create similar background services.

## Table of Contents
- [Overview](#overview)
- [Service Structure](#service-structure)
- [Key Components](#key-components)
- [Implementation Guide](#implementation-guide)
- [Best Practices](#best-practices)
- [Registration](#registration)
- [Lifecycle Management](#lifecycle-management)

## Overview

A background service in .NET is a long-running service that executes continuously in the background. It inherits from `BackgroundService` and runs independently of HTTP requests.

### Use Cases
- Periodic data processing
- Scheduled tasks
- Queue processing
- Data synchronization
- Cleanup operations

## Service Structure

### Base Class
```csharp
public class YourBackgroundService : BackgroundService
```

### Core Components

#### 1. **Dependencies**
```csharp
private readonly IServiceScopeFactory _serviceScopeFactory;
private readonly ILogger<YourBackgroundService> _logger;
private readonly SemaphoreSlim _semaphore;
private readonly IUnitOfWork _unitOfWork; // Optional, if needed
```

**Purpose:**
- `IServiceScopeFactory`: Creates scoped services for each execution cycle (prevents scope leaks)
- `ILogger<T>`: Structured logging for the service
- `SemaphoreSlim`: Prevents concurrent execution of the same service
- `IUnitOfWork`: Optional, for database operations

#### 2. **Constructor**
```csharp
public YourBackgroundService(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<YourBackgroundService> logger,
    IUnitOfWork unitOfWork) // Optional
{
    _serviceScopeFactory = serviceScopeFactory;
    _logger = logger;
    _unitOfWork = unitOfWork;
    _semaphore = new SemaphoreSlim(1, 1); // Only 1 concurrent execution
}
```

#### 3. **Main Execution Loop** (`ExecuteAsync`)
```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    _logger.LogInformation("YourBackgroundService started");
    
    TimeSpan nextDelay = TimeSpan.FromMinutes(3); // Initial delay
    
    while (!stoppingToken.IsCancellationRequested)
    {
        try
        {
            // Use semaphore to prevent concurrent execution
            if (await _semaphore.WaitAsync(0, stoppingToken))
            {
                try
                {
                    nextDelay = await ProcessWorkAsync(stoppingToken);
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            else
            {
                _logger.LogWarning("Previous execution still running, skipping this cycle");
                nextDelay = TimeSpan.FromMinutes(3); // Default delay
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("YourBackgroundService is stopping");
            break;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in YourBackgroundService execution cycle");
            nextDelay = TimeSpan.FromMinutes(10); // Longer delay on error
        }
        
        // Wait before next iteration
        await Task.Delay(nextDelay, stoppingToken);
    }
    
    _logger.LogInformation("YourBackgroundService stopped");
}
```

**Key Points:**
- Runs in a continuous loop until cancellation
- Uses `SemaphoreSlim` to prevent overlapping executions
- Handles cancellation gracefully
- Implements dynamic delay based on workload/errors
- Always respects `stoppingToken` for graceful shutdown

#### 4. **Work Processing Method**
```csharp
private async Task<TimeSpan> ProcessWorkAsync(CancellationToken cancellationToken)
{
    // Create a new scope for each execution to ensure fresh dependencies
    using var scope = _serviceScopeFactory.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
    // ... other scoped services
    
    try
    {
        // 1. Query/Fetch data
        var queryResult = await mediator.Send(new YourQuery(), cancellationToken);
        
        if (!queryResult.IsSuccess)
        {
            _logger.LogError("Query failed: {Message}", queryResult.Message);
            return TimeSpan.FromMinutes(10); // Return delay on failure
        }
        
        var data = queryResult.Data;
        
        if (data == null || !data.Any())
        {
            _logger.LogInformation("No data to process");
            return TimeSpan.FromMinutes(10); // Return delay when no work
        }
        
        // 2. Process data
        var processedIds = new List<long>();
        
        foreach (var item in data)
        {
            try
            {
                // Process each item
                // ...
                processedIds.Add(item.ID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing item {ItemId}", item.ID);
            }
        }
        
        // 3. Publish events (if needed)
        eventPublisher.Publish();
        
        // 4. Update database (if needed)
        if (processedIds.Any())
        {
            var updateResult = await mediator.Send(
                new MarkAsProcessedCommand(processedIds), 
                cancellationToken);
            
            if (!updateResult.IsSuccess)
            {
                _logger.LogError("Failed to mark as processed: {Message}", updateResult.Message);
                return TimeSpan.FromMinutes(10);
            }
        }
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        // 5. Determine next delay based on workload
        if (data.Count == maxLimit)
        {
            return TimeSpan.FromSeconds(30); // More work available, check sooner
        }
        else
        {
            return TimeSpan.FromMinutes(10); // All processed, wait longer
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error while executing work");
        throw; // Re-throw to be caught by ExecuteAsync
    }
}
```

**Key Points:**
- **Always create a new scope** for each execution to prevent scope leaks
- **Return `TimeSpan`** to control next execution delay dynamically
- **Handle errors gracefully** and return appropriate delays
- **Use cancellation tokens** throughout async operations
- **Log important operations** for debugging and monitoring

#### 5. **Lifecycle Methods**

```csharp
public override async Task StopAsync(CancellationToken cancellationToken)
{
    _logger.LogInformation("YourBackgroundService is stopping");
    await base.StopAsync(cancellationToken);
}

public override void Dispose()
{
    _semaphore?.Dispose();
    base.Dispose();
}
```

## Key Components

### 1. **SemaphoreSlim - Concurrency Control**
```csharp
private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
```

**Purpose:** Ensures only one execution runs at a time, preventing:
- Race conditions
- Duplicate processing
- Resource conflicts

**Usage:**
```csharp
if (await _semaphore.WaitAsync(0, stoppingToken))
{
    try
    {
        // Your work here
    }
    finally
    {
        _semaphore.Release();
    }
}
```

### 2. **Service Scope Factory - Dependency Injection**
```csharp
using var scope = _serviceScopeFactory.CreateScope();
var service = scope.ServiceProvider.GetRequiredService<IService>();
```

**Purpose:** 
- Creates fresh service instances for each execution
- Prevents scope leaks (services live longer than intended)
- Ensures scoped services (like `IEventPublisher`) are properly disposed

**Important:** Always use `using` statement to ensure proper disposal.

### 3. **Dynamic Delay Strategy**
```csharp
// Return different delays based on workload
if (hasMoreWork)
    return TimeSpan.FromSeconds(30);  // Check soon
else
    return TimeSpan.FromMinutes(10);   // Wait longer
```

**Benefits:**
- Reduces unnecessary database queries when no work exists
- Processes work faster when backlog exists
- Adapts to system load automatically

### 4. **Cancellation Token Support**
Always pass `CancellationToken` to async operations:
```csharp
await mediator.Send(query, cancellationToken);
await Task.Delay(delay, cancellationToken);
await _unitOfWork.SaveChangesAsync(cancellationToken);
```

**Purpose:** Allows graceful shutdown when application stops.

## Implementation Guide

### Step 1: Create the Service Class
```csharp
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace API.Application.Services;

public class YourBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<YourBackgroundService> _logger;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    
    public YourBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<YourBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }
    
    // ... implement methods
}
```

### Step 2: Implement ExecuteAsync
- Create the main loop
- Add semaphore protection
- Handle cancellation
- Implement error handling
- Add dynamic delay logic

### Step 3: Implement Work Processing Method
- Create service scope
- Fetch data
- Process items
- Update database
- Return next delay

### Step 4: Register in Program.cs
```csharp
builder.Services.AddHostedService<API.Application.Services.YourBackgroundService>();
```

### Step 5: Add Lifecycle Methods
- Implement `StopAsync` for graceful shutdown
- Implement `Dispose` to clean up resources

## Best Practices

### ✅ DO:
1. **Always use service scopes** for scoped dependencies
2. **Respect cancellation tokens** in all async operations
3. **Log important events** (start, stop, errors, processing counts)
4. **Use semaphore** to prevent concurrent execution
5. **Return dynamic delays** based on workload
6. **Handle errors gracefully** without crashing the service
7. **Dispose resources** properly (semaphore, scopes)
8. **Use structured logging** with context information

### ❌ DON'T:
1. **Don't use singleton-scoped services directly** - create scopes instead
2. **Don't ignore cancellation tokens** - always pass them through
3. **Don't use fixed delays** - adapt to workload
4. **Don't swallow exceptions** - log them appropriately
5. **Don't process without limits** - implement batch processing
6. **Don't forget to release semaphore** - use try/finally
7. **Don't block the thread** - use async/await throughout

## Registration

### In Program.cs
```csharp
// Register background service
builder.Services.AddHostedService<API.Application.Services.YourBackgroundService>();
```

**Location:** Register after other service configurations, before `app.Build()`.

**Note:** `AddHostedService` automatically registers the service as a singleton and starts it when the application starts.

## Lifecycle Management

### Startup
1. Service is instantiated when application starts
2. `ExecuteAsync` is called automatically
3. Service runs in the background

### Execution Cycle
1. Check if previous execution completed (semaphore)
2. Create new service scope
3. Process work
4. Calculate next delay
5. Wait for delay period
6. Repeat

### Shutdown
1. Application receives shutdown signal
2. `stoppingToken` is cancelled
3. `StopAsync` is called
4. Current execution completes (if possible)
5. Service stops gracefully

## Example: Complete Service Template

```csharp
using API.Shared.Models;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace API.Application.Services;

public class YourBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<YourBackgroundService> _logger;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public YourBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<YourBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("YourBackgroundService started");

        TimeSpan nextDelay = TimeSpan.FromMinutes(3);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (await _semaphore.WaitAsync(0, stoppingToken))
                {
                    try
                    {
                        nextDelay = await ProcessWorkAsync(stoppingToken);
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                }
                else
                {
                    _logger.LogWarning("Previous execution still running, skipping this cycle");
                    nextDelay = TimeSpan.FromMinutes(3);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("YourBackgroundService is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in YourBackgroundService execution cycle");
                nextDelay = TimeSpan.FromMinutes(10);
            }

            await Task.Delay(nextDelay, stoppingToken);
        }

        _logger.LogInformation("YourBackgroundService stopped");
    }

    private async Task<TimeSpan> ProcessWorkAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        try
        {
            // Your processing logic here
            _logger.LogInformation("Processing work...");
            
            // Example: Query data
            // var result = await mediator.Send(new YourQuery(), cancellationToken);
            
            // Example: Process items
            // foreach (var item in items) { ... }
            
            // Determine next delay
            return TimeSpan.FromMinutes(10);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while executing work");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("YourBackgroundService is stopping");
        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _semaphore?.Dispose();
        base.Dispose();
    }
}
```

## Troubleshooting

### Service Not Starting
- Check if it's registered in `Program.cs`
- Verify no exceptions in constructor
- Check application logs for startup errors

### Service Running Too Frequently
- Check delay logic in `ProcessWorkAsync`
- Verify semaphore is working correctly
- Review error handling (errors might reset delay)

### Memory Leaks
- Ensure service scopes are disposed (`using` statement)
- Check for event handlers not being unsubscribed
- Verify semaphore is disposed

### Duplicate Processing
- Verify semaphore is working
- Check if multiple instances are registered
- Ensure proper error handling doesn't skip marking as processed

## Additional Resources

- [.NET Background Services Documentation](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services)
- [Cancellation Tokens](https://learn.microsoft.com/en-us/dotnet/standard/threading/cancellation-in-managed-threads)
- [Dependency Injection Scopes](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#service-lifetimes)

