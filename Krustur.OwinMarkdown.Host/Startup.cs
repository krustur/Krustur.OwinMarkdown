using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Markdig;
using System.IO;
using Microsoft.Extensions.FileProviders;

namespace Krustur.OwinMarkdown.Host
{
    public class Startup
    {
        private static string ContentRootPath;
        private MarkdownPipeline _pipeline;

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
            _pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //Log.Logger = new LoggerConfiguration()
            //    .Enrich.FromLogContext()
            //    .WriteTo.Console()
            //    .CreateLogger();

            ContentRootPath = env.ContentRootPath;

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Use(async (context, next) =>
            {
                var requestPath = context.Request.Path.Value;
                if (requestPath.EndsWith("/"))
                {
                    requestPath += "README.md";
                }
                if (requestPath.StartsWith("/"))
                {
                    requestPath = requestPath.Substring(1);
                }
                requestPath = requestPath.Replace('/', '\\');
                var filePath = Path.Combine(ContentRootPath, "Markdown", requestPath);
                if (filePath.ToLower().EndsWith(".md") && File.Exists(filePath))
                {

                    using (var reader = new System.IO.StreamReader(filePath))
                    {
                        var markdown = reader.ReadToEnd();
                        //var html = CommonMark.CommonMarkConverter.Convert(markdown);
                        var html = Markdown.ToHtml(markdown, _pipeline);


                        //        < link rel = "stylesheet" href = "css/styles.css?v=1.0" >
                        //         < !--[if lt IE 9]>
                        //< script src = "https://cdnjs.cloudflare.com/ajax/libs/html5shiv/3.7.3/html5shiv.js" ></ script >
                        //< ![endif]-- >

                        //< script src = "js/scripts.js" ></ script >

                        var htmlStart = @"<html lang=""en"">
<head>
    <meta charset=""utf-8"">
    <title>Hej</title>    
    <meta name=""description"" content =""Desc"">
    <meta name=""author"" content=""Krister"">
</head>
<body>";
                        var htmlEnd = @"</body>
</html>";
                        await context.Response.WriteAsync(htmlStart);
                        await context.Response.WriteAsync(html);
                        await context.Response.WriteAsync(htmlEnd);
                    }

                    //var html = Markdown.ToHtml(markdown);
                    //var html = CommonMark.CommonMarkConverter.Convert(markdown);
                    // parse markdown into document structure
                    //var document = CommonMarkConverter.Parse("[click this link](~/hello)");
                    //var document = CommonMarkConverter.Parse("markdown");
                    //var html = document.Document;

                    //await context.Response.WriteAsync(html);
                }
                else
                {
                    await next.Invoke();
                }

            });


            var physicalFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Markdown");
            app.UseStaticFiles(new StaticFileOptions
            {
                //RequestPath = Path.Combine(ContentRootPath, "Markdown")
                FileProvider = new PhysicalFileProvider(physicalFilePath),
                ServeUnknownFileTypes = true,
                RequestPath = ""

            });

            app.UseDirectoryBrowser(new DirectoryBrowserOptions
            {
                FileProvider = new PhysicalFileProvider(physicalFilePath),
                RequestPath = ""
            });
        }
    }
}
