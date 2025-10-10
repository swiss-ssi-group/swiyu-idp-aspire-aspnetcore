var builder = DistributedApplication.CreateBuilder(args);

var IDENTITY_PROVIDER = "identityProvider";
var WEB_CLIENT = "webClient";
var API_SERVICE = "apiService";
var CACHE = "cache";

var cache = builder.AddRedis(CACHE);

var identityProvider = builder.AddProject<Projects.Idp_Swiyu_IdentityProvider>(IDENTITY_PROVIDER)
    .WithExternalHttpEndpoints()
    .WithReference(cache)
    .WaitFor(cache);

var apiService = builder.AddProject<Projects.Idp_Swiyu_ApiService>(API_SERVICE)
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.Idp_Swiyu_Web>(WEB_CLIENT)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(apiService)
    .WaitFor(apiService)
    .WaitFor(identityProvider);

builder.Build().Run();
