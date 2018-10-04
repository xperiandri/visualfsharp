// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.VisualStudio.FSharp.ObjectBrowser

open System
open Microsoft.CodeAnalysis
open Microsoft.VisualStudio.ComponentModelHost
open Microsoft.VisualStudio.FSharp.Editor
open Microsoft.VisualStudio.LanguageServices.Implementation.Library.ObjectBrowser
open Microsoft.VisualStudio.LanguageServices
open Microsoft.VisualStudio.Shell.Interop

type internal ObjectBrowserLibraryManager (serviceProvider) =
    inherit AbstractObjectBrowserLibraryManager (LanguageNames.FSharp, Guid FSharpConstants.packageGuidString, __SymbolToolLanguage.SymbolToolLanguage_CSharp, serviceProvider)

    override this.CreateDescriptionBuilder (description : IVsObjectBrowserDescription3,
                                            listItem : ObjectListItem,
                                            project: Project) =
        DescriptionBuilder (description, this, listItem, project) :> _

    override __.CreateListItemFactory () = ListItemFactory () :> _
