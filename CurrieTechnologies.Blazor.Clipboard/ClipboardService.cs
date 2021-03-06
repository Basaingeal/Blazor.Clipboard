﻿using Microsoft.JSInterop;
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
        /// <returns>
        /// A Task that resolves with a string containing the textual contents of the clipboard. A JSException is thrown if the caller does not have permission to write to the clipboard.
        /// </returns>
        public async Task<string> ReadTextAsync()
        {
            var tcs = new TaskCompletionSource<string>();
            var requestId = Guid.NewGuid();
            pendingReadRequests.Add(requestId, tcs);
            string invokeResponse = await jSRuntime.InvokeAsync<string>("CurrieTechnologies.Blazor.Clipboard.ReadText", requestId);

            if (invokeResponse.Length > 0)
            {
                pendingReadRequests.Remove(requestId);
                throw new JSException(invokeResponse);
            }

            return await tcs.Task;
        }

        /// <summary>
        /// Writes text to the system clipboard.
        /// </summary>
        /// <param name="newClipText">The string to be written to the clipboard.</param>
        /// <returns>
        /// A Task which is resolved once the text is fully copied into the clipboard. Returns an empty string if the clipboard is empty, does not contain text, or does not include a textual representation among the DataTransfer objects representing the clipboard's contents.
        /// </returns>
        public async Task WriteTextAsync(string newClipText)
        {
            var tcs = new TaskCompletionSource<object>();
            var requestId = Guid.NewGuid();
            pendingWriteRequests.Add(requestId, tcs);
            string invokeResponse = await jSRuntime.InvokeAsync<string>("CurrieTechnologies.Blazor.Clipboard.WriteText", requestId, newClipText);

            if (invokeResponse.Length > 0)
            {
                pendingWriteRequests.Remove(requestId);
                throw new JSException(invokeResponse);
            }

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
            pendingReadRequests.Remove(idVal);
        }

        [JSInvokable]
        public static void ReceiveWriteResponse(string id)
        {
            TaskCompletionSource<object> pendingTask;
            var idVal = Guid.Parse(id);
            pendingTask = pendingWriteRequests.First(x => x.Key == idVal).Value;
            pendingTask.SetResult(null);
            pendingWriteRequests.Remove(idVal);
        }
    }
}
