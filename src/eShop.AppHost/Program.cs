using eShop.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddForwardedHeaders();

var redis = builder.AddRedis("redis");
var rabbitMq = builder.AddRabbitMQ("eventbus").WithLifetime(ContainerLifetime.Persistent);
var postgres = builder
    .AddPostgres("postgres")
    .WithImage("ankane/pgvector")
    .WithImageTag("latest")
    .WithLifetime(ContainerLifetime.Persistent);

var catalogDb = postgres.AddDatabase("catalogdb");
var identityDb = postgres.AddDatabase("identitydb");
var orderDb = postgres.AddDatabase("orderingdb");
var webhooksDb = postgres.AddDatabase("webhooksdb");

var launchProfileName = ShouldUseHttpForEndpoints() ? "http" : "https";

// Services
var identityApi = builder
    .AddProject<Projects.Identity_API>("identity-api", launchProfileName)
    .WithExternalHttpEndpoints()
    .WithReference(identityDb);

var identityEndpoint = identityApi.GetEndpoint(launchProfileName);

var orderingApi = builder
    .AddProject<Projects.Ordering_API>("ordering-api")
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq)
    .WithReference(orderDb)
    .WaitFor(orderDb)
    .WithHttpHealthCheck("/health")
    .WithEnvironment("Identity__Url", identityEndpoint);

builder
    .AddProject<Projects.OrderProcessor>("order-processor")
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq)
    .WithReference(orderDb)
    .WaitFor(orderingApi); // wait for the orderingApi to be ready because that contains the EF migrations

builder
    .AddProject<Projects.PaymentProcessor>("payment-processor")
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq);

// Identity has a reference to all of the apps for callback urls, this is a cyclic reference
identityApi.WithEnvironment("OrderingApiClient", orderingApi.GetEndpoint("http"));

builder.Build().Run();

// For test use only.
// Looks for an environment variable that forces the use of HTTP for all the endpoints. We
// are doing this for ease of running the Playwright tests in CI.
static bool ShouldUseHttpForEndpoints()
{
    const string EnvVarName = "ESHOP_USE_HTTP_ENDPOINTS";
    var envValue = Environment.GetEnvironmentVariable(EnvVarName);

    // Attempt to parse the environment variable value; return true if it's exactly "1".
    return int.TryParse(envValue, out int result) && result == 1;
}
