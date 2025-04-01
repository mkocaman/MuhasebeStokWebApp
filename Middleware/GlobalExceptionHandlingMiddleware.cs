using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MuhasebeStokWebApp.Middleware
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Beklenmeyen bir hata oluştu");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "text/html";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            // AJAX isteği mi kontrol et
            if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                context.Response.ContentType = "application/json";
                string result = JsonSerializer.Serialize(new
                {
                    Success = false,
                    Message = "İşlem sırasında bir hata oluştu",
                    Details = exception.ToString()
                });
                await context.Response.WriteAsync(result);
            }
            else
            {
                // Normal sayfa isteğinde basit bir hata sayfası döndür
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
                </head>
                <body>
                    <div class='container mt-5'>
                        <div class='row'>
                            <div class='col-md-8 offset-md-2'>
                                <div class='card'>
                                    <div class='card-header bg-danger text-white'>
                                        <h5 class='mb-0'>Hata Oluştu</h5>
                                    </div>
                                    <div class='card-body'>
                                        <p>İşlem sırasında bir hata oluştu. Lütfen daha sonra tekrar deneyin.</p>
                                        <a href='/' class='btn btn-primary'>Ana Sayfaya Dön</a>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <script>
                        $(document).ready(function() {{
                            Swal.fire({{
                                title: 'Hata!',
                                text: 'İşlem sırasında bir hata oluştu. Lütfen daha sonra tekrar deneyin.',
                                icon: 'error',
                                confirmButtonText: 'Tamam'
                            }});
                        }});
                    </script>
                </body>
                </html>";
                
                await context.Response.WriteAsync(errorHtml);
            }
        }
    }
} 