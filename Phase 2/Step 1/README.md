<img align="left" width="116" height="116" src="../pezza-logo.png" />

# &nbsp;**Pezza - Phase 2 - Step 1**

<br/><br/>

This Phase might feel a bit tedious, but it puts down a strong foundation to build off from for the entire incubator.

If at any point you are struggling you can refrence Phase 2\src\02. EndSolution

## **Install Mediatr**

To help us with CQRS we will be using the Mediatr Nuget package. 

What is Mediatr?
In-process messaging with no dependencies.

Supports request/response, commands, queries, notifications and events, synchronous and async with intelligent dispatching via C# generic variance.

Install Mediatr on the Core Project and your Common Project

![](Assets/2020-11-20-10-57-45.png)

Install MediatR.Extensions.Microsoft.DependencyInjection on the Core Project.

## **Create the other database entities and update database context**

- [ ] To speed up entity generation you can use a CLI tool or create it manually
  - [ ] Open Command Line
  - [ ] Create a new folder where entities and mapping be generated in
  - [ ] ```dotnet tool install --global EntityFrameworkCore.Generator```
  - [ ] ```efg generate -c "DB Connection String"```
  - [ ] Fix the generated namespaces and code cleanup
  - [ ] or can copy it from Phase2\Data

### **Create Base Address**

This is for any DTO or Entity that has an address.

![](Assets/2020-11-20-08-30-10.png)

```cs
namespace Pezza.Common.Entities
{
    public class AddressBase
    {
        public string Address { get; set; }

        public string City { get; set; }

        public string Province { get; set; }

        public string ZipCode { get; set; }
    }
}
```

### **Create Image Data Base**

This is for any DTO or Entity that has an Image that needs to be created.

![](Assets/2020-11-20-09-09-20.png)

```cs
namespace Pezza.Common.Entities
{
    public class ImageDataBase
    {
        public string ImageData { get; set; }
    }
}
```

### **Entitites**

Representing Database Tables Entities

![](Assets/2020-09-16-08-24-37.png)

### **DTO**

Create a Data Transfer Object with only the information the consumer of the data will need. This allows you to hide any sensitive data.

![](Assets/2020-09-16-08-24-51.png)
### **Unit Tests Test Data**

![](Assets/2020-10-04-19-37-53.png)


### **Database EFCore Maps**

![DBCOntext Map](Assets/2021-01-14-07-45-18.png)

### **Base Entity**

All of our Database Tables has a Primary Key of Id and type of Int.

![](Assets/![Database%20Context%20Interface%20Setup](../Assets/phase1-setup-db-context-interface.png).png)

```cs
namespace Pezza.Common.Entities
{
    public interface IEntity
    {
        int Id { get; set; }
    }
}

namespace Pezza.Common.Entities
{
    public abstract class Entity : IEntity
    {
        public int Id { get; set; }
    }
}
```

Add Entity Inheritance to all entities and DTO's

![](Assets/2020-10-04-20-31-00.png)

Remove

```cs
public int Id { get; set; }
```

### **Base DataAccess**

```cs
namespace Pezza.DataAccess.Contracts
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IDataAccess<T>
    {
        Task<T> GetAsync(int id);

        Task<List<T>> GetAllAsync();

        Task<T> UpdateAsync(T entity);

        Task<T> SaveAsync(T entity);

        Task<bool> DeleteAsync(int id);
    }
}
```

Remove IStockDataAcess.cs

!DataAccess Contracts Structure[](Assets/2020-10-04-20-36-09.png)

Convert StockDataAccess to inherit from IDataAccess.cs

```cs
namespace Pezza.DataAccess.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Threading.Tasks;
    using AutoMapper;
    using Microsoft.EntityFrameworkCore;
    using Pezza.Common.DTO;
    using Pezza.Common.Entities;
    using Pezza.DataAccess.Contracts;

    public class StockDataAccess : IDataAccess<StockDTO>
    {
        private readonly IDatabaseContext databaseContext;

        private readonly IMapper mapper;

        public StockDataAccess(IDatabaseContext databaseContext, IMapper mapper)
            => (this.databaseContext, this.mapper) = (databaseContext, mapper);

        public async Task<StockDTO> GetAsync(int id)
            => this.mapper.Map<StockDTO>(await this.databaseContext.Stocks.FirstOrDefaultAsync(x => x.Id == id));

        public async Task<List<StockDTO>> GetAllAsync()
        {
            var entities = await this.databaseContext.Stocks.Select(x => x).AsNoTracking().ToListAsync();
            return this.mapper.Map<List<StockDTO>>(entities);
        }

        public async Task<StockDTO> SaveAsync(StockDTO entity)
        {
            this.databaseContext.Stocks.Add(this.mapper.Map<Stock>(entity));
            await this.databaseContext.SaveChangesAsync();

            return entity;
        }

        public async Task<StockDTO> UpdateAsync(StockDTO entity)
        {
            var findEntity = await this.databaseContext.Stocks.FirstOrDefaultAsync(x => x.Id == entity.Id);

            findEntity.Name = !string.IsNullOrEmpty(entity.Name) ? entity.Name : findEntity.Name;
            findEntity.UnitOfMeasure = !string.IsNullOrEmpty(entity.UnitOfMeasure) ? entity.UnitOfMeasure : findEntity.UnitOfMeasure;
            findEntity.ValueOfMeasure = entity.ValueOfMeasure ?? findEntity.ValueOfMeasure;
            findEntity.Quantity = entity.Quantity ?? findEntity.Quantity;
            findEntity.ExpiryDate = entity.ExpiryDate ?? findEntity.ExpiryDate;
            findEntity.Comment = entity.Comment;
            this.databaseContext.Stocks.Update(this.mapper.Map<Stock>(entity));
            await this.databaseContext.SaveChangesAsync();

            return this.mapper.Map<StockDTO>(findEntity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await this.databaseContext.Stocks.FirstOrDefaultAsync(x => x.Id == id);
            this.databaseContext.Stocks.Remove(entity);
            var result = await this.databaseContext.SaveChangesAsync();

            return (result == 1);
        }
    }
}
```

Create DataAccess for all the Entities

![DataAccess Structure](Assets/2020-10-04-20-46-27.png)
### **Business Logic - Core**

We will be moving to CQRS pattern for the Core Layer. This helps Single Responsibility.

[CQRS Overview](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs)

To help us out achieving this we will be using a Nuget Package - Mediatr

[Mediatr](https://github.com/jbogard/MediatR)

To create consistency with the result we send back from the Core layer we will utilize a Result.cs class. This helps to create unity between all Commands and Queries.

![DataAccess Structure](Assets/2021-01-14-07-43-18.png)

## **Common Models**

Copy MimeTypes.cs from Phase 2\Data\Common\Models

![MimeTypes.cs](Assets/2021-01-14-08-05-17.png)

```cs
namespace Pezza.Common.Models
{
    using System.Collections.Generic;
    using System.Linq;

    public class Result
    {
        internal Result(bool succeeded, string error)
        {
            this.Succeeded = succeeded;

            this.Errors = new List<string>
            {
                error
            };
        }

        internal Result(bool succeeded, List<string> errors)
        {
            this.Succeeded = succeeded;
            this.Errors = errors;
        }

        public bool Succeeded { get; set; }

        public List<string> Errors { get; set; }

        public static Result Success() => new Result(true, new List<string> { });

        public static Result Failure(List<string> errors) => new Result(false, errors);

        public static Result Failure(string error) => new Result(false, error);
    }

    public class Result<T>
    {
        internal Result(bool succeeded, string error)
        {
            this.Succeeded = succeeded;
            this.Errors = new List<string>
            {
                error
            };
        }

        internal Result(bool succeeded, List<string> errors)
        {
            this.Succeeded = succeeded;
            this.Errors = errors;
        }

        internal Result(bool succeeded, T data, List<string> errors)
        {
            this.Succeeded = succeeded;
            this.Errors = errors;
            this.Data = data;
        }

        public bool Succeeded { get; set; }

        public T Data { get; set; }

        public List<string> Errors { get; set; }

        public static Result<T> Success(T data) => new Result<T>(true, data, new List<string> { });

        public static Result<T> Failure(string error) => new Result<T>(false, error);

        public static Result<T> Failure(List<string> errors) => new Result<T>(false, errors);
    }

    public class ListResult<T>
    {
        internal ListResult(bool succeeded, string error)
        {
            this.Succeeded = succeeded;
            this.Errors = new List<string>
            {
                error
            };
        }

        internal ListResult(bool succeeded, List<string> errors)
        {
            this.Succeeded = succeeded;
            this.Errors = errors;
        }

        internal ListResult(bool succeeded, List<T> data, List<string> errors)
        {
            this.Succeeded = succeeded;
            this.Errors = errors;
            this.Data = data;
        }

        internal ListResult(bool succeeded, IEnumerable<T> data, List<string> errors)
        {
            this.Succeeded = succeeded;
            this.Errors = errors;
            this.Data = data.ToList();
        }

        public bool Succeeded { get; set; }

        public List<T> Data { get; set; }

        public List<string> Errors { get; set; }

        public static ListResult<T> Success(List<T> data) => new ListResult<T>(true, data, new List<string> { });
        
        public static ListResult<T> Success(IEnumerable<T> data) => new ListResult<T>(true, data, new List<string> { });

        public static ListResult<T> Failure(string error) => new ListResult<T>(false, error);

        public static ListResult<T> Failure(List<string> errors) => new ListResult<T>(false, errors);
    }

}

```

Move Address Data into Addressbase into Pezza.Common\Entities AddressBase.cs

```cs
namespace Pezza.Common.Entities
{
    public class AddressBase
    {
        public string Address { get; set; }

        public string City { get; set; }

        public string Province { get; set; }

        public string ZipCode { get; set; }
    }
}
```

Move Address Data into ImageDataBase into Pezza.Common\Entities ImageDataBase.cs

```cs
namespace Pezza.Common.Entities
{
    public class ImageDataBase : Entity
    {
        public string ImageData { get; set; }
    }
}
```

## **DTO's**

Create DTO's that we will use in the calling projects for SOLID principal. Only send in data that is needed. Copy from Phase 2\Data\Common\DTO

![DTO's](Assets/2021-01-17-09-04-19.png)

You will see in any Data DTO, nullable boolean needs an extra property. Otherwise project like MVC doesn't know how to render it correctly.

## **Mapping**

Make sure Automapper is installed

![Automapper](2021-01-17-15-09-07.png)

[AutoMapper](https://docs.automapper.org/en/stable/Getting-started.html)

In Pezza.Common create a Profile folder, inside a new class MappingProfile.cs. We will use to Map Entities to DTO's and back.

```cs
namespace Pezza.Common.Profiles
{
    using AutoMapper;
    using Pezza.Common.DTO;
    using Pezza.Common.Entities;

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            this.CreateMap<Customer, CustomerDTO>()
                .ForMember(x => x.Address, x => x.MapFrom((src) => new AddressBase() { 
                    Address = src.Address,
                    City = src.City,
                    Province = src.Province,
                    ZipCode = src.ZipCode
                }))
                .ReverseMap();
            this.CreateMap<CustomerDTO, Customer>()
                .ForMember(vm => vm.Address, m => m.MapFrom(u => u.Address.Address))
                .ForMember(vm => vm.City, m => m.MapFrom(u => u.Address.City))
                .ForMember(vm => vm.Province, m => m.MapFrom(u => u.Address.Province))
                .ForMember(vm => vm.ZipCode, m => m.MapFrom(u => u.Address.ZipCode));

            this.CreateMap<Notify, NotifyDTO>();
            this.CreateMap<NotifyDTO, Notify>();

            this.CreateMap<Order, OrderDTO>();
            this.CreateMap<OrderDTO, Order>();

            this.CreateMap<OrderItem, OrderItemDTO>();
            this.CreateMap<OrderItemDTO, OrderItem>();

            this.CreateMap<Product, ProductDTO>();
            this.CreateMap<ProductDTO, Product>();

            this.CreateMap<Restaurant, RestaurantDTO>()
                .ForMember(x => x.Address, x => x.MapFrom((src) => new AddressBase()
                {
                    Address = src.Address,
                    City = src.City,
                    Province = src.Province,
                    ZipCode = src.PostalCode
                }))
                .ReverseMap();
            this.CreateMap<RestaurantDTO, Restaurant>()
                .ForMember(vm => vm.Address, m => m.MapFrom(u => u.Address.Address))
                .ForMember(vm => vm.City, m => m.MapFrom(u => u.Address.City))
                .ForMember(vm => vm.Province, m => m.MapFrom(u => u.Address.Province))
                .ForMember(vm => vm.PostalCode, m => m.MapFrom(u => u.Address.ZipCode));

            this.CreateMap<Stock, StockDTO>();
            this.CreateMap<StockDTO, Stock>();
        }
    }
}
```

Modify Pezza.Api Startup.cs Configure Method. To be able to view images you will need to enable StaticFiles.

```cs
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"Media")),
    RequestPath = new PathString("/Media"),
});
```

![Core Contract Structure](Assets/2020-10-04-22-21-37.png)

```cs
namespace Pezza.Core.Contracts
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ICore<T>
    {
        Task<T> GetAsync(int id);

        Task<IEnumerable<T>> GetAllAsync();

        Task<T> UpdateAsync(T model);

        Task<T> SaveAsync(T model);

        Task<bool> DeleteAsync(int id);
    }
}
```

Create the following Commands for each Entity in Pezza.Core

- Create Command

```cs
namespace Pezza.Core.Customer.Commands
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Pezza.Common.DTO;
    using Pezza.Common.Models;
    using Pezza.DataAccess.Contracts;

    public partial class CreateCustomerCommand : IRequest<Result<CustomerDTO>>
    {
        public CustomerDTO Data { get; set; }
    }

    public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Result<CustomerDTO>>
    {
        private readonly IDataAccess<CustomerDTO> dataAcess;

        public CreateCustomerCommandHandler(IDataAccess<CustomerDTO> dataAcess)
            => this.dataAcess = dataAcess;

        public async Task<Result<CustomerDTO>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            var outcome = await this.dataAcess.SaveAsync(request.Data);
            return (outcome != null) ? Result<CustomerDTO>.Success(outcome) : Result<CustomerDTO>.Failure("Error creating a Customer");
        }
    }
}
```

- Delete Command

```cs
namespace Pezza.Core.Customer.Commands
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Pezza.Common.DTO;
    using Pezza.Common.Models;
    using Pezza.DataAccess.Contracts;

    public partial class DeleteCustomerCommand : IRequest<Result>
    {
        public int Id { get; set; }
    }

    public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, Result>
    {
        private readonly IDataAccess<CustomerDTO> dataAcess;

        public DeleteCustomerCommandHandler(IDataAccess<CustomerDTO> dataAcess)
            => this.dataAcess = dataAcess;

        public async Task<Result> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
        {
            var outcome = await this.dataAcess.DeleteAsync(request.Id);

            return (outcome) ? Result.Success() : Result.Failure("Error deleting a Customer");
        }
    }
}
```

- Update Command

```cs
namespace Pezza.Core.Customer.Commands
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Pezza.Common.DTO;
    using Pezza.Common.Models;
    using Pezza.DataAccess.Contracts;

    public partial class UpdateCustomerCommand : IRequest<Result<CustomerDTO>>
    {
        public CustomerDTO Data { get; set; }
    }

    public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Result<CustomerDTO>>
    {
        private readonly IDataAccess<CustomerDTO> dataAcess;

        public UpdateCustomerCommandHandler(IDataAccess<CustomerDTO> dataAcess) => this.dataAcess = dataAcess ?? throw new System.ArgumentNullException(nameof(dataAcess));

        public async Task<Result<CustomerDTO>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
        {            
            var outcome = await this.dataAcess.UpdateAsync(request.Data);
            return (outcome != null) ? Result<CustomerDTO>.Success(outcome) : Result<CustomerDTO>.Failure("Error updating a Customer");
        }
    }
}
```

If a property is not required and can be empty, don't enclose it in a shorthand if or coalescing.

```cs
findEntity.Description = request.Data?.Description;
```

Create the following Queries

-Get Single

```cs
namespace Pezza.Core.Customer.Queries
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Pezza.Common.DTO;
    using Pezza.Common.Models;
    using Pezza.DataAccess.Contracts;

    public class GetCustomerQuery : IRequest<Result<CustomerDTO>>
    {
        public int Id { get; set; }
    }

    public class GetCustomerQueryHandler : IRequestHandler<GetCustomerQuery, Result<CustomerDTO>>
    {
        private readonly IDataAccess<CustomerDTO> dataAcess;

        public GetCustomerQueryHandler(IDataAccess<CustomerDTO> dataAcess) => this.dataAcess = dataAcess;

        public async Task<Result<CustomerDTO>> Handle(GetCustomerQuery request, CancellationToken cancellationToken)
        {
            var search = await this.dataAcess.GetAsync(request.Id);
            return Result<CustomerDTO>.Success(search);
        }
    }
}
```

- Get All

```cs
namespace Pezza.Core.Customer.Queries
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Pezza.Common.DTO;
    using Pezza.Common.Models;
    using Pezza.DataAccess.Contracts;

    public class GetCustomersQuery : IRequest<ListResult<CustomerDTO>>
    {
    }

    public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, ListResult<CustomerDTO>>
    {
        private readonly IDataAccess<CustomerDTO> dataAcess;

        public GetCustomersQueryHandler(IDataAccess<CustomerDTO> dataAcess) => this.dataAcess = dataAcess;

        public async Task<ListResult<CustomerDTO>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
        {
            var search = await this.dataAcess.GetAllAsync();

            return ListResult<CustomerDTO>.Success(search);
        }
    }
}
```

Core Project should look this when you are done.

![Core Project Structure](Assets/2020-10-04-23-57-57.png)

Update DependencyInjection.cs - to include the new DataAccess and CQRS Classes

For MediatR Dependency Injection we need to create 3 Behaviour Classes inside Common

- PerformanceBehaviour.cs

```cs
namespace Pezza.Common.Behaviours
{
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Pezza.Common.Interfaces;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class PerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly Stopwatch timer;
        private readonly ILogger<TRequest> logger;
        private readonly ICurrentUserService currentUserService;
        private readonly IIdentityService identityService;

        public PerformanceBehaviour(
            ILogger<TRequest> logger,
            ICurrentUserService currentUserService,
            IIdentityService identityService)
        {
            this.timer = new Stopwatch();

            this.logger = logger;
            this.currentUserService = currentUserService;
            this.identityService = identityService;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            this.timer.Start();

            var response = await next();

            this.timer.Stop();

            var elapsedMilliseconds = this.timer.ElapsedMilliseconds;

            if (elapsedMilliseconds > 500)
            {
                var requestName = typeof(TRequest).Name;
                var userId = this.currentUserService.UserId ?? string.Empty;
                var userName = string.Empty;

                if (!string.IsNullOrEmpty(userId))
                {
                    userName = await this.identityService.GetUserNameAsync(userId);
                }

                this.logger.LogWarning("Pezza Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@UserId} {@UserName} {@Request}",
                    requestName, elapsedMilliseconds, userId, userName, request);
            }

            return response;
        }
    }
}
```

- UnhandledExceptionBehaviour.cs

```cs
namespace Pezza.Common.Behaviours
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class UnhandledExceptionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILogger<TRequest> logger;

        public UnhandledExceptionBehaviour(ILogger<TRequest> logger) => this.logger = logger;

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            try
            {
                return await next();
            }
            catch (Exception ex)
            {
                var requestName = typeof(TRequest).Name;

                this.logger.LogError(ex, "Pezza Request: Unhandled Exception for Request {Name} {@Request}", requestName, request);

                throw;
            }
        }
    }
}
```

- ValidationBehavior.cs -Will be used in Phase 3

```cs
namespace Pezza.Common.Behaviours
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentValidation;
    using MediatR;

    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) => this.validators = validators;

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            if (this.validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);

                var validationResults = await Task.WhenAll(this.validators.Select(v => v.ValidateAsync(context, cancellationToken)));
                var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null);

                if (!failures.Any())
                {
                    throw new ValidationException(failures);
                }
            }
            return await next();
        }
    }
}
```

DependencyInjection.cs in Pezza.Core

```cs
namespace Pezza.Core
{
    using System.Reflection;
    using AutoMapper;
    using FluentValidation;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Pezza.Common.Behaviours;
    using Pezza.Common.DTO;
    using Pezza.Common.Profiles;
    using Pezza.DataAccess.Contracts;
    using Pezza.DataAccess.Data;

    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(Assembly.GetExecutingAssembly());
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));

            services.AddTransient(typeof(IDataAccess<OrderDTO>), typeof(OrderDataAccess));
            services.AddTransient(typeof(IDataAccess<StockDTO>), typeof(StockDataAccess));
            services.AddTransient(typeof(IDataAccess<NotifyDTO>), typeof(NotifyDataAccess));
            services.AddTransient(typeof(IDataAccess<ProductDTO>), typeof(ProductDataAccess));
            services.AddTransient(typeof(IDataAccess<CustomerDTO>), typeof(CustomerDataAccess));
            services.AddTransient(typeof(IDataAccess<RestaurantDTO>), typeof(RestaurantDataAccess));

            services.AddAutoMapper(typeof(MappingProfile));

            return services;
        }
    }
}
```

### **Remove Core.Contracts Project**


## **STEP 2 - Unit Tests**

Move to Step 2
[Click Here](https://github.com/entelect-incubator/.NET/tree/master/Phase%202/Step%202) 