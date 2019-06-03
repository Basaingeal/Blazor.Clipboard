declare var DotNet: any;

let assemblyName: string = 'CurrieTechnologies.Blazor.Clipboard';
let namespace: string = 'CurrieTechnologies.Blazor.Clipboard';

async function dispatchResponse(id: string, text: string) : Promise<void> {
  await DotNet.invokeMethodAsync(namespace, 'ReceiveResponse', id, text);
}

async function dispatchWriteResponse(id: string) : Promise<void> {
  await DotNet.invokeMethodAsync(namespace, 'ReceiveWriteResponse', id);
}

window['CurrieTechnologies'] = window['CurrieTechnologies'] || {};
window['CurrieTechnologies']['Blazor'] = window['CurrieTechnologies']['Blazor'] || {};
window['CurrieTechnologies']['Blazor']['Clipboard'] = window['CurrieTechnologies']['Blazor']['Clipboard'] || {};

window['CurrieTechnologies']['Blazor']['Clipboard']['ReadText'] = async (requestId: string): Promise<string> => {
  if (window.navigator.clipboard !== undefined) {
    const text: string = await window.navigator.clipboard.readText();
    await dispatchResponse(requestId, text);
  } else {
    return 'No clipboard support';
  }

  return '';
};

window['CurrieTechnologies']['Blazor']['Clipboard']['WriteText'] = async (requestId: string, textToWrite: string): Promise<string> => {
  if (window.navigator.clipboard !== undefined) {
    await window.navigator.clipboard.writeText(textToWrite);
    await dispatchWriteResponse(requestId);
  } else {
    return 'No clipboard support';
  }

  return '';
};
