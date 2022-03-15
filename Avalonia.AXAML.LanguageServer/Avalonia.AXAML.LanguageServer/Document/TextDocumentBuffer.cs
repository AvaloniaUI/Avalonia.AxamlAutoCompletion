using System.Collections.Concurrent;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Avalonia.AXAML.LanguageServer.Document
{
    public class TextDocumentBuffer
    {
        private ConcurrentDictionary<DocumentUri, Buffer> _buffers = new ConcurrentDictionary<DocumentUri, Buffer>();

        public void CreateBuffer(OptionalVersionedTextDocumentIdentifier id, string data)
        {
            _buffers[id.Uri] = new Buffer(id, data);
        }

        public void UpdateBuffer(OptionalVersionedTextDocumentIdentifier id, int position, string newText, int charactersToRemove = 0)
        {
            if(!_buffers.TryGetValue(id.Uri, out var buffer))
            {
                return;
            }

            if(charactersToRemove > 0)
            {
                buffer.Data.Remove(position, charactersToRemove);
            }

            buffer.Data.Insert(position, newText);
        }

        public string GetBuffer(TextDocumentIdentifier id)
        {
            return _buffers.TryGetValue(id.Uri, out var buffer) ? buffer.Data.ToString() : "";
        }
    }
}
