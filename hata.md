An unhandled exception occurred while processing the request.
InvalidOperationException: The entity type 'UrunKategori' requires a primary key to be defined. If you intended to use a keyless entity type, call 'HasNoKey' in 'OnModelCreating'. For more information on keyless entity types, see https://go.microsoft.com/fwlink/?linkid=2141943.
Microsoft.EntityFrameworkCore.Infrastructure.ModelValidator.ValidateNonNullPrimaryKeys(IModel model, IDiagnosticsLogger<Validation> logger)

Stack Query Cookies Headers Routing
InvalidOperationException: The entity type 'UrunKategori' requires a primary key to be defined. If you intended to use a keyless entity type, call 'HasNoKey' in 'OnModelCreating'. For more information on keyless entity types, see https://go.microsoft.com/fwlink/?linkid=2141943.
Microsoft.EntityFrameworkCore.Infrastructure.ModelValidator.ValidateNonNullPrimaryKeys(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.Infrastructure.ModelValidator.Validate(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.Infrastructure.RelationalModelValidator.Validate(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal.SqlServerModelValidator.Validate(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.Infrastructure.ModelRuntimeInitializer.Initialize(IModel model, bool designTime, IDiagnosticsLogger<Validation> validationLogger)
Microsoft.EntityFrameworkCore.Infrastructure.ModelSource.CreateModel(DbContext context, ModelCreationDependencies modelCreationDependencies, bool designTime)
Microsoft.EntityFrameworkCore.Infrastructure.ModelSource.GetModel(DbContext context, ModelCreationDependencies modelCreationDependencies, bool designTime)
Microsoft.EntityFrameworkCore.Internal.DbContextServices.CreateModel(bool designTime)
Microsoft.EntityFrameworkCore.Internal.DbContextServices.get_Model()
Microsoft.EntityFrameworkCore.Infrastructure.EntityFrameworkServicesBuilder+<>c.<TryAddCoreServices>b__8_4(IServiceProvider p)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService(IServiceProvider provider, Type serviceType)
Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<T>(IServiceProvider provider)
Microsoft.EntityFrameworkCore.DbContext.get_DbContextDependencies()
Microsoft.EntityFrameworkCore.DbContext.get_ContextServices()
Microsoft.EntityFrameworkCore.DbContext.get_InternalServiceProvider()
Microsoft.EntityFrameworkCore.DbContext.get_DbContextDependencies()
Microsoft.EntityFrameworkCore.DbContext.Set<TEntity>()
MuhasebeStokWebApp.Data.Repositories.Repository<T>..ctor(ApplicationDbContext context) in Repository.cs
+
        public Repository(ApplicationDbContext context)
MuhasebeStokWebApp.Data.Repositories.UnitOfWork.Repository<TEntity>() in UnitOfWork.cs
+
            var repository = new Repository<TEntity>(_context);
MuhasebeStokWebApp.Controllers.CariController.Index(string searchString) in CariController.cs
+
            var cariRepository = _unitOfWork.Repository<Cari>();
Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor+TaskOfIActionResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, object controller, object[] arguments)
System.Runtime.CompilerServices.ValueTaskAwaiter<TResult>.GetResult()
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Awaited|12_0(ControllerActionInvoker invoker, ValueTask<IActionResult> actionResultValueTask)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, object state, bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(ref State next, ref Scope scope, ref object state, ref bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeInnerFilterAsync()
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextResourceFilter>g__Awaited|25_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, object state, bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Rethrow(ResourceExecutedContextSealed context)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Next(ref State next, ref Scope scope, ref object state, ref bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.InvokeFilterPipelineAsync()
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)

Show raw exception details

An unhandled exception occurred while processing the request.
InvalidOperationException: The entity type 'UrunKategori' requires a primary key to be defined. If you intended to use a keyless entity type, call 'HasNoKey' in 'OnModelCreating'. For more information on keyless entity types, see https://go.microsoft.com/fwlink/?linkid=2141943.
Microsoft.EntityFrameworkCore.Infrastructure.ModelValidator.ValidateNonNullPrimaryKeys(IModel model, IDiagnosticsLogger<Validation> logger)

Stack Query Cookies Headers Routing
InvalidOperationException: The entity type 'UrunKategori' requires a primary key to be defined. If you intended to use a keyless entity type, call 'HasNoKey' in 'OnModelCreating'. For more information on keyless entity types, see https://go.microsoft.com/fwlink/?linkid=2141943.
Microsoft.EntityFrameworkCore.Infrastructure.ModelValidator.ValidateNonNullPrimaryKeys(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.Infrastructure.ModelValidator.Validate(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.Infrastructure.RelationalModelValidator.Validate(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal.SqlServerModelValidator.Validate(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.Infrastructure.ModelRuntimeInitializer.Initialize(IModel model, bool designTime, IDiagnosticsLogger<Validation> validationLogger)
Microsoft.EntityFrameworkCore.Infrastructure.ModelSource.CreateModel(DbContext context, ModelCreationDependencies modelCreationDependencies, bool designTime)
Microsoft.EntityFrameworkCore.Infrastructure.ModelSource.GetModel(DbContext context, ModelCreationDependencies modelCreationDependencies, bool designTime)
Microsoft.EntityFrameworkCore.Internal.DbContextServices.CreateModel(bool designTime)
Microsoft.EntityFrameworkCore.Internal.DbContextServices.get_Model()
Microsoft.EntityFrameworkCore.Infrastructure.EntityFrameworkServicesBuilder+<>c.<TryAddCoreServices>b__8_4(IServiceProvider p)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService(IServiceProvider provider, Type serviceType)
Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<T>(IServiceProvider provider)
Microsoft.EntityFrameworkCore.DbContext.get_DbContextDependencies()
Microsoft.EntityFrameworkCore.DbContext.get_ContextServices()
Microsoft.EntityFrameworkCore.DbContext.get_InternalServiceProvider()
Microsoft.EntityFrameworkCore.DbContext.get_DbContextDependencies()
Microsoft.EntityFrameworkCore.DbContext.Set<TEntity>()
MuhasebeStokWebApp.Data.Repositories.Repository<T>..ctor(ApplicationDbContext context) in Repository.cs
+
        public Repository(ApplicationDbContext context)
MuhasebeStokWebApp.Data.Repositories.UnitOfWork.Repository<TEntity>() in UnitOfWork.cs
+
            var repository = new Repository<TEntity>(_context);
MuhasebeStokWebApp.Controllers.UrunController.Index() in UrunController.cs
+
            var urunRepository = _unitOfWork.Repository<Urun>();
Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor+TaskOfIActionResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, object controller, object[] arguments)
System.Runtime.CompilerServices.ValueTaskAwaiter<TResult>.GetResult()
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Awaited|12_0(ControllerActionInvoker invoker, ValueTask<IActionResult> actionResultValueTask)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, object state, bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(ref State next, ref Scope scope, ref object state, ref bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeInnerFilterAsync()
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextResourceFilter>g__Awaited|25_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, object state, bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Rethrow(ResourceExecutedContextSealed context)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Next(ref State next, ref Scope scope, ref object state, ref bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.InvokeFilterPipelineAsync()
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)

Show raw exception details

An unhandled exception occurred while processing the request.
InvalidOperationException: The entity type 'UrunKategori' requires a primary key to be defined. If you intended to use a keyless entity type, call 'HasNoKey' in 'OnModelCreating'. For more information on keyless entity types, see https://go.microsoft.com/fwlink/?linkid=2141943.
Microsoft.EntityFrameworkCore.Infrastructure.ModelValidator.ValidateNonNullPrimaryKeys(IModel model, IDiagnosticsLogger<Validation> logger)

Stack Query Cookies Headers Routing
InvalidOperationException: The entity type 'UrunKategori' requires a primary key to be defined. If you intended to use a keyless entity type, call 'HasNoKey' in 'OnModelCreating'. For more information on keyless entity types, see https://go.microsoft.com/fwlink/?linkid=2141943.
Microsoft.EntityFrameworkCore.Infrastructure.ModelValidator.ValidateNonNullPrimaryKeys(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.Infrastructure.ModelValidator.Validate(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.Infrastructure.RelationalModelValidator.Validate(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal.SqlServerModelValidator.Validate(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.Infrastructure.ModelRuntimeInitializer.Initialize(IModel model, bool designTime, IDiagnosticsLogger<Validation> validationLogger)
Microsoft.EntityFrameworkCore.Infrastructure.ModelSource.CreateModel(DbContext context, ModelCreationDependencies modelCreationDependencies, bool designTime)
Microsoft.EntityFrameworkCore.Infrastructure.ModelSource.GetModel(DbContext context, ModelCreationDependencies modelCreationDependencies, bool designTime)
Microsoft.EntityFrameworkCore.Internal.DbContextServices.CreateModel(bool designTime)
Microsoft.EntityFrameworkCore.Internal.DbContextServices.get_Model()
Microsoft.EntityFrameworkCore.Infrastructure.EntityFrameworkServicesBuilder+<>c.<TryAddCoreServices>b__8_4(IServiceProvider p)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService(IServiceProvider provider, Type serviceType)
Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<T>(IServiceProvider provider)
Microsoft.EntityFrameworkCore.DbContext.get_DbContextDependencies()
Microsoft.EntityFrameworkCore.DbContext.get_ContextServices()
Microsoft.EntityFrameworkCore.DbContext.get_Model()
Microsoft.EntityFrameworkCore.Internal.InternalDbSet<TEntity>.get_EntityType()
Microsoft.EntityFrameworkCore.Internal.InternalDbSet<TEntity>.CheckState()
Microsoft.EntityFrameworkCore.Internal.InternalDbSet<TEntity>.get_EntityQueryable()
Microsoft.EntityFrameworkCore.Internal.InternalDbSet<TEntity>.System.Linq.IQueryable.get_Provider()
System.Linq.Queryable.OrderByDescending<TSource, TKey>(IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector)
MuhasebeStokWebApp.Controllers.UrunKategoriController.Index() in UrunKategoriController.cs
+
            var kategoriler = await _context.UrunKategorileri
Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor+TaskOfIActionResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, object controller, object[] arguments)
System.Runtime.CompilerServices.ValueTaskAwaiter<TResult>.GetResult()
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Awaited|12_0(ControllerActionInvoker invoker, ValueTask<IActionResult> actionResultValueTask)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, object state, bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(ref State next, ref Scope scope, ref object state, ref bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeInnerFilterAsync()
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextResourceFilter>g__Awaited|25_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, object state, bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Rethrow(ResourceExecutedContextSealed context)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Next(ref State next, ref Scope scope, ref object state, ref bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.InvokeFilterPipelineAsync()
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)

Show raw exception details


An unhandled exception occurred while processing the request.
InvalidOperationException: The entity type 'UrunKategori' requires a primary key to be defined. If you intended to use a keyless entity type, call 'HasNoKey' in 'OnModelCreating'. For more information on keyless entity types, see https://go.microsoft.com/fwlink/?linkid=2141943.
Microsoft.EntityFrameworkCore.Infrastructure.ModelValidator.ValidateNonNullPrimaryKeys(IModel model, IDiagnosticsLogger<Validation> logger)

Stack Query Cookies Headers Routing
InvalidOperationException: The entity type 'UrunKategori' requires a primary key to be defined. If you intended to use a keyless entity type, call 'HasNoKey' in 'OnModelCreating'. For more information on keyless entity types, see https://go.microsoft.com/fwlink/?linkid=2141943.
Microsoft.EntityFrameworkCore.Infrastructure.ModelValidator.ValidateNonNullPrimaryKeys(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.Infrastructure.ModelValidator.Validate(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.Infrastructure.RelationalModelValidator.Validate(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal.SqlServerModelValidator.Validate(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.Infrastructure.ModelRuntimeInitializer.Initialize(IModel model, bool designTime, IDiagnosticsLogger<Validation> validationLogger)
Microsoft.EntityFrameworkCore.Infrastructure.ModelSource.CreateModel(DbContext context, ModelCreationDependencies modelCreationDependencies, bool designTime)
Microsoft.EntityFrameworkCore.Infrastructure.ModelSource.GetModel(DbContext context, ModelCreationDependencies modelCreationDependencies, bool designTime)
Microsoft.EntityFrameworkCore.Internal.DbContextServices.CreateModel(bool designTime)
Microsoft.EntityFrameworkCore.Internal.DbContextServices.get_Model()
Microsoft.EntityFrameworkCore.Infrastructure.EntityFrameworkServicesBuilder+<>c.<TryAddCoreServices>b__8_4(IServiceProvider p)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService(IServiceProvider provider, Type serviceType)
Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<T>(IServiceProvider provider)
Microsoft.EntityFrameworkCore.DbContext.get_DbContextDependencies()
Microsoft.EntityFrameworkCore.DbContext.get_ContextServices()
Microsoft.EntityFrameworkCore.DbContext.get_Model()
Microsoft.EntityFrameworkCore.Internal.InternalDbSet<TEntity>.get_EntityType()
Microsoft.EntityFrameworkCore.Internal.InternalDbSet<TEntity>.CheckState()
Microsoft.EntityFrameworkCore.Internal.InternalDbSet<TEntity>.get_EntityQueryable()
Microsoft.EntityFrameworkCore.Internal.InternalDbSet<TEntity>.System.Linq.IQueryable.get_Provider()
System.Linq.Queryable.Where<TSource>(IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
MuhasebeStokWebApp.Controllers.BirimController.Index() in BirimController.cs
+
            var birimler = await _context.Birimler
Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor+TaskOfIActionResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, object controller, object[] arguments)
System.Runtime.CompilerServices.ValueTaskAwaiter<TResult>.GetResult()
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Awaited|12_0(ControllerActionInvoker invoker, ValueTask<IActionResult> actionResultValueTask)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, object state, bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(ref State next, ref Scope scope, ref object state, ref bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeInnerFilterAsync()
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextResourceFilter>g__Awaited|25_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, object state, bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Rethrow(ResourceExecutedContextSealed context)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Next(ref State next, ref Scope scope, ref object state, ref bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.InvokeFilterPipelineAsync()
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)

Show raw exception details


An unhandled exception occurred while processing the request.
InvalidOperationException: The entity type 'UrunKategori' requires a primary key to be defined. If you intended to use a keyless entity type, call 'HasNoKey' in 'OnModelCreating'. For more information on keyless entity types, see https://go.microsoft.com/fwlink/?linkid=2141943.
Microsoft.EntityFrameworkCore.Infrastructure.ModelValidator.ValidateNonNullPrimaryKeys(IModel model, IDiagnosticsLogger<Validation> logger)

Stack Query Cookies Headers Routing
InvalidOperationException: The entity type 'UrunKategori' requires a primary key to be defined. If you intended to use a keyless entity type, call 'HasNoKey' in 'OnModelCreating'. For more information on keyless entity types, see https://go.microsoft.com/fwlink/?linkid=2141943.
Microsoft.EntityFrameworkCore.Infrastructure.ModelValidator.ValidateNonNullPrimaryKeys(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.Infrastructure.ModelValidator.Validate(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.Infrastructure.RelationalModelValidator.Validate(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal.SqlServerModelValidator.Validate(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.Infrastructure.ModelRuntimeInitializer.Initialize(IModel model, bool designTime, IDiagnosticsLogger<Validation> validationLogger)
Microsoft.EntityFrameworkCore.Infrastructure.ModelSource.CreateModel(DbContext context, ModelCreationDependencies modelCreationDependencies, bool designTime)
Microsoft.EntityFrameworkCore.Infrastructure.ModelSource.GetModel(DbContext context, ModelCreationDependencies modelCreationDependencies, bool designTime)
Microsoft.EntityFrameworkCore.Internal.DbContextServices.CreateModel(bool designTime)
Microsoft.EntityFrameworkCore.Internal.DbContextServices.get_Model()
Microsoft.EntityFrameworkCore.Infrastructure.EntityFrameworkServicesBuilder+<>c.<TryAddCoreServices>b__8_4(IServiceProvider p)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService(IServiceProvider provider, Type serviceType)
Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<T>(IServiceProvider provider)
Microsoft.EntityFrameworkCore.DbContext.get_DbContextDependencies()
Microsoft.EntityFrameworkCore.DbContext.get_ContextServices()
Microsoft.EntityFrameworkCore.DbContext.get_Model()
Microsoft.EntityFrameworkCore.Internal.InternalDbSet<TEntity>.get_EntityType()
Microsoft.EntityFrameworkCore.Internal.InternalDbSet<TEntity>.CheckState()
Microsoft.EntityFrameworkCore.Internal.InternalDbSet<TEntity>.get_EntityQueryable()
Microsoft.EntityFrameworkCore.Internal.InternalDbSet<TEntity>.System.Linq.IQueryable.get_Provider()
System.Linq.Queryable.Where<TSource>(IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
MuhasebeStokWebApp.Controllers.DepoController.Index() in DepoController.cs
+
            var depolar = await _context.Depolar
Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor+TaskOfIActionResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, object controller, object[] arguments)
System.Runtime.CompilerServices.ValueTaskAwaiter<TResult>.GetResult()
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Awaited|12_0(ControllerActionInvoker invoker, ValueTask<IActionResult> actionResultValueTask)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, object state, bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(ref State next, ref Scope scope, ref object state, ref bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeInnerFilterAsync()
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextResourceFilter>g__Awaited|25_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, object state, bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Rethrow(ResourceExecutedContextSealed context)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Next(ref State next, ref Scope scope, ref object state, ref bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.InvokeFilterPipelineAsync()
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)

Show raw exception details



An unhandled exception occurred while processing the request.
InvalidOperationException: Unable to resolve service for type 'Microsoft.AspNetCore.Identity.UserManager`1[Microsoft.AspNetCore.Identity.IdentityUser]' while attempting to activate 'MuhasebeStokWebApp.Controllers.FaturaController'.
Microsoft.Extensions.DependencyInjection.ActivatorUtilities.ThrowHelperUnableToResolveService(Type type, Type requiredBy)

Stack Query Cookies Headers Routing
InvalidOperationException: Unable to resolve service for type 'Microsoft.AspNetCore.Identity.UserManager`1[Microsoft.AspNetCore.Identity.IdentityUser]' while attempting to activate 'MuhasebeStokWebApp.Controllers.FaturaController'.
Microsoft.Extensions.DependencyInjection.ActivatorUtilities.ThrowHelperUnableToResolveService(Type type, Type requiredBy)
lambda_method130(Closure , IServiceProvider , object[] )
Microsoft.AspNetCore.Mvc.Controllers.ControllerFactoryProvider+<>c__DisplayClass6_0.<CreateControllerFactory>g__CreateController|0(ControllerContext controllerContext)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(ref State next, ref Scope scope, ref object state, ref bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeInnerFilterAsync()
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextResourceFilter>g__Awaited|25_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, object state, bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Rethrow(ResourceExecutedContextSealed context)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Next(ref State next, ref Scope scope, ref object state, ref bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.InvokeFilterPipelineAsync()
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)

Show raw exception details



An unhandled exception occurred while processing the request.
InvalidOperationException: The entity type 'UrunKategori' requires a primary key to be defined. If you intended to use a keyless entity type, call 'HasNoKey' in 'OnModelCreating'. For more information on keyless entity types, see https://go.microsoft.com/fwlink/?linkid=2141943.
Microsoft.EntityFrameworkCore.Infrastructure.ModelValidator.ValidateNonNullPrimaryKeys(IModel model, IDiagnosticsLogger<Validation> logger)

Stack Query Cookies Headers Routing
InvalidOperationException: The entity type 'UrunKategori' requires a primary key to be defined. If you intended to use a keyless entity type, call 'HasNoKey' in 'OnModelCreating'. For more information on keyless entity types, see https://go.microsoft.com/fwlink/?linkid=2141943.
Microsoft.EntityFrameworkCore.Infrastructure.ModelValidator.ValidateNonNullPrimaryKeys(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.Infrastructure.ModelValidator.Validate(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.Infrastructure.RelationalModelValidator.Validate(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal.SqlServerModelValidator.Validate(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.Infrastructure.ModelRuntimeInitializer.Initialize(IModel model, bool designTime, IDiagnosticsLogger<Validation> validationLogger)
Microsoft.EntityFrameworkCore.Infrastructure.ModelSource.CreateModel(DbContext context, ModelCreationDependencies modelCreationDependencies, bool designTime)
Microsoft.EntityFrameworkCore.Infrastructure.ModelSource.GetModel(DbContext context, ModelCreationDependencies modelCreationDependencies, bool designTime)
Microsoft.EntityFrameworkCore.Internal.DbContextServices.CreateModel(bool designTime)
Microsoft.EntityFrameworkCore.Internal.DbContextServices.get_Model()
Microsoft.EntityFrameworkCore.Infrastructure.EntityFrameworkServicesBuilder+<>c.<TryAddCoreServices>b__8_4(IServiceProvider p)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService(IServiceProvider provider, Type serviceType)
Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<T>(IServiceProvider provider)
Microsoft.EntityFrameworkCore.DbContext.get_DbContextDependencies()
Microsoft.EntityFrameworkCore.DbContext.get_ContextServices()
Microsoft.EntityFrameworkCore.DbContext.get_InternalServiceProvider()
Microsoft.EntityFrameworkCore.DbContext.get_DbContextDependencies()
Microsoft.EntityFrameworkCore.DbContext.Set<TEntity>()
MuhasebeStokWebApp.Data.Repositories.Repository<T>..ctor(ApplicationDbContext context) in Repository.cs
+
        public Repository(ApplicationDbContext context)
MuhasebeStokWebApp.Data.Repositories.UnitOfWork.Repository<TEntity>() in UnitOfWork.cs
+
            var repository = new Repository<TEntity>(_context);
MuhasebeStokWebApp.Controllers.IrsaliyeController.Index(string searchString, string irsaliyeTuru, string durum) in IrsaliyeController.cs
+
            var irsaliyeler = await _unitOfWork.Repository<Irsaliye>().GetAsync(
Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor+TaskOfIActionResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, object controller, object[] arguments)
System.Runtime.CompilerServices.ValueTaskAwaiter<TResult>.GetResult()
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Awaited|12_0(ControllerActionInvoker invoker, ValueTask<IActionResult> actionResultValueTask)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, object state, bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(ref State next, ref Scope scope, ref object state, ref bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeInnerFilterAsync()
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextResourceFilter>g__Awaited|25_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, object state, bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Rethrow(ResourceExecutedContextSealed context)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Next(ref State next, ref Scope scope, ref object state, ref bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.InvokeFilterPipelineAsync()
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)

Show raw exception details



An unhandled exception occurred while processing the request.
InvalidOperationException: The entity type 'UrunKategori' requires a primary key to be defined. If you intended to use a keyless entity type, call 'HasNoKey' in 'OnModelCreating'. For more information on keyless entity types, see https://go.microsoft.com/fwlink/?linkid=2141943.
Microsoft.EntityFrameworkCore.Infrastructure.ModelValidator.ValidateNonNullPrimaryKeys(IModel model, IDiagnosticsLogger<Validation> logger)

Stack Query Cookies Headers Routing
InvalidOperationException: The entity type 'UrunKategori' requires a primary key to be defined. If you intended to use a keyless entity type, call 'HasNoKey' in 'OnModelCreating'. For more information on keyless entity types, see https://go.microsoft.com/fwlink/?linkid=2141943.
Microsoft.EntityFrameworkCore.Infrastructure.ModelValidator.ValidateNonNullPrimaryKeys(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.Infrastructure.ModelValidator.Validate(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.Infrastructure.RelationalModelValidator.Validate(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal.SqlServerModelValidator.Validate(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.Infrastructure.ModelRuntimeInitializer.Initialize(IModel model, bool designTime, IDiagnosticsLogger<Validation> validationLogger)
Microsoft.EntityFrameworkCore.Infrastructure.ModelSource.CreateModel(DbContext context, ModelCreationDependencies modelCreationDependencies, bool designTime)
Microsoft.EntityFrameworkCore.Infrastructure.ModelSource.GetModel(DbContext context, ModelCreationDependencies modelCreationDependencies, bool designTime)
Microsoft.EntityFrameworkCore.Internal.DbContextServices.CreateModel(bool designTime)
Microsoft.EntityFrameworkCore.Internal.DbContextServices.get_Model()
Microsoft.EntityFrameworkCore.Infrastructure.EntityFrameworkServicesBuilder+<>c.<TryAddCoreServices>b__8_4(IServiceProvider p)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService(IServiceProvider provider, Type serviceType)
Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<T>(IServiceProvider provider)
Microsoft.EntityFrameworkCore.DbContext.get_DbContextDependencies()
Microsoft.EntityFrameworkCore.DbContext.get_ContextServices()
Microsoft.EntityFrameworkCore.DbContext.get_Model()
Microsoft.EntityFrameworkCore.Internal.InternalDbSet<TEntity>.get_EntityType()
Microsoft.EntityFrameworkCore.Internal.InternalDbSet<TEntity>.CheckState()
Microsoft.EntityFrameworkCore.Internal.InternalDbSet<TEntity>.get_EntityQueryable()
Microsoft.EntityFrameworkCore.Internal.InternalDbSet<TEntity>.System.Linq.IQueryable.get_Provider()
System.Linq.Queryable.Where<TSource>(IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
MuhasebeStokWebApp.Controllers.KasaController.Index() in KasaController.cs
+
            var kasalar = await _context.Kasalar
Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor+TaskOfIActionResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, object controller, object[] arguments)
System.Runtime.CompilerServices.ValueTaskAwaiter<TResult>.GetResult()
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Awaited|12_0(ControllerActionInvoker invoker, ValueTask<IActionResult> actionResultValueTask)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, object state, bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(ref State next, ref Scope scope, ref object state, ref bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeInnerFilterAsync()
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextResourceFilter>g__Awaited|25_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, object state, bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Rethrow(ResourceExecutedContextSealed context)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Next(ref State next, ref Scope scope, ref object state, ref bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.InvokeFilterPipelineAsync()
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)

Show raw exception details



An unhandled exception occurred while processing the request.
InvalidOperationException: The entity type 'UrunKategori' requires a primary key to be defined. If you intended to use a keyless entity type, call 'HasNoKey' in 'OnModelCreating'. For more information on keyless entity types, see https://go.microsoft.com/fwlink/?linkid=2141943.
Microsoft.EntityFrameworkCore.Infrastructure.ModelValidator.ValidateNonNullPrimaryKeys(IModel model, IDiagnosticsLogger<Validation> logger)

Stack Query Cookies Headers Routing
InvalidOperationException: The entity type 'UrunKategori' requires a primary key to be defined. If you intended to use a keyless entity type, call 'HasNoKey' in 'OnModelCreating'. For more information on keyless entity types, see https://go.microsoft.com/fwlink/?linkid=2141943.
Microsoft.EntityFrameworkCore.Infrastructure.ModelValidator.ValidateNonNullPrimaryKeys(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.Infrastructure.ModelValidator.Validate(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.Infrastructure.RelationalModelValidator.Validate(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal.SqlServerModelValidator.Validate(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.Infrastructure.ModelRuntimeInitializer.Initialize(IModel model, bool designTime, IDiagnosticsLogger<Validation> validationLogger)
Microsoft.EntityFrameworkCore.Infrastructure.ModelSource.CreateModel(DbContext context, ModelCreationDependencies modelCreationDependencies, bool designTime)
Microsoft.EntityFrameworkCore.Infrastructure.ModelSource.GetModel(DbContext context, ModelCreationDependencies modelCreationDependencies, bool designTime)
Microsoft.EntityFrameworkCore.Internal.DbContextServices.CreateModel(bool designTime)
Microsoft.EntityFrameworkCore.Internal.DbContextServices.get_Model()
Microsoft.EntityFrameworkCore.Infrastructure.EntityFrameworkServicesBuilder+<>c.<TryAddCoreServices>b__8_4(IServiceProvider p)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService(IServiceProvider provider, Type serviceType)
Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<T>(IServiceProvider provider)
Microsoft.EntityFrameworkCore.DbContext.get_DbContextDependencies()
Microsoft.EntityFrameworkCore.DbContext.get_ContextServices()
Microsoft.EntityFrameworkCore.DbContext.get_Model()
Microsoft.EntityFrameworkCore.Internal.InternalDbSet<TEntity>.get_EntityType()
Microsoft.EntityFrameworkCore.Internal.InternalDbSet<TEntity>.CheckState()
Microsoft.EntityFrameworkCore.Internal.InternalDbSet<TEntity>.get_EntityQueryable()
Microsoft.EntityFrameworkCore.Internal.InternalDbSet<TEntity>.System.Linq.IQueryable.get_Provider()
System.Linq.Queryable.Where<TSource>(IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
MuhasebeStokWebApp.Controllers.KasaController.Index() in KasaController.cs
+
            var kasalar = await _context.Kasalar
Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor+TaskOfIActionResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, object controller, object[] arguments)
System.Runtime.CompilerServices.ValueTaskAwaiter<TResult>.GetResult()
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Awaited|12_0(ControllerActionInvoker invoker, ValueTask<IActionResult> actionResultValueTask)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, object state, bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(ref State next, ref Scope scope, ref object state, ref bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeInnerFilterAsync()
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextResourceFilter>g__Awaited|25_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, object state, bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Rethrow(ResourceExecutedContextSealed context)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Next(ref State next, ref Scope scope, ref object state, ref bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.InvokeFilterPipelineAsync()
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)

Show raw exception details


An unhandled exception occurred while processing the request.
InvalidOperationException: The entity type 'UrunKategori' requires a primary key to be defined. If you intended to use a keyless entity type, call 'HasNoKey' in 'OnModelCreating'. For more information on keyless entity types, see https://go.microsoft.com/fwlink/?linkid=2141943.
Microsoft.EntityFrameworkCore.Infrastructure.ModelValidator.ValidateNonNullPrimaryKeys(IModel model, IDiagnosticsLogger<Validation> logger)

Stack Query Cookies Headers Routing
InvalidOperationException: The entity type 'UrunKategori' requires a primary key to be defined. If you intended to use a keyless entity type, call 'HasNoKey' in 'OnModelCreating'. For more information on keyless entity types, see https://go.microsoft.com/fwlink/?linkid=2141943.
Microsoft.EntityFrameworkCore.Infrastructure.ModelValidator.ValidateNonNullPrimaryKeys(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.Infrastructure.ModelValidator.Validate(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.Infrastructure.RelationalModelValidator.Validate(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal.SqlServerModelValidator.Validate(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.Infrastructure.ModelRuntimeInitializer.Initialize(IModel model, bool designTime, IDiagnosticsLogger<Validation> validationLogger)
Microsoft.EntityFrameworkCore.Infrastructure.ModelSource.CreateModel(DbContext context, ModelCreationDependencies modelCreationDependencies, bool designTime)
Microsoft.EntityFrameworkCore.Infrastructure.ModelSource.GetModel(DbContext context, ModelCreationDependencies modelCreationDependencies, bool designTime)
Microsoft.EntityFrameworkCore.Internal.DbContextServices.CreateModel(bool designTime)
Microsoft.EntityFrameworkCore.Internal.DbContextServices.get_Model()
Microsoft.EntityFrameworkCore.Infrastructure.EntityFrameworkServicesBuilder+<>c.<TryAddCoreServices>b__8_4(IServiceProvider p)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService(IServiceProvider provider, Type serviceType)
Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<T>(IServiceProvider provider)
Microsoft.EntityFrameworkCore.DbContext.get_DbContextDependencies()
Microsoft.EntityFrameworkCore.DbContext.get_ContextServices()
Microsoft.EntityFrameworkCore.DbContext.get_Model()
Microsoft.EntityFrameworkCore.Internal.InternalDbSet<TEntity>.get_EntityType()
Microsoft.EntityFrameworkCore.Internal.InternalDbSet<TEntity>.CheckState()
Microsoft.EntityFrameworkCore.Internal.InternalDbSet<TEntity>.get_EntityQueryable()
Microsoft.EntityFrameworkCore.Internal.InternalDbSet<TEntity>.System.Linq.IQueryable.get_Provider()
System.Linq.Queryable.Where<TSource>(IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
MuhasebeStokWebApp.Controllers.BankaController.Index() in BankaController.cs
+
            var bankalar = await _context.Bankalar
Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor+TaskOfIActionResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, object controller, object[] arguments)
System.Runtime.CompilerServices.ValueTaskAwaiter<TResult>.GetResult()
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Awaited|12_0(ControllerActionInvoker invoker, ValueTask<IActionResult> actionResultValueTask)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, object state, bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(ref State next, ref Scope scope, ref object state, ref bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeInnerFilterAsync()
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextResourceFilter>g__Awaited|25_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, object state, bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Rethrow(ResourceExecutedContextSealed context)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Next(ref State next, ref Scope scope, ref object state, ref bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.InvokeFilterPipelineAsync()
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)

Show raw exception details



An unhandled exception occurred while processing the request.
InvalidOperationException: The entity type 'UrunKategori' requires a primary key to be defined. If you intended to use a keyless entity type, call 'HasNoKey' in 'OnModelCreating'. For more information on keyless entity types, see https://go.microsoft.com/fwlink/?linkid=2141943.
Microsoft.EntityFrameworkCore.Infrastructure.ModelValidator.ValidateNonNullPrimaryKeys(IModel model, IDiagnosticsLogger<Validation> logger)

Stack Query Cookies Headers Routing
InvalidOperationException: The entity type 'UrunKategori' requires a primary key to be defined. If you intended to use a keyless entity type, call 'HasNoKey' in 'OnModelCreating'. For more information on keyless entity types, see https://go.microsoft.com/fwlink/?linkid=2141943.
Microsoft.EntityFrameworkCore.Infrastructure.ModelValidator.ValidateNonNullPrimaryKeys(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.Infrastructure.ModelValidator.Validate(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.Infrastructure.RelationalModelValidator.Validate(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal.SqlServerModelValidator.Validate(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.Infrastructure.ModelRuntimeInitializer.Initialize(IModel model, bool designTime, IDiagnosticsLogger<Validation> validationLogger)
Microsoft.EntityFrameworkCore.Infrastructure.ModelSource.CreateModel(DbContext context, ModelCreationDependencies modelCreationDependencies, bool designTime)
Microsoft.EntityFrameworkCore.Infrastructure.ModelSource.GetModel(DbContext context, ModelCreationDependencies modelCreationDependencies, bool designTime)
Microsoft.EntityFrameworkCore.Internal.DbContextServices.CreateModel(bool designTime)
Microsoft.EntityFrameworkCore.Internal.DbContextServices.get_Model()
Microsoft.EntityFrameworkCore.Infrastructure.EntityFrameworkServicesBuilder+<>c.<TryAddCoreServices>b__8_4(IServiceProvider p)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService(IServiceProvider provider, Type serviceType)
Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<T>(IServiceProvider provider)
Microsoft.EntityFrameworkCore.DbContext.get_DbContextDependencies()
Microsoft.EntityFrameworkCore.DbContext.get_ContextServices()
Microsoft.EntityFrameworkCore.DbContext.get_Model()
Microsoft.EntityFrameworkCore.Internal.InternalDbSet<TEntity>.get_EntityType()
Microsoft.EntityFrameworkCore.Internal.InternalDbSet<TEntity>.CheckState()
Microsoft.EntityFrameworkCore.Internal.InternalDbSet<TEntity>.get_EntityQueryable()
Microsoft.EntityFrameworkCore.Internal.InternalDbSet<TEntity>.System.Linq.IQueryable.get_Provider()
System.Linq.Queryable.Where<TSource>(IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
MuhasebeStokWebApp.Services.DovizService.GetKurlarAsync() in DovizService.cs
+
            return await _context.DovizKurlari.Where(k => k.Aktif).OrderBy(k => k.DovizKodu).ToListAsync();
MuhasebeStokWebApp.Controllers.DovizController.Index() in DovizController.cs
+
            var kurlar = await _dovizService.GetKurlarAsync();
Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor+TaskOfIActionResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, object controller, object[] arguments)
System.Runtime.CompilerServices.ValueTaskAwaiter<TResult>.GetResult()
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Awaited|12_0(ControllerActionInvoker invoker, ValueTask<IActionResult> actionResultValueTask)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, object state, bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(ref State next, ref Scope scope, ref object state, ref bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeInnerFilterAsync()
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextResourceFilter>g__Awaited|25_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, object state, bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Rethrow(ResourceExecutedContextSealed context)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Next(ref State next, ref Scope scope, ref object state, ref bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.InvokeFilterPipelineAsync()
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)

Show raw exception details



An unhandled exception occurred while processing the request.
InvalidOperationException: The entity type 'UrunKategori' requires a primary key to be defined. If you intended to use a keyless entity type, call 'HasNoKey' in 'OnModelCreating'. For more information on keyless entity types, see https://go.microsoft.com/fwlink/?linkid=2141943.
Microsoft.EntityFrameworkCore.Infrastructure.ModelValidator.ValidateNonNullPrimaryKeys(IModel model, IDiagnosticsLogger<Validation> logger)

Stack Query Cookies Headers Routing
InvalidOperationException: The entity type 'UrunKategori' requires a primary key to be defined. If you intended to use a keyless entity type, call 'HasNoKey' in 'OnModelCreating'. For more information on keyless entity types, see https://go.microsoft.com/fwlink/?linkid=2141943.
Microsoft.EntityFrameworkCore.Infrastructure.ModelValidator.ValidateNonNullPrimaryKeys(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.Infrastructure.ModelValidator.Validate(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.Infrastructure.RelationalModelValidator.Validate(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal.SqlServerModelValidator.Validate(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.Infrastructure.ModelRuntimeInitializer.Initialize(IModel model, bool designTime, IDiagnosticsLogger<Validation> validationLogger)
Microsoft.EntityFrameworkCore.Infrastructure.ModelSource.CreateModel(DbContext context, ModelCreationDependencies modelCreationDependencies, bool designTime)
Microsoft.EntityFrameworkCore.Infrastructure.ModelSource.GetModel(DbContext context, ModelCreationDependencies modelCreationDependencies, bool designTime)
Microsoft.EntityFrameworkCore.Internal.DbContextServices.CreateModel(bool designTime)
Microsoft.EntityFrameworkCore.Internal.DbContextServices.get_Model()
Microsoft.EntityFrameworkCore.Infrastructure.EntityFrameworkServicesBuilder+<>c.<TryAddCoreServices>b__8_4(IServiceProvider p)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService(IServiceProvider provider, Type serviceType)
Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<T>(IServiceProvider provider)
Microsoft.EntityFrameworkCore.DbContext.get_DbContextDependencies()
Microsoft.EntityFrameworkCore.DbContext.get_ContextServices()
Microsoft.EntityFrameworkCore.DbContext.get_Model()
Microsoft.EntityFrameworkCore.Internal.InternalDbSet<TEntity>.get_EntityType()
Microsoft.EntityFrameworkCore.Internal.InternalDbSet<TEntity>.CheckState()
Microsoft.EntityFrameworkCore.Internal.InternalDbSet<TEntity>.get_EntityQueryable()
Microsoft.EntityFrameworkCore.Internal.InternalDbSet<TEntity>.System.Linq.IQueryable.get_Provider()
System.Linq.Queryable.Where<TSource>(IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
MuhasebeStokWebApp.Services.DovizService.GetKurlarAsync() in DovizService.cs
+
            return await _context.DovizKurlari.Where(k => k.Aktif).OrderBy(k => k.DovizKodu).ToListAsync();
MuhasebeStokWebApp.Controllers.DovizController.Cevirici() in DovizController.cs
+
            var kurlar = await _dovizService.GetKurlarAsync();
Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor+TaskOfIActionResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, object controller, object[] arguments)
System.Runtime.CompilerServices.ValueTaskAwaiter<TResult>.GetResult()
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Awaited|12_0(ControllerActionInvoker invoker, ValueTask<IActionResult> actionResultValueTask)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, object state, bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(ref State next, ref Scope scope, ref object state, ref bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeInnerFilterAsync()
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextResourceFilter>g__Awaited|25_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, object state, bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Rethrow(ResourceExecutedContextSealed context)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Next(ref State next, ref Scope scope, ref object state, ref bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.InvokeFilterPipelineAsync()
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)

Show raw exception details


An unhandled exception occurred while processing the request.
InvalidOperationException: The entity type 'UrunKategori' requires a primary key to be defined. If you intended to use a keyless entity type, call 'HasNoKey' in 'OnModelCreating'. For more information on keyless entity types, see https://go.microsoft.com/fwlink/?linkid=2141943.
Microsoft.EntityFrameworkCore.Infrastructure.ModelValidator.ValidateNonNullPrimaryKeys(IModel model, IDiagnosticsLogger<Validation> logger)

Stack Query Cookies Headers Routing
InvalidOperationException: The entity type 'UrunKategori' requires a primary key to be defined. If you intended to use a keyless entity type, call 'HasNoKey' in 'OnModelCreating'. For more information on keyless entity types, see https://go.microsoft.com/fwlink/?linkid=2141943.
Microsoft.EntityFrameworkCore.Infrastructure.ModelValidator.ValidateNonNullPrimaryKeys(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.Infrastructure.ModelValidator.Validate(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.Infrastructure.RelationalModelValidator.Validate(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal.SqlServerModelValidator.Validate(IModel model, IDiagnosticsLogger<Validation> logger)
Microsoft.EntityFrameworkCore.Infrastructure.ModelRuntimeInitializer.Initialize(IModel model, bool designTime, IDiagnosticsLogger<Validation> validationLogger)
Microsoft.EntityFrameworkCore.Infrastructure.ModelSource.CreateModel(DbContext context, ModelCreationDependencies modelCreationDependencies, bool designTime)
Microsoft.EntityFrameworkCore.Infrastructure.ModelSource.GetModel(DbContext context, ModelCreationDependencies modelCreationDependencies, bool designTime)
Microsoft.EntityFrameworkCore.Internal.DbContextServices.CreateModel(bool designTime)
Microsoft.EntityFrameworkCore.Internal.DbContextServices.get_Model()
Microsoft.EntityFrameworkCore.Infrastructure.EntityFrameworkServicesBuilder+<>c.<TryAddCoreServices>b__8_4(IServiceProvider p)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitCache(ServiceCallSite callSite, RuntimeResolverContext context, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor<TArgument, TResult>.VisitCallSite(ServiceCallSite callSite, TArgument argument)
Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService(IServiceProvider provider, Type serviceType)
Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<T>(IServiceProvider provider)
Microsoft.EntityFrameworkCore.DbContext.get_DbContextDependencies()
Microsoft.EntityFrameworkCore.DbContext.get_ContextServices()
Microsoft.EntityFrameworkCore.DbContext.get_Model()
Microsoft.EntityFrameworkCore.Internal.InternalDbSet<TEntity>.get_EntityType()
Microsoft.EntityFrameworkCore.Internal.InternalDbSet<TEntity>.CheckState()
Microsoft.EntityFrameworkCore.Internal.InternalDbSet<TEntity>.get_EntityQueryable()
Microsoft.EntityFrameworkCore.Internal.InternalDbSet<TEntity>.System.Linq.IQueryable.get_Provider()
Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ExecuteAsync<TSource, TResult>(MethodInfo operatorMethodInfo, IQueryable<TSource> source, Expression expression, CancellationToken cancellationToken)
Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ExecuteAsync<TSource, TResult>(MethodInfo operatorMethodInfo, IQueryable<TSource> source, CancellationToken cancellationToken)
Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync<TSource>(IQueryable<TSource> source, CancellationToken cancellationToken)
MuhasebeStokWebApp.Controllers.DovizController.Ayarlar() in DovizController.cs
+
            var sistemAyarlari = await _context.SistemAyarlari.FirstOrDefaultAsync();
Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor+TaskOfIActionResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, object controller, object[] arguments)
System.Runtime.CompilerServices.ValueTaskAwaiter<TResult>.GetResult()
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Awaited|12_0(ControllerActionInvoker invoker, ValueTask<IActionResult> actionResultValueTask)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, object state, bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(ref State next, ref Scope scope, ref object state, ref bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeInnerFilterAsync()
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextResourceFilter>g__Awaited|25_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, object state, bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Rethrow(ResourceExecutedContextSealed context)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Next(ref State next, ref Scope scope, ref object state, ref bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.InvokeFilterPipelineAsync()
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)

Show raw exception details



An unhandled exception occurred while processing the request.
InvalidOperationException: Unable to resolve service for type 'Microsoft.AspNetCore.Identity.UserManager`1[Microsoft.AspNetCore.Identity.IdentityUser]' while attempting to activate 'MuhasebeStokWebApp.Controllers.RaporController'.
Microsoft.Extensions.DependencyInjection.ActivatorUtilities.ThrowHelperUnableToResolveService(Type type, Type requiredBy)

Stack Query Cookies Headers Routing
InvalidOperationException: Unable to resolve service for type 'Microsoft.AspNetCore.Identity.UserManager`1[Microsoft.AspNetCore.Identity.IdentityUser]' while attempting to activate 'MuhasebeStokWebApp.Controllers.RaporController'.
Microsoft.Extensions.DependencyInjection.ActivatorUtilities.ThrowHelperUnableToResolveService(Type type, Type requiredBy)
lambda_method196(Closure , IServiceProvider , object[] )
Microsoft.AspNetCore.Mvc.Controllers.ControllerFactoryProvider+<>c__DisplayClass6_0.<CreateControllerFactory>g__CreateController|0(ControllerContext controllerContext)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(ref State next, ref Scope scope, ref object state, ref bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeInnerFilterAsync()
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextResourceFilter>g__Awaited|25_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, object state, bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Rethrow(ResourceExecutedContextSealed context)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Next(ref State next, ref Scope scope, ref object state, ref bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.InvokeFilterPipelineAsync()
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)

Show raw exception details

An unhandled exception occurred while processing the request.
InvalidOperationException: Unable to resolve service for type 'Microsoft.AspNetCore.Identity.UserManager`1[Microsoft.AspNetCore.Identity.IdentityUser]' while attempting to activate 'MuhasebeStokWebApp.Controllers.KullaniciController'.
Microsoft.Extensions.DependencyInjection.ActivatorUtilities.ThrowHelperUnableToResolveService(Type type, Type requiredBy)

Stack Query Cookies Headers Routing
InvalidOperationException: Unable to resolve service for type 'Microsoft.AspNetCore.Identity.UserManager`1[Microsoft.AspNetCore.Identity.IdentityUser]' while attempting to activate 'MuhasebeStokWebApp.Controllers.KullaniciController'.
Microsoft.Extensions.DependencyInjection.ActivatorUtilities.ThrowHelperUnableToResolveService(Type type, Type requiredBy)
lambda_method204(Closure , IServiceProvider , object[] )
Microsoft.AspNetCore.Mvc.Controllers.ControllerFactoryProvider+<>c__DisplayClass6_0.<CreateControllerFactory>g__CreateController|0(ControllerContext controllerContext)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(ref State next, ref Scope scope, ref object state, ref bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.InvokeInnerFilterAsync()
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextResourceFilter>g__Awaited|25_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, object state, bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Rethrow(ResourceExecutedContextSealed context)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Next(ref State next, ref Scope scope, ref object state, ref bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.InvokeFilterPipelineAsync()
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)

Show raw exception details