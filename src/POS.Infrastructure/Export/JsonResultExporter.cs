using Pos.Application.Abstractions;
using Pos.Application.Dtos;
using Pos.Infrastructure.Persistence.Json;

namespace Pos.Infrastructure.Export;

public sealed class JsonResultExporter : IResultExporter
{
    public string FileExtension => ".json";

    public void Export(IReadOnlyList<FinalResultDto> results, string path)
        => new JsonFileStore(path).Write(results);
}