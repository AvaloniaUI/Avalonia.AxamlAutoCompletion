using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Avalonia.AXAML.LanguageServer.AssemblyMetadata
{
    public class TextDocumentToProjectMapper
    {
        private readonly ILogger<TextDocumentToProjectMapper> _logger;

        public TextDocumentToProjectMapper(ILogger<TextDocumentToProjectMapper> logger)
        {
            this._logger = logger;
        }

        private ConcurrentDictionary<string, string> DocumentToCsprojMapping { get; } = new ConcurrentDictionary<string, string>();

        public string GetProjectForDocument(string documentPath)
        {
            return DocumentToCsprojMapping.GetOrAdd(documentPath, FindProjectFor);
        }

        private string FindProjectFor(string documentPath)
        {
            string path;
            string newPath = Path.GetDirectoryName(documentPath);
            do
            {
                path = newPath;
                string[] projects = Directory.GetFiles(path, "*.csproj");
                if (projects.Any())
                {
                    return projects[0];
                }

                newPath = Path.GetDirectoryName(path);
            }
            while (path != newPath);

            return null;
        }
    }
}
