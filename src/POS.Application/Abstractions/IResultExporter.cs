using Pos.Application.Dtos;

namespace Pos.Application.Abstractions;

public interface IResultExporter
{
    string FileExtension { get; }

    void Export(IReadOnlyList<FinalResultDto> results, string path);
}