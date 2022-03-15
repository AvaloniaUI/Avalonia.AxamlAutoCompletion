using Avalonia.AXAML.LanguageServer.Document;
using Avalonia.AXAML.LanguageServer.Extensions;
using Avalonia.Ide.CompletionEngine;
using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace Avalonia.AXAML.LanguageServer.Handlers
{
    public class TextDocumentHandler : ITextDocumentSyncHandler
    {
        private readonly TextDocumentBuffer _textDocumentBuffer;

        private readonly DocumentSelector _documentSelector = new DocumentSelector(
            new DocumentFilter()
            {
                Pattern = "**/*.xaml"
            },
            new DocumentFilter()
            {
                Pattern = "**/*.axaml"
            }
        );

        public TextDocumentHandler(TextDocumentBuffer textDocumentBuffer)
        {
            this._textDocumentBuffer = textDocumentBuffer;
        }

        public TextDocumentChangeRegistrationOptions GetRegistrationOptions(
            SynchronizationCapability capability, 
            ClientCapabilities clientCapabilities)
        {
            return new TextDocumentChangeRegistrationOptions()
            {
                DocumentSelector = this._documentSelector,
                SyncKind = TextDocumentSyncKind.Incremental
            };
        }

        public TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri) => new TextDocumentAttributes(uri, "xml");

        public async Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
        {
            var text = request.ContentChanges.FirstOrDefault()?.Text;

            var buffer = this._textDocumentBuffer.GetBuffer(request.TextDocument);
            int offset = 0;
            int characterToRemove = 0;
            foreach (var change in request.ContentChanges)
            {
                offset = buffer.AsSpan().PositionToOffset(change.Range.Start); 
                characterToRemove = 0;
                if (change.Range.Start != change.Range.End)
                {
                    characterToRemove = buffer.AsSpan().PositionToOffset(change.Range.End) - offset;
                }

                this._textDocumentBuffer.UpdateBuffer(request.TextDocument, offset, text, characterToRemove);
            }
            if (request.ContentChanges.Count() == 1)
            {
                var bufferWithContentChange = this._textDocumentBuffer.GetBuffer(request.TextDocument);
                this.ApplyTextManipulations(request, text, buffer, bufferWithContentChange, offset, characterToRemove);
            }
            return Unit.Value;
        }

        public Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
        {
            this._textDocumentBuffer.CreateBuffer(new OptionalVersionedTextDocumentIdentifier()
            {
                Uri = request.TextDocument.Uri,
                Version = request.TextDocument.Version
            }, request.TextDocument.Text);
            return Unit.Task;
        }

        public Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
        {
            return Unit.Task;
        }

        public Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
        {
            return Unit.Task;
        }

        TextDocumentOpenRegistrationOptions IRegistration<TextDocumentOpenRegistrationOptions, SynchronizationCapability>.GetRegistrationOptions(SynchronizationCapability capability, ClientCapabilities clientCapabilities)
        {
            return new TextDocumentOpenRegistrationOptions()
            {
                DocumentSelector = this._documentSelector
            };
        }

        TextDocumentCloseRegistrationOptions IRegistration<TextDocumentCloseRegistrationOptions, SynchronizationCapability>.GetRegistrationOptions(SynchronizationCapability capability, ClientCapabilities clientCapabilities)
        {
            return new TextDocumentCloseRegistrationOptions()
            {
                DocumentSelector = this._documentSelector
            };
        }

        TextDocumentSaveRegistrationOptions IRegistration<TextDocumentSaveRegistrationOptions, SynchronizationCapability>.GetRegistrationOptions(SynchronizationCapability capability, ClientCapabilities clientCapabilities)
        {
            return new TextDocumentSaveRegistrationOptions()
            {
                DocumentSelector = this._documentSelector,
                IncludeText = true
            };
        }

        private void ApplyTextManipulations(DidChangeTextDocumentParams request, string text, string buffer, string changedBuffer, int position, int deletedCharacters)
        {
            var textManipulator = new TextManipulator(changedBuffer, position);
            var manipulations = textManipulator.ManipulateText(new TextChangeAdapter(position, text, buffer.Substring(position, deletedCharacters)));

            if(manipulations.Count == 0)
            {
                return;
            }

            var edits = manipulations.Select(n =>
            {
                var start = changedBuffer.AsSpan().OffsetToPosition(n.Start);
                
                switch (n.Type)
                {
                    case ManipulationType.Insert:
                        return new TextEdit()
                        {
                            NewText = n.Text,
                            Range = new Range(start, start)
                        };
                    case ManipulationType.Delete:
                        var end = changedBuffer.AsSpan().OffsetToPosition(n.End);
                        return new TextEdit()
                        {
                            NewText = "",
                            Range = new Range(start, end)
                        };
                    default:
                        throw new NotSupportedException();
                }
            }).ToList();
            if (edits.Count > 0)
            {
                // await _router.ApplyWorkspaceEdit(new ApplyWorkspaceEditParams()
                // {
                //     Edit = new WorkspaceEdit()
                //     {
                //         DocumentChanges = new Container<WorkspaceEditDocumentChange>(new WorkspaceEditDocumentChange(new TextDocumentEdit()
                //         {
                //             TextDocument = request.TextDocument,
                //             Edits = new TextEditContainer(edits)
                //         }))
                //     }
                // });
            }
        }
    }
}
