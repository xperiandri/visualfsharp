// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.VisualStudio.FSharp.ObjectBrowser

open System;
open System.ComponentModel.Composition;
open Microsoft.CodeAnalysis.Editor;
open Microsoft.CodeAnalysis.Editor.Shared.Utilities;
open Microsoft.CodeAnalysis.Host.Mef;
open Microsoft.VisualStudio.LanguageServices.Implementation.Library.ClassView;
open Microsoft.VisualStudio.Shell;
open Microsoft.VisualStudio.Utilities;
open Microsoft.VisualStudio.FSharp.Editor

[<Export(contractType = typeof<ICommandHandler>)>]
[<ContentType(FSharpConstants.FSharpLanguageLongName)>]
[<Name(PredefinedCommandHandlerNames.ClassView)>]
type FSharpSyncClassViewCommandHandler
    [<ImportingConstructor>]
    //[<Obsolete(MefConstruction.ImportingConstructorMessage, error = true)>]
    (serviceProvider : SVsServiceProvider) =
    inherit AbstractSyncClassViewCommandHandler(serviceProvider)
