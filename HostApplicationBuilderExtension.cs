using NgLoader.Queries.Converters;
using NgLoader.Queries.Options;
using NgLoader.Queries.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace NgLoader.Queries
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
