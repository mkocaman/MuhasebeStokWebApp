carileri listelemek istediğimde aldığım  hata 

An unhandled exception occurred while processing the request.
InvalidOperationException: Unable to resolve service for type 'MuhasebeStokWebApp.Services.SistemLogService' while attempting to activate 'MuhasebeStokWebApp.Controllers.CariController'.
Microsoft.Extensions.DependencyInjection.ActivatorUtilities.ThrowHelperUnableToResolveService(Type type, Type requiredBy)

Stack Query Cookies Headers Routing
InvalidOperationException: Unable to resolve service for type 'MuhasebeStokWebApp.Services.SistemLogService' while attempting to activate 'MuhasebeStokWebApp.Controllers.CariController'.
Microsoft.Extensions.DependencyInjection.ActivatorUtilities.ThrowHelperUnableToResolveService(Type type, Type requiredBy)
lambda_method636(Closure , IServiceProvider , object[] )
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
Microsoft.AspNetCore.Diagnostics.StatusCodePagesMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)

Show raw exception details

Aşağıdaki hatayı fatura detailsde aldım


An unhandled exception occurred while processing the request.
SqlException: Invalid column name 'Aktif'.
Invalid column name 'AktifParaBirimleri'.
Invalid column name 'OlusturmaTarihi'.
Invalid column name 'SoftDelete'.
Microsoft.Data.SqlClient.SqlCommand+<>c.<ExecuteDbDataReaderAsync>b__211_0(Task<SqlDataReader> result)

Stack Query Cookies Headers Routing
SqlException: Invalid column name 'Aktif'. Invalid column name 'AktifParaBirimleri'. Invalid column name 'OlusturmaTarihi'. Invalid column name 'SoftDelete'.
Microsoft.Data.SqlClient.SqlCommand+<>c.<ExecuteDbDataReaderAsync>b__211_0(Task<SqlDataReader> result)
System.Threading.Tasks.ContinuationResultTaskFromResultTask<TAntecedentResult, TResult>.InnerInvoke()
System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, object state)
System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, object state)
System.Threading.Tasks.Task.ExecuteWithThreadLocal(ref Task currentTaskSlot, Thread threadPoolThread)
Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable<T>+AsyncEnumerator.InitializeReaderAsync(AsyncEnumerator enumerator, CancellationToken cancellationToken)
Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal.SqlServerExecutionStrategy.ExecuteAsync<TState, TResult>(TState state, Func<DbContext, TState, CancellationToken, Task<TResult>> operation, Func<DbContext, TState, CancellationToken, Task<ExecutionResult<TResult>>> verifySucceeded, CancellationToken cancellationToken)
Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable<T>+AsyncEnumerator.MoveNextAsync()
System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<TResult>+ConfiguredValueTaskAwaiter.GetResult()
Microsoft.EntityFrameworkCore.Query.ShapedQueryCompilingExpressionVisitor.SingleOrDefaultAsync<TSource>(IAsyncEnumerable<TSource> asyncEnumerable, CancellationToken cancellationToken)
Microsoft.EntityFrameworkCore.Query.ShapedQueryCompilingExpressionVisitor.SingleOrDefaultAsync<TSource>(IAsyncEnumerable<TSource> asyncEnumerable, CancellationToken cancellationToken)
MuhasebeStokWebApp.Controllers.FaturaController.Details(Nullable<Guid> id) in FaturaController.cs
+
            var sistemAyarlari = await _context.SistemAyarlari.FirstOrDefaultAsync();
Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor+TaskOfIActionResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, object controller, object[] arguments)
System.Runtime.CompilerServices.ValueTaskAwaiter<TResult>.GetResult()
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Awaited|12_0(ControllerActionInvoker invoker, ValueTask<IActionResult> actionResultValueTask)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, object state, bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(ref State next, ref Scope scope, ref object state, ref bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeInnerFilterAsync>g__Awaited|13_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, object state, bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextResourceFilter>g__Awaited|25_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, object state, bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Rethrow(ResourceExecutedContextSealed context)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Next(ref State next, ref Scope scope, ref object state, ref bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeFilterPipelineAsync>g__Awaited|20_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, object state, bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Diagnostics.StatusCodePagesMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)

Show raw exception details

yeni kasa ekle butonuna bastığımda aşağıdaki hatayı alıyorum,

An unhandled exception occurred while processing the request.
SqlException: Invalid column name 'Aciklama'.
Invalid column name 'BazParaBirimi'.
Invalid column name 'Kaynak'.
Invalid column name 'Kur'.
Invalid column name 'OlusturmaTarihi'.
Invalid column name 'ParaBirimi'.
Invalid column name 'SoftDelete'.
System.Threading.Tasks.ContinuationResultTaskFromResultTask<TAntecedentResult, TResult>.InnerInvoke()

Stack Query Cookies Headers Routing
SqlException: Invalid column name 'Aciklama'. Invalid column name 'BazParaBirimi'. Invalid column name 'Kaynak'. Invalid column name 'Kur'. Invalid column name 'OlusturmaTarihi'. Invalid column name 'ParaBirimi'. Invalid column name 'SoftDelete'.
System.Threading.Tasks.ContinuationResultTaskFromResultTask<TAntecedentResult, TResult>.InnerInvoke()
System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, object state)
System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, object state)
System.Threading.Tasks.Task.ExecuteWithThreadLocal(ref Task currentTaskSlot, Thread threadPoolThread)
Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable<T>+AsyncEnumerator.InitializeReaderAsync(AsyncEnumerator enumerator, CancellationToken cancellationToken)
Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal.SqlServerExecutionStrategy.ExecuteAsync<TState, TResult>(TState state, Func<DbContext, TState, CancellationToken, Task<TResult>> operation, Func<DbContext, TState, CancellationToken, Task<ExecutionResult<TResult>>> verifySucceeded, CancellationToken cancellationToken)
Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable<T>+AsyncEnumerator.MoveNextAsync()
Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync<TSource>(IQueryable<TSource> source, CancellationToken cancellationToken)
Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync<TSource>(IQueryable<TSource> source, CancellationToken cancellationToken)
MuhasebeStokWebApp.Controllers.KasaController.Create() in KasaController.cs
+
            var dovizKurlari = await _context.DovizKurlari
Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor+TaskOfIActionResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, object controller, object[] arguments)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Awaited|12_0(ControllerActionInvoker invoker, ValueTask<IActionResult> actionResultValueTask)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, object state, bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(ref State next, ref Scope scope, ref object state, ref bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeInnerFilterAsync>g__Awaited|13_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, object state, bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextResourceFilter>g__Awaited|25_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, object state, bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Rethrow(ResourceExecutedContextSealed context)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Next(ref State next, ref Scope scope, ref object state, ref bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeFilterPipelineAsync>g__Awaited|20_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, object state, bool isCompleted)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Diagnostics.StatusCodePagesMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)

Show raw exception details

kasa hareketlerini görmek istediğimde aldığım hata aşağıda,

An unhandled exception occurred while processing the request.
AmbiguousMatchException: The request matched multiple endpoints. Matches:

MuhasebeStokWebApp.Controllers.KasaController.Hareketler (MuhasebeStokWebApp)
MuhasebeStokWebApp.Controllers.KasaController.Hareketler (MuhasebeStokWebApp)
Microsoft.AspNetCore.Routing.Matching.DefaultEndpointSelector.ReportAmbiguity(Span<CandidateState> candidateState)

Stack Query Cookies Headers Routing
AmbiguousMatchException: The request matched multiple endpoints. Matches: MuhasebeStokWebApp.Controllers.KasaController.Hareketler (MuhasebeStokWebApp) MuhasebeStokWebApp.Controllers.KasaController.Hareketler (MuhasebeStokWebApp)
Microsoft.AspNetCore.Routing.Matching.DefaultEndpointSelector.ReportAmbiguity(Span<CandidateState> candidateState)
Microsoft.AspNetCore.Routing.Matching.DefaultEndpointSelector.ProcessFinalCandidates(HttpContext httpContext, Span<CandidateState> candidateState)
Microsoft.AspNetCore.Routing.Matching.DefaultEndpointSelector.Select(HttpContext httpContext, Span<CandidateState> candidateState)
Microsoft.AspNetCore.Routing.Matching.DfaMatcher.MatchAsync(HttpContext httpContext)
Microsoft.AspNetCore.Routing.EndpointRoutingMiddleware.Invoke(HttpContext httpContext)
Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.HttpsPolicy.HttpsRedirectionMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Diagnostics.StatusCodePagesMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)

Show raw exception details


döviz kurları sayfasını açmak istediğimde aldığım hata aşağıda,

An unhandled exception occurred while processing the request.
InvalidOperationException: Unable to resolve service for type 'MuhasebeStokWebApp.Services.IDovizService' while attempting to activate 'MuhasebeStokWebApp.Controllers.DovizController'.
Microsoft.Extensions.DependencyInjection.ActivatorUtilities.ThrowHelperUnableToResolveService(Type type, Type requiredBy)

Stack Query Cookies Headers Routing
InvalidOperationException: Unable to resolve service for type 'MuhasebeStokWebApp.Services.IDovizService' while attempting to activate 'MuhasebeStokWebApp.Controllers.DovizController'.
Microsoft.Extensions.DependencyInjection.ActivatorUtilities.ThrowHelperUnableToResolveService(Type type, Type requiredBy)
lambda_method2334(Closure , IServiceProvider , object[] )
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
Microsoft.AspNetCore.Diagnostics.StatusCodePagesMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)

Show raw exception details

manuel döviz kuru ekle sayfasını açmak istediğimde aldığım hata aşağıda,

An unhandled exception occurred while processing the request.
InvalidOperationException: Unable to resolve service for type 'MuhasebeStokWebApp.Services.IDovizService' while attempting to activate 'MuhasebeStokWebApp.Controllers.DovizController'.
Microsoft.Extensions.DependencyInjection.ActivatorUtilities.ThrowHelperUnableToResolveService(Type type, Type requiredBy)

Stack Query Cookies Headers Routing
InvalidOperationException: Unable to resolve service for type 'MuhasebeStokWebApp.Services.IDovizService' while attempting to activate 'MuhasebeStokWebApp.Controllers.DovizController'.
Microsoft.Extensions.DependencyInjection.ActivatorUtilities.ThrowHelperUnableToResolveService(Type type, Type requiredBy)
lambda_method2336(Closure , IServiceProvider , object[] )
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
Microsoft.AspNetCore.Diagnostics.StatusCodePagesMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)

Show raw exception details


Döviz çevirici sayfasını açmak istediğimde aldığım hata aşağıda,

An unhandled exception occurred while processing the request.
InvalidOperationException: Unable to resolve service for type 'MuhasebeStokWebApp.Services.IDovizService' while attempting to activate 'MuhasebeStokWebApp.Controllers.DovizController'.
Microsoft.Extensions.DependencyInjection.ActivatorUtilities.ThrowHelperUnableToResolveService(Type type, Type requiredBy)

Stack Query Cookies Headers Routing
InvalidOperationException: Unable to resolve service for type 'MuhasebeStokWebApp.Services.IDovizService' while attempting to activate 'MuhasebeStokWebApp.Controllers.DovizController'.
Microsoft.Extensions.DependencyInjection.ActivatorUtilities.ThrowHelperUnableToResolveService(Type type, Type requiredBy)
lambda_method2344(Closure , IServiceProvider , object[] )
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
Microsoft.AspNetCore.Diagnostics.StatusCodePagesMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)

Show raw exception details


döviz ayarları sayfasını açmak istediğim de aldığım hata aşağıda,

An unhandled exception occurred while processing the request.
InvalidOperationException: Unable to resolve service for type 'MuhasebeStokWebApp.Services.IDovizService' while attempting to activate 'MuhasebeStokWebApp.Controllers.DovizController'.
Microsoft.Extensions.DependencyInjection.ActivatorUtilities.ThrowHelperUnableToResolveService(Type type, Type requiredBy)

Stack Query Cookies Headers Routing
InvalidOperationException: Unable to resolve service for type 'MuhasebeStokWebApp.Services.IDovizService' while attempting to activate 'MuhasebeStokWebApp.Controllers.DovizController'.
Microsoft.Extensions.DependencyInjection.ActivatorUtilities.ThrowHelperUnableToResolveService(Type type, Type requiredBy)
lambda_method2352(Closure , IServiceProvider , object[] )
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
Microsoft.AspNetCore.Diagnostics.StatusCodePagesMiddleware.Invoke(HttpContext context)
Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)

Show raw exception details