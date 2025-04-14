using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MuhasebeStokWebApp.Middleware
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                string requestId = Activity.Current?.Id ?? context.TraceIdentifier;
                string path = context.Request.Path;
                string method = context.Request.Method;
                string queryString = context.Request.QueryString.ToString();
                string userAgent = context.Request.Headers["User-Agent"].ToString();
                string userIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                string userId = context.User?.Identity?.IsAuthenticated == true ? context.User.Identity.Name : "anonymous";

                // Daha detaylı loglama
                _logger.LogError(ex,
                    "Hata: {RequestId}, Kullanıcı: {UserId}, Path: {Path}, Method: {Method}, QueryString: {QueryString}, IP: {UserIp}, UserAgent: {UserAgent}",
                    requestId, userId, path, method, queryString, userIp, userAgent);

                await HandleExceptionAsync(context, ex, requestId);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception, string requestId)
        {
            context.Response.ContentType = "text/html";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            // Hata tipine göre özel mesajlar
            string userFriendlyMessage = GetUserFriendlyMessage(exception);
            
            // AJAX isteği mi kontrol et
            bool isAjaxRequest = context.Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (isAjaxRequest)
            {
                // AJAX istekleri için JSON yanıtı
                context.Response.ContentType = "application/json";
                
                var responseObject = new
                { 
                    Success = false, 
                    Message = userFriendlyMessage,
                    RequestId = requestId,
                    Detail = _env.IsDevelopment() ? GetDetailedExceptionInfo(exception) : null
                };
                
                await context.Response.WriteAsync(JsonConvert.SerializeObject(responseObject));
            }
            else
            {
                // Normal sayfa isteğinde daha zengin bir hata sayfası döndür
                string errorHtml = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1'>
                    <title>Hata Oluştu</title>
                    <link href='https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css' rel='stylesheet'>
                    <link href='https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.min.css' rel='stylesheet'>
                    <script src='https://cdn.jsdelivr.net/npm/jquery@3.6.0/dist/jquery.min.js'></script>
                    <script src='https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.bundle.min.js'></script>
                    <script src='https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.all.min.js'></script>
                    <style>
                        .error-container {{
                            border-left: 5px solid #dc3545;
                            background-color: #f8f9fa;
                        }}
                        .error-id {{
                            font-family: monospace;
                            font-size: 90%;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container mt-5'>
                        <div class='row'>
                            <div class='col-md-8 offset-md-2'>
                                <div class='card'>
                                    <div class='card-header bg-danger text-white'>
                                        <h5 class='mb-0'>İşlem Sırasında Hata Oluştu</h5>
                                    </div>
                                    <div class='card-body'>
                                        <div class='alert alert-danger error-container p-4 mb-4'>
                                            <h5><i class='fas fa-exclamation-triangle'></i> {userFriendlyMessage}</h5>
                                            <p class='mt-3 mb-0'>Hata Referans No: <span class='error-id'>{requestId}</span></p>
                                        </div>
                                        <p>Bu hata teknik ekibimiz tarafından incelenecektir. Eğer işleme devam edemiyorsanız, lütfen daha sonra tekrar deneyin veya yardım için teknik destek ile iletişime geçin.</p>
                                        <div class='mt-4'>
                                            <a href='/' class='btn btn-primary me-2'><i class='fas fa-home'></i> Ana Sayfaya Dön</a>
                                            <button onclick='window.history.back()' class='btn btn-secondary'><i class='fas fa-arrow-left'></i> Geri Dön</button>
                                        </div>
                                    </div>
                                </div>
                                {(_env.IsDevelopment() ? "<div class='card mt-4'><div class='card-header bg-dark text-white'><h5 class='mb-0'>Hata Detayları (Sadece Geliştirme Ortamında)</h5></div><div class='card-body'><pre>" + System.Web.HttpUtility.HtmlEncode(exception.ToString()) + "</pre></div></div>" : "")}
                            </div>
                        </div>
                    </div>

                    <script>
                        $(document).ready(function() {{
                            Swal.fire({{
                                title: 'Hata!',
                                text: '{userFriendlyMessage}',
                                icon: 'error',
                                confirmButtonText: 'Tamam',
                                footer: '<span>Hata Referans No: {requestId}</span>',
                                allowOutsideClick: false
                            }});
                        }});
                    </script>
                </body>
                </html>";
                
                await context.Response.WriteAsync(errorHtml);
            }
        }

        private string GetUserFriendlyMessage(Exception exception)
        {
            // Hata tipine göre daha anlaşılır mesajlar
            return exception switch
            {
                DbUpdateConcurrencyException => "Veri güncelleme sırasında beklenmeyen bir çakışma oluştu. Lütfen sayfayı yenileyip tekrar deneyin.",
                DbUpdateException => "Veritabanı işlemi sırasında bir hata oluştu. Girdiğiniz veriler doğru formatta mı kontrol edin.",
                TimeoutException => "İşlem zaman aşımına uğradı. Lütfen daha sonra tekrar deneyin.",
                UnauthorizedAccessException => "Bu işlemi gerçekleştirmek için gereken yetkiye sahip değilsiniz.",
                ArgumentException => "Geçersiz bir değer girildi. Lütfen girdiğiniz değerleri kontrol edin.",
                InvalidOperationException => "İşlem şu anda gerçekleştirilemiyor. Lütfen sistem yöneticinize başvurun.",
                FormatException => "Geçersiz format. Lütfen girdiğiniz verinin formatını kontrol edin.",
                OverflowException => "Çok büyük veya çok küçük bir değer girdiniz.",
                _ => "İşlem sırasında beklenmeyen bir hata oluştu. Teknik ekibimiz bu konuyla ilgileniyor."
            };
        }

        private object GetDetailedExceptionInfo(Exception exception)
        {
            var exceptionInfo = new Dictionary<string, object>
            {
                ["type"] = exception.GetType().Name,
                ["message"] = exception.Message,
                ["stackTrace"] = exception.StackTrace?.Split('\n')
            };

            if (exception.InnerException != null)
            {
                exceptionInfo["innerException"] = GetDetailedExceptionInfo(exception.InnerException);
            }

            return exceptionInfo;
        }
    }
} 