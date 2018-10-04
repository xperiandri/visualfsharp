// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.VisualStudio.FSharp.ObjectBrowser

open Microsoft.CodeAnalysis
open Microsoft.VisualStudio.LanguageServices.Implementation.Library.ObjectBrowser

type ListItemFactory () =
    inherit AbstractListItemFactory ()

    let s_memberDisplayFormat =
        SymbolDisplayFormat (
            typeQualificationStyle = SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions = (SymbolDisplayGenericsOptions.IncludeTypeParameters ||| SymbolDisplayGenericsOptions.IncludeVariance),
            memberOptions = (SymbolDisplayMemberOptions.IncludeExplicitInterface ||| SymbolDisplayMemberOptions.IncludeParameters),
            parameterOptions = SymbolDisplayParameterOptions.IncludeType,
            miscellaneousOptions = SymbolDisplayMiscellaneousOptions.UseSpecialTypes)

    let s_memberWithContainingTypeDisplayFormat =
        SymbolDisplayFormat (
            typeQualificationStyle = SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions = (SymbolDisplayGenericsOptions.IncludeTypeParameters ||| SymbolDisplayGenericsOptions.IncludeVariance),
            memberOptions = (SymbolDisplayMemberOptions.IncludeContainingType ||| SymbolDisplayMemberOptions.IncludeExplicitInterface ||| SymbolDisplayMemberOptions.IncludeParameters),
            parameterOptions = SymbolDisplayParameterOptions.IncludeType,
            miscellaneousOptions = SymbolDisplayMiscellaneousOptions.UseSpecialTypes)

    override __.GetMemberDisplayString (memberSymbol : ISymbol) = memberSymbol.ToDisplayString (s_memberDisplayFormat)

    override __.GetMemberAndTypeDisplayString (memberSymbol : ISymbol) = memberSymbol.ToDisplayString (s_memberWithContainingTypeDisplayFormat)
