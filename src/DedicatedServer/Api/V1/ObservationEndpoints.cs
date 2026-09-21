using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using GOILauncher.Multiplayer.DedicatedServer.Web;
using GOILauncher.Multiplayer.Server.Services;

namespace GOILauncher.Multiplayer.DedicatedServer.Api.V1
{
    public static class ObservationEndpoints
    {
        private const int DefaultLogLimit = 300;

        public static RouteGroupBuilder MapObservationApi(
            this WebApplication app,
            IObservationService observation,
            WebLogTarget logs,
            int gamePort)
        {
            // Keep every API under one versioned group. Authentication can later be attached here
            // without changing the observation service or individual endpoint implementations.
            var api = app.MapGroup("/api/v1").WithTags("Observation");

            api.MapGet("/snapshot", () => Results.Json(
                    ObservationContractMapper.Map(observation.Snapshot, gamePort, System.DateTimeOffset.UtcNow)))
                .WithName("GetObservationSnapshot");

            api.MapGet("/logs", (long? after, int? limit) =>
                {
                    var rows = logs.Fetch(after ?? 0, limit ?? DefaultLogLimit,
                        out var nextSeq, out var reset);
                    return Results.Json(new LogBatchDto(rows, nextSeq, reset));
                })
                .WithName("GetLogBatch");

            return api;
        }
    }
}
