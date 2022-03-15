using System.Collections.Concurrent;
using Avalonia.AXAML.LanguageServer.ProjectModel;
using Avalonia.Ide.CompletionEngine;

namespace Avalonia.AXAML.LanguageServer.AssemblyMetadata
{
    public class AvaloniaMetadataShepard
    {
        private readonly AvaloniaMetadataLoader _metadataLoader;
        private readonly ProjectShepard _projectShepard;

        public AvaloniaMetadataShepard(AvaloniaMetadataLoader metadataLoader, ProjectShepard projectShepard)
        {
            _metadataLoader = metadataLoader;
            _projectShepard = projectShepard;
        }

        public ConcurrentDictionary<string, Task<Metadata>> ProjectMetadata { get; } = new ConcurrentDictionary<string, Task<Metadata>>();

        public async Task<Metadata> GetMetadataForProject(string projectPath)
        {
            var metadataTask = await ProjectMetadata.GetOrAdd(
                projectPath, 
                async n => await _metadataLoader.CreateMetadataForProject(_projectShepard.GetProject(n)));
            return metadataTask;
        }

        internal void InvalidateMetadata(string path)
        {
            if(ProjectMetadata.TryRemove(path, out _))
            {
                // If metadata exists regenerate it
                GetMetadataForProject(path);
            }
        }
    }
}
