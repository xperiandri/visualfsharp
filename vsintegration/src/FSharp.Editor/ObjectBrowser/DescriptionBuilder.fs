// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.VisualStudio.FSharp.ObjectBrowser

open System.Collections.Immutable
open System.Diagnostics
open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.Shared.Extensions
open Microsoft.VisualStudio.LanguageServices.Implementation.Library.ObjectBrowser
open Microsoft.VisualStudio.Shell.Interop

type LinkFlags = Microsoft.VisualStudio.LanguageServices.Implementation.Library.ObjectBrowser.AbstractDescriptionBuilder.LinkFlags

type DescriptionBuilder(description, libraryManager, listItem, project) =
    inherit AbstractDescriptionBuilder(description, libraryManager, listItem, project)

    override this.BuildNamespaceDeclaration(namespaceSymbol : INamespaceSymbol, options : _VSOBJDESCOPTIONS) =
        this.AddText "namespace "
        this.AddName (namespaceSymbol.ToDisplayString ())
        ()

    override this.BuildDelegateDeclaration(typeSymbol : INamedTypeSymbol, options : _VSOBJDESCOPTIONS) =
        Debug.Assert(typeSymbol.TypeKind = TypeKind.Delegate)

        this.BuildTypeModifiers typeSymbol
        this.AddText "delegate "

        let delegateInvokeMethod = typeSymbol.DelegateInvokeMethod

        this.AddTypeLink (delegateInvokeMethod.ReturnType, AbstractDescriptionBuilder.LinkFlags.None)
        this.AddText " "

        let typeQualificationStyle =
            if int(options &&& _VSOBJDESCOPTIONS.ODO_USEFULLNAME) <> 0
            then SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces
            else SymbolDisplayTypeQualificationStyle.NameOnly

        let typeNameFormat =
            SymbolDisplayFormat (typeQualificationStyle = typeQualificationStyle,
                                 genericsOptions = (SymbolDisplayGenericsOptions.IncludeTypeParameters
                                                ||| SymbolDisplayGenericsOptions.IncludeVariance))

        this.AddName (typeSymbol.ToDisplayString(typeNameFormat))

        this.AddText "("
        this.BuildParameterList delegateInvokeMethod.Parameters
        this.AddText ")"

        if typeSymbol.IsGenericType then this.BuildGenericConstraints(typeSymbol)

        ()

    override this.BuildTypeDeclaration (typeSymbol : INamedTypeSymbol , options : _VSOBJDESCOPTIONS) =
        this.BuildTypeModifiers typeSymbol

        match typeSymbol.TypeKind with
        | TypeKind.Enum -> this.AddText "enum "
        | TypeKind.Struct -> this.AddText "struct "
        | TypeKind.Interface -> this.AddText "interface "
        | TypeKind.Class -> this.AddText "class "
        | _ -> Debug.Fail("Invalid type kind encountered: " + typeSymbol.TypeKind.ToString())

        let typeNameFormat =
            SymbolDisplayFormat (genericsOptions = (SymbolDisplayGenericsOptions.IncludeTypeParameters
                                                ||| SymbolDisplayGenericsOptions.IncludeVariance))

        this.AddName (typeSymbol.ToDisplayString(typeNameFormat))

        if typeSymbol.TypeKind = TypeKind.Enum then
            let underlyingType = typeSymbol.EnumUnderlyingType
            if underlyingType <> null then
                if underlyingType.SpecialType <> SpecialType.System_Int32 then
                    this.AddText " : "
                    this.AddTypeLink (underlyingType, LinkFlags.None)
        else
            let baseType = typeSymbol.BaseType
            if baseType <> null then
                if baseType.SpecialType <> SpecialType.System_Object
                && baseType.SpecialType <> SpecialType.System_Delegate
                && baseType.SpecialType <> SpecialType.System_MulticastDelegate
                && baseType.SpecialType <> SpecialType.System_Enum
                && baseType.SpecialType <> SpecialType.System_ValueType then
                    this.AddText " : "
                    this.AddTypeLink (baseType, LinkFlags.None)

        if typeSymbol.IsGenericType then
            this.BuildGenericConstraints typeSymbol

        ()

    member private this.BuildAccessibility (symbol: ISymbol) =
        match symbol.DeclaredAccessibility with
        | Accessibility.Public -> this.AddText "public "
        | Accessibility.Private -> this.AddText "private "
        | Accessibility.Protected -> this.AddText "protected "
        | Accessibility.Internal -> this.AddText "internal "
        | Accessibility.ProtectedOrInternal -> this.AddText "protected internal "
        | Accessibility.ProtectedAndInternal -> this.AddText "private protected "
        | _ -> this.AddText "internal "
        ()

    member private this.BuildTypeModifiers (typeSymbol : INamedTypeSymbol) =
        this.BuildAccessibility typeSymbol

        if typeSymbol.IsStatic then this.AddText "static "

        if typeSymbol.IsAbstract
        && typeSymbol.TypeKind <> TypeKind.Interface
        then this.AddText "abstract "

        if typeSymbol.IsSealed
        && typeSymbol.TypeKind <> TypeKind.Struct
        && typeSymbol.TypeKind <> TypeKind.Enum
        && typeSymbol.TypeKind <> TypeKind.Delegate then this.AddText "sealed "

        ()

    override this.BuildMethodDeclaration (methodSymbol : IMethodSymbol, options : _VSOBJDESCOPTIONS) =
        this.BuildMemberModifiers methodSymbol

        if methodSymbol.MethodKind <> MethodKind.Constructor
        && methodSymbol.MethodKind <> MethodKind.Destructor
        && methodSymbol.MethodKind <> MethodKind.StaticConstructor
        && methodSymbol.MethodKind <> MethodKind.Conversion
        then
            this.AddTypeLink (methodSymbol.ReturnType, LinkFlags.None)
            this.AddText " "

        if methodSymbol.MethodKind = MethodKind.Conversion then
            match methodSymbol.Name with
            | WellKnownMemberNames.ImplicitConversionName -> this.AddName "implicit operator "
            | WellKnownMemberNames.ExplicitConversionName -> this.AddName "explicit operator "

            this.AddTypeLink (methodSymbol.ReturnType, LinkFlags.None)
        else
            let methodNameFormat =
                SymbolDisplayFormat (genericsOptions = (SymbolDisplayGenericsOptions.IncludeTypeParameters
                                                    ||| SymbolDisplayGenericsOptions.IncludeVariance))

            this.AddName (methodSymbol.ToDisplayString(methodNameFormat))

        this.AddText "("

        if methodSymbol.IsExtensionMethod then this.AddText "this "

        this.BuildParameterList methodSymbol.Parameters
        this.AddText ")"

        if methodSymbol.IsGenericMethod then this.BuildGenericConstraints methodSymbol

        ()

    member private this.BuildMemberModifiers (memberSymbol : ISymbol) =
        if memberSymbol.ContainingType <> null
        && memberSymbol.ContainingType.TypeKind = TypeKind.Interface
        then ()

        let methodSymbol = memberSymbol :?> IMethodSymbol
        let fieldSymbol = memberSymbol :?> IFieldSymbol

        if methodSymbol <> null
        && methodSymbol.MethodKind = MethodKind.Destructor
        then ()

        if fieldSymbol <> null
        && fieldSymbol.ContainingType.TypeKind = TypeKind.Enum
        then ()

        // TODO: 'new' modifier isn't exposed on symbols. Do we need it?

        // Note: we don't display the access modifier for static constructors
        if (isNull methodSymbol)
        || methodSymbol.MethodKind <> MethodKind.StaticConstructor
        then this.BuildAccessibility memberSymbol

        // TODO: check if possible
        // IsUnsafe is internal and cannot be used
        //if memberSymbol.IsUnsafe() then this.AddText "unsafe "

        // Note: we don't display 'static' for constant fields
        if memberSymbol.IsStatic
        && ((isNull fieldSymbol) || (not fieldSymbol.IsConst))
        then this.AddText "static "

        if memberSymbol.IsExtern then this.AddText "extern "

        if fieldSymbol <> null then
            if fieldSymbol.IsReadOnly then this.AddText "readonly "
            if fieldSymbol.IsConst then this.AddText "const "
            if fieldSymbol.IsVolatile then this.AddText "volatile "

        if memberSymbol.IsAbstract then this.AddText "abstract "
        elif memberSymbol.IsOverride then
            if memberSymbol.IsSealed then this.AddText "sealed "
            this.AddText "override "
        elif memberSymbol.IsVirtual then this.AddText "virtual "

        ()

    member private this.BuildGenericConstraints (typeSymbol : INamedTypeSymbol) =
        for typeParameterSymbol in typeSymbol.TypeParameters do
            this.BuildConstraints typeParameterSymbol
        ()

    member private this.BuildGenericConstraints(methodSymbol : IMethodSymbol) =
        for typeParameterSymbol in methodSymbol.TypeParameters do
            this.BuildConstraints typeParameterSymbol
        ()

    member private this.BuildConstraints (typeParameterSymbol : ITypeParameterSymbol) =
        if typeParameterSymbol.ConstraintTypes.Length = 0
        && (not typeParameterSymbol.HasConstructorConstraint)
        && (not typeParameterSymbol.HasReferenceTypeConstraint)
        && (not typeParameterSymbol.HasValueTypeConstraint)
        then ()

        this.AddLineBreak ()
        this.AddText "\t"
        this.AddText "where "
        this.AddName typeParameterSymbol.Name
        this.AddText " : "

        let mutable isFirst = true

        if typeParameterSymbol.HasReferenceTypeConstraint then
            if not(isFirst) then this.AddComma ()

            this.AddText("class")
            isFirst <- false

        if typeParameterSymbol.HasValueTypeConstraint then
            if not(isFirst) then this.AddComma ()

            this.AddText "struct"
            isFirst <- false

        for  constraintType in typeParameterSymbol.ConstraintTypes do
            if not(isFirst) then this.AddComma ()

            this.AddTypeLink (constraintType, LinkFlags.None)
            isFirst <- false

        if typeParameterSymbol.HasConstructorConstraint then
            if not(isFirst) then this.AddComma ()

            this.AddText "new()"
            isFirst <- false

    member private this.BuildParameterList (parameters : ImmutableArray<IParameterSymbol>) =
        let count = parameters.Length
        if count = 0 then ()

        for i = 0 to count do
            if i > 0 then this.AddComma ()

            let current = parameters.[i]
            if current.IsOptional then this.AddText "["
            if current.RefKind = RefKind.Ref then this.AddText "ref "
            elif current.RefKind = RefKind.Out then this.AddText "out "

            if current.IsParams then this.AddText "params "

            this.AddTypeLink (current.Type, LinkFlags.None)
            this.AddText " "
            this.AddParam current.Name

            if current.HasExplicitDefaultValue then
                this.AddText " = "
                match current.ExplicitDefaultValue with
                | null -> this.AddText "null"
                | _ -> this.AddText (current.ExplicitDefaultValue.ToString())

            if current.IsOptional then this.AddText "]"

        ()

    override this.BuildFieldDeclaration (fieldSymbol : IFieldSymbol, options : _VSOBJDESCOPTIONS) =
        this.BuildMemberModifiers fieldSymbol

        if fieldSymbol.ContainingType.TypeKind <> TypeKind.Enum then
            this.AddTypeLink (fieldSymbol.Type, LinkFlags.None)
            this.AddText " "

        this.AddName fieldSymbol.Name

        ()

    override this.BuildPropertyDeclaration (propertySymbol : IPropertySymbol, options : _VSOBJDESCOPTIONS) =
        this.BuildMemberModifiers propertySymbol

        this.AddTypeLink (propertySymbol.Type, LinkFlags.None)
        this.AddText " "

        if propertySymbol.IsIndexer then
            this.AddName "this"
            this.AddText "["
            this.BuildParameterList propertySymbol.Parameters
            this.AddText "]"
        else
            this.AddName propertySymbol.Name

        this.AddText " { "

        if propertySymbol.GetMethod <> null then
            if propertySymbol.GetMethod.DeclaredAccessibility <> propertySymbol.DeclaredAccessibility then
                this.BuildAccessibility propertySymbol.GetMethod
            this.AddText "get; "

        if propertySymbol.SetMethod <> null then
            if propertySymbol.SetMethod.DeclaredAccessibility <> propertySymbol.DeclaredAccessibility then
                this.BuildAccessibility propertySymbol.SetMethod
            this.AddText "set; "

        this.AddText "}"

        ()

    override this.BuildEventDeclaration (eventSymbol : IEventSymbol, options : _VSOBJDESCOPTIONS) =
        this.BuildMemberModifiers eventSymbol

        this.AddText "event "

        this.AddTypeLink (eventSymbol.Type, LinkFlags.None)
        this.AddText " "

        this.AddName eventSymbol.Name
