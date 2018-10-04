// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.VisualStudio.FSharp.ObjectBrowser

open System
open System.Composition
open Microsoft.CodeAnalysis
open Microsoft.VisualStudio.FSharp.Editor
open Microsoft.CodeAnalysis.Host.Mef
open Microsoft.VisualStudio.LanguageServices.Implementation.Library
open Microsoft.VisualStudio.Shell.Interop

[<ExportLanguageService(typeof<ILibraryService>, LanguageNames.FSharp)>]
[<Shared>]
type FSharpLibraryService =
    inherit AbstractLibraryService

    new() =
        let s_typeDisplayFormat =
            SymbolDisplayFormat(
                typeQualificationStyle = SymbolDisplayTypeQualificationStyle.NameAndContainingTypes,
                genericsOptions = (SymbolDisplayGenericsOptions.IncludeTypeParameters ||| SymbolDisplayGenericsOptions.IncludeVariance))

        let s_memberDisplayFormat =
            SymbolDisplayFormat(
                typeQualificationStyle = SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions = (SymbolDisplayGenericsOptions.IncludeTypeParameters ||| SymbolDisplayGenericsOptions.IncludeVariance),
                memberOptions = (SymbolDisplayMemberOptions.IncludeExplicitInterface ||| SymbolDisplayMemberOptions.IncludeParameters),
                parameterOptions = (SymbolDisplayParameterOptions.IncludeType),
                miscellaneousOptions = (SymbolDisplayMiscellaneousOptions.UseSpecialTypes))

        { inherit AbstractLibraryService (Guid FSharpConstants.packageGuidString, __SymbolToolLanguage.SymbolToolLanguage_CSharp, s_typeDisplayFormat, s_memberDisplayFormat) }
