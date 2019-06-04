using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurrieTechnologies.Blazor.Clipboard
{
    public class ClipboardService
    {
        static readonly IDictionary<Guid, TaskCompletionSource<string>> pendingReadRequests = 
            new Dictionary<Guid, TaskCompletionSource<string>>();
        static readonly IDictionary<Guid, TaskCompletionSource<object>> pendingWriteRequests =
            new Dictionary<Guid, TaskCompletionSource<object>>();
        private readonly IJSRuntime jSRuntime;

        public ClipboardService(IJSRuntime jSRuntime)
        {
            this.jSRuntime = jSRuntime;
        }

        /// <summary>
        /// Requests text from the system clipboard.
        /// </summary>
        /// <returns>A Task that resolves with a DOMString containing the textual contents of the clipboard.</returns>
        public async Task<string> ReadTextAsync()
        {
            var tcs = new TaskCompletionSource<string>();
            var requestId = Guid.NewGuid();
            pendingReadRequests.Add(requestId, tcs);
            _ = await jSRuntime.InvokeAsync<string>("CurrieTechnologies.Blazor.Clipboard.ReadText", requestId);

            return await tcs.Task;
        }

        /// <summary>
        /// Writes text to the system clipboard.
        /// </summary>
        /// <param name="newClipText">The string to be written to the clipboard.</param>
        /// <returns>A Task which is resolved once the text is fully copied into the clipboard.</returns>
        public async Task WriteTextAsync(string newClipText)
        {
            var tcs = new TaskCompletionSource<object>();
            var requestId = Guid.NewGuid();
            pendingWriteRequests.Add(requestId, tcs);
            _ = await jSRuntime.InvokeAsync<object>("CurrieTechnologies.Blazor.Clipboard.WriteText", requestId, newClipText);

            await tcs.Task;
            return;
        }

        [JSInvokable]
        public static void ReceiveResponse(string id, string text)
        {
            TaskCompletionSource<string> pendingTask;
            var idVal = Guid.Parse(id);
            pendingTask = pendingReadRequests.First(x => x.Key == idVal).Value;
            pendingTask.SetResult(text);
        }

        [JSInvokable]
        public static void ReceiveWriteResponse(string id)
        {
            TaskCompletionSource<object> pendingTask;
            var idVal = Guid.Parse(id);
            pendingTask = pendingWriteRequests.First(x => x.Key == idVal).Value;
            pendingTask.SetResult(null);
        }
    }
}
