using Imprex.Queries.Converters;
using Imprex.Queries.Options;
using Imprex.Queries.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Imprex.Queries
{
    public static class HostApplicationBuilderExtension
    {
        public static void AddImprexQueries(this IHostApplicationBuilder builder)
        {
            builder.Services.Configure<QueryOptions>(builder.Configuration.GetSection("Query"));

            builder.Services.Configure<JsonOptions>(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new QueryFilterConverter());
            });

            builder.Services.AddSingleton<QueryService>();
        }
    }
}
