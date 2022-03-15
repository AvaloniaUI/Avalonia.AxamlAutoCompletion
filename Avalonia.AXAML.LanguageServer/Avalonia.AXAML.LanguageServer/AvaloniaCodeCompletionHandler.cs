using Avalonia.AXAML.LanguageServer.AssemblyMetadata;
using Avalonia.AXAML.LanguageServer.Document;
using Avalonia.AXAML.LanguageServer.Extensions;
using Avalonia.Ide.CompletionEngine;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Avalonia.AXAML.LanguageServer
{
    public class AvaloniaCodeCompletionHandler : ICompletionHandler
    {
        private readonly DocumentSelector _documentSelector = new DocumentSelector(
            new DocumentFilter()
            {
                Pattern = "**/*.axaml"
            },
            new DocumentFilter()
            {
                Pattern = "**/*.xaml"
            }
        );
        private readonly ILogger<AvaloniaCodeCompletionHandler> _logger;
        private readonly TextDocumentBuffer _textDocumentBuffer;
        private readonly DocumentMetadataProvider _metadataProvider;

        public AvaloniaCodeCompletionHandler(
            TextDocumentBuffer  textDocumentBuffer,
            DocumentMetadataProvider metadataProvider,
            ILogger<AvaloniaCodeCompletionHandler> logger)
        {
            this._textDocumentBuffer = textDocumentBuffer;
            this._metadataProvider = metadataProvider;
            this._logger = logger;
        }

        public CompletionRegistrationOptions GetRegistrationOptions(
            CompletionCapability capability, 
            ClientCapabilities clientCapabilities)
        {
            this._logger.LogInformation("GetRegistrationOptions method");

            return new CompletionRegistrationOptions
            {
                DocumentSelector = _documentSelector,
                ResolveProvider = false
            };
        }

        public async Task<CompletionList> Handle(
            CompletionParams request, 
            CancellationToken cancellationToken)
        {   
            var documentPath = request.TextDocument.Uri.ToUri().LocalPath;
            
            var buffer = this._textDocumentBuffer.GetBuffer(request.TextDocument.Uri);
            if (buffer == null)
            {
                return new CompletionList();
            }

            var metadata = await this._metadataProvider.GetMetadataForDocument(documentPath);
            if(metadata == null)
            {
                return new CompletionList();
            }
            
            var position =  buffer.AsSpan().PositionToOffset(request.Position);  
            var completionResult = new CompletionEngine()
                .GetCompletions(metadata, buffer.ToString(), position);

            if (completionResult == null)
            {
                return new CompletionList();
            }

            return completionResult.ToCompletionList(request.Position, buffer);
        }
    }
}
