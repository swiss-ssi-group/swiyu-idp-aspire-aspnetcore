var builder = DistributedApplication.CreateBuilder(args);

var IDENTITY_PROVIDER = "identityProvider";
var WEB_CLIENT = "webClient";
var CACHE = "cache";

var cache = builder.AddRedis(CACHE);

var identityProvider = builder.AddProject<Projects.Idp_Swiyu_IdentityProvider>(IDENTITY_PROVIDER)
    .WithExternalHttpEndpoints()
    .WithReference(cache)
    .WaitFor(cache);

builder.AddProject<Projects.Idp_Swiyu_Web>(WEB_CLIENT)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WaitFor(cache)
    .WaitFor(identityProvider);

builder.Build().Run();
