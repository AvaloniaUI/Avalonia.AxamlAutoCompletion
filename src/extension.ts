import path = require('path');
import * as vscode from 'vscode';
import { workspace } from 'vscode';

import {
    LanguageClient,
    ServerOptions,
	LanguageClientOptions,
    TransportKind,
} from "vscode-languageclient/node";

let client: LanguageClient;

export function activate(context: vscode.ExtensionContext) {

	const extensionPath = vscode.extensions.getExtension("AvaloniaUI.avalonia-axamlautocompletion")?.extensionPath as string;
	const libsFolder = path.join(extensionPath, "libs");
	const dllPath = path.join(libsFolder, "Avalonia.AXAML.LanguageServer.dll");

	let serverOptions: ServerOptions = {
		run: {
			command: "dotnet",
			args: [dllPath],
			transport: TransportKind.pipe
		},
		debug: {
			command: "dotnet",
			args: [dllPath],
			transport: TransportKind.pipe,
			runtime: ""
		},
		options: { detached: true }
	};

	let clientOptions: LanguageClientOptions = {
		documentSelector: [
			{
				pattern: "**/*.xaml",
			},
			{
				pattern: "**/*.axaml",
			},
			{
				pattern: "**/*.csproj",
			},
		],
		  synchronize: {
			// Notify the server about file changes to '.clientrc files contained in the workspace
			fileEvents: workspace.createFileSystemWatcher('**/.axaml')
		  }
	};

	client = new LanguageClient(
		"AvaloniaAXAML",
		"Avalonia aXAML Language Server",
		serverOptions,
		clientOptions
	);

	let disposable = client.start();
	context.subscriptions.push(disposable);
}


export function deactivate() : Thenable<void> | undefined
{
	if (!client) {
		return undefined;
	}

	return client.stop();
}
