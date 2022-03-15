using System.Text;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Avalonia.AXAML.LanguageServer.Document
{
    internal class Buffer
    {
        public StringBuilder Data { get; } = new StringBuilder();
        public DocumentUri Url { get; }
        public int Version { get; }

        public Buffer(OptionalVersionedTextDocumentIdentifier id, string initialString)
        {
            Data.Append(initialString);
            Url = id.Uri;
            Version = (int)id.Version;
        }
    }
}
