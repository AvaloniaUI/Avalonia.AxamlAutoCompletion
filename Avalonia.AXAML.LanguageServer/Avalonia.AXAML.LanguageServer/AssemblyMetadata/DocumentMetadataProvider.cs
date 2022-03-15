using Avalonia.AXAML.LanguageServer.ProjectModel;
using Avalonia.Ide.CompletionEngine;

namespace Avalonia.AXAML.LanguageServer.AssemblyMetadata
{
    public class DocumentMetadataProvider
    {
        private readonly TextDocumentToProjectMapper _documentMapper;
        private readonly AvaloniaMetadataShepard _metadataRepository;
        private readonly ProjectShepard _projectShepard;

        public DocumentMetadataProvider(
            TextDocumentToProjectMapper documentMapper, 
            AvaloniaMetadataShepard metadataRepository,
            ProjectShepard projectShepard)
        {
            this._documentMapper = documentMapper;
            this._metadataRepository = metadataRepository;
            this._projectShepard = projectShepard;
        }

        public async Task<Metadata> GetMetadataForDocument(string documentPath)
        {
            string projectPath = _documentMapper.GetProjectForDocument(documentPath);
            if (projectPath == null)
            {
                return null;
            }

            return await this._metadataRepository.GetMetadataForProject(projectPath);
            // if (!metadataTask.IsCompletedSuccessfully)
            // {
            //     return null;
            // }
            // return metadataTask.Result;
        }
    }
}
