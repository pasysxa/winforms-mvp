using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace WinformsMVP.Analyzers
{
    /// <summary>
    /// Analyzer for MVP Rule 7: Presenter should access View only through interface.
    /// Detects when Presenters reference concrete Form types instead of View interfaces.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MVPRule07ViewInterfaceOnlyAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.MVPRule07ViewInterfaceOnly);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeFieldDeclaration, SyntaxKind.FieldDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzePropertyDeclaration, SyntaxKind.PropertyDeclaration);
        }

        private void AnalyzeFieldDeclaration(SyntaxNodeAnalysisContext context)
        {
            var fieldDeclaration = (FieldDeclarationSyntax)context.Node;

            // Only analyze in Presenter classes
            if (!IsPresenterClass(fieldDeclaration.Parent))
                return;

            var variableDeclaration = fieldDeclaration.Declaration;
            var typeSyntax = variableDeclaration.Type;

            CheckForFormTypeReference(context, typeSyntax, fieldDeclaration.GetLocation());
        }

        private void AnalyzePropertyDeclaration(SyntaxNodeAnalysisContext context)
        {
            var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;

            // Only analyze in Presenter classes
            if (!IsPresenterClass(propertyDeclaration.Parent))
                return;

            var typeSyntax = propertyDeclaration.Type;

            CheckForFormTypeReference(context, typeSyntax, propertyDeclaration.GetLocation());
        }

        private bool IsPresenterClass(SyntaxNode node)
        {
            if (node is ClassDeclarationSyntax classDeclaration)
            {
                var className = classDeclaration.Identifier.Text;
                return className.EndsWith("Presenter");
            }
            return false;
        }

        private void CheckForFormTypeReference(
            SyntaxNodeAnalysisContext context,
            TypeSyntax typeSyntax,
            Location location)
        {
            var typeSymbol = context.SemanticModel.GetTypeInfo(typeSyntax).Type;
            if (typeSymbol == null)
                return;

            var typeName = typeSymbol.Name;

            // Check if it's a concrete Form type (ends with "Form" but not an interface)
            if (typeName.EndsWith("Form") &&
                typeSymbol.TypeKind != TypeKind.Interface &&
                !IsAcceptableFormReference(typeSymbol))
            {
                var presenterClass = GetContainingPresenterClass(location.SourceTree.GetRoot(), location);
                if (presenterClass != null)
                {
                    var diagnostic = Diagnostic.Create(
                        DiagnosticDescriptors.MVPRule07ViewInterfaceOnly,
                        location,
                        presenterClass,
                        typeName);

                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private bool IsAcceptableFormReference(ITypeSymbol typeSymbol)
        {
            // System.Windows.Forms.Form itself is acceptable (for casting)
            return typeSymbol.ContainingNamespace?.ToDisplayString() == "System.Windows.Forms" &&
                   typeSymbol.Name == "Form";
        }

        private string GetContainingPresenterClass(SyntaxNode root, Location location)
        {
            var node = root.FindNode(location.SourceSpan);
            var classDeclaration = node.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            return classDeclaration?.Identifier.Text;
        }
    }
}
