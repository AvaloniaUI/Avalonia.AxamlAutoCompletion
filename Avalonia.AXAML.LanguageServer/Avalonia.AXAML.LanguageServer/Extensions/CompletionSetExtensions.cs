using Avalonia.Ide.CompletionEngine;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Avalonia.AXAML.LanguageServer.Extensions
{
    public static class CompletionSetExtensions
    {
        public static CompletionList ToCompletionList(
            this CompletionSet completionSet, 
            Position position, 
            string buffer)
        {
            var mappedCompletionList = new List<CompletionItem>();

            for (int i = 0; i < completionSet.Completions.Count; i++)
            {
                var completion = MapCompletions(
                    completionSet.Completions[i],
                    completionSet.StartPosition,
                    position,
                    i.ToString().PadLeft(10,'0'),
                    buffer);
                mappedCompletionList.Add(completion);
            }

            return new CompletionList(mappedCompletionList);
        }

        private static CompletionItem MapCompletions(
            Completion completion, 
            int startOffset, 
            Position position, 
            string sortText, 
            string buffer)
        {
            Position startPosition = buffer.AsSpan().OffsetToPosition(startOffset);  // Utils.OffsetToPosition(startOffset, buffer);
            
            TextEdit edit = new TextEdit()
            {
                NewText = completion.InsertText,
                Range = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range(startPosition, position)
            };

            CompletionItem item = new CompletionItem()
            {
                Kind = MapKind(completion.Kind),
                Label = completion.DisplayText,
                Detail = completion.DisplayText,
                Documentation = completion.Description,
                TextEdit = edit,
                InsertTextFormat = completion.RecommendedCursorOffset == null ? InsertTextFormat.PlainText : InsertTextFormat.Snippet,
                SortText = sortText
            };
            
            if (completion.RecommendedCursorOffset != null)
            {
                edit.NewText.Insert(completion.RecommendedCursorOffset.Value, "$0");
                // edit.NewText = edit.NewText.Insert(completion.RecommendedCursorOffset.Value, "$0");
                // item.InsertTextFormat = InsertTextFormat.Snippet;
            }

            return item;
        }

        public static CompletionItemKind MapKind(CompletionKind kind)
        {
            switch(kind)
            {
                case CompletionKind.Class: 
                    return CompletionItemKind.Class;
                case CompletionKind.Property: 
                    return CompletionItemKind.Property;
                case CompletionKind.AttachedProperty: 
                    return CompletionItemKind.Property;
                case CompletionKind.StaticProperty: 
                    return CompletionItemKind.Property;
                case CompletionKind.Namespace: 
                    return CompletionItemKind.Module;
                case CompletionKind.Enum: 
                    return CompletionItemKind.Enum;
                case CompletionKind.MarkupExtension: 
                    return CompletionItemKind.Function;
                case CompletionKind.Event: 
                    return CompletionItemKind.Event;
                case CompletionKind.AttachedEvent: 
                    return CompletionItemKind.Event;
                default:
                    return CompletionItemKind.Text;
            };
        }
    }
}
