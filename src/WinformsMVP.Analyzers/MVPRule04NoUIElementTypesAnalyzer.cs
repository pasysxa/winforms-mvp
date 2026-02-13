using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace WinformsMVP.Analyzers
{
    /// <summary>
    /// Analyzer for MVP Rule 4: No UI element types in View interfaces and Presenters.
    /// Detects when View interfaces or Presenters expose WinForms UI types (Button, TextBox, etc.)
    /// in properties, fields, parameters, or return types.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MVPRule04NoUIElementTypesAnalyzer : DiagnosticAnalyzer
    {
        private static readonly string[] UIElementTypes = new[]
        {
            "Button", "TextBox", "Label", "ListBox", "DataGrid", "DataGridView",
            "TreeView", "ListView", "TabControl", "Panel", "GroupBox",
            "ComboBox", "CheckBox", "RadioButton", "PictureBox", "ProgressBar",
            "TrackBar", "NumericUpDown", "DateTimePicker", "MonthCalendar",
            "RichTextBox", "WebBrowser", "SplitContainer", "FlowLayoutPanel",
            "TableLayoutPanel", "ToolStrip", "MenuStrip", "StatusStrip",
            "ContextMenuStrip", "PropertyGrid", "MaskedTextBox", "Form",
            "Control", "UserControl", "ToolStripButton", "ToolStripMenuItem",
            "ToolStripLabel", "ToolStripTextBox", "ToolStripComboBox"
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.MVPRule04NoUIElementTypes);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // Check interface properties and methods
            context.RegisterSyntaxNodeAction(AnalyzeInterfaceProperty, SyntaxKind.PropertyDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeInterfaceMethod, SyntaxKind.MethodDeclaration);

            // Check Presenter fields, properties, and methods
            context.RegisterSyntaxNodeAction(AnalyzePresenterField, SyntaxKind.FieldDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzePresenterProperty, SyntaxKind.PropertyDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzePresenterMethod, SyntaxKind.MethodDeclaration);
        }

        private void AnalyzeInterfaceProperty(SyntaxNodeAnalysisContext context)
        {
            var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;

            // Only analyze in View interfaces
            if (!(propertyDeclaration.Parent is InterfaceDeclarationSyntax interfaceDeclaration))
                return;

            var interfaceName = interfaceDeclaration.Identifier.Text;
            if (!interfaceName.EndsWith("View"))
                return;

            // Check property type
            var typeSymbol = context.SemanticModel.GetTypeInfo(propertyDeclaration.Type).Type;
            if (typeSymbol == null)
                return;

            CheckForUIElementType(context, typeSymbol, propertyDeclaration.Type.GetLocation(),
                "Interface property", interfaceName);
        }

        private void AnalyzeInterfaceMethod(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;

            // Only analyze in View interfaces
            if (!(methodDeclaration.Parent is InterfaceDeclarationSyntax interfaceDeclaration))
                return;

            var interfaceName = interfaceDeclaration.Identifier.Text;
            if (!interfaceName.EndsWith("View"))
                return;

            // Check return type
            var returnTypeSymbol = context.SemanticModel.GetTypeInfo(methodDeclaration.ReturnType).Type;
            if (returnTypeSymbol != null && returnTypeSymbol.SpecialType != SpecialType.System_Void)
            {
                CheckForUIElementType(context, returnTypeSymbol, methodDeclaration.ReturnType.GetLocation(),
                    "Interface method", interfaceName);
            }

            // Check parameters
            foreach (var parameter in methodDeclaration.ParameterList.Parameters)
            {
                var paramTypeSymbol = context.SemanticModel.GetTypeInfo(parameter.Type).Type;
                if (paramTypeSymbol != null)
                {
                    CheckForUIElementType(context, paramTypeSymbol, parameter.Type.GetLocation(),
                        "Interface method parameter", interfaceName);
                }
            }
        }

        private void AnalyzePresenterField(SyntaxNodeAnalysisContext context)
        {
            var fieldDeclaration = (FieldDeclarationSyntax)context.Node;

            // Only analyze in Presenter classes
            if (!IsPresenterClass(fieldDeclaration.Parent))
                return;

            var containingClass = (ClassDeclarationSyntax)fieldDeclaration.Parent;
            var className = containingClass.Identifier.Text;

            var variableDeclaration = fieldDeclaration.Declaration;
            var typeSymbol = context.SemanticModel.GetTypeInfo(variableDeclaration.Type).Type;
            if (typeSymbol == null)
                return;

            CheckForUIElementType(context, typeSymbol, variableDeclaration.Type.GetLocation(),
                "Presenter field", className);
        }

        private void AnalyzePresenterProperty(SyntaxNodeAnalysisContext context)
        {
            var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;

            // Only analyze in Presenter classes
            if (!IsPresenterClass(propertyDeclaration.Parent))
                return;

            var containingClass = (ClassDeclarationSyntax)propertyDeclaration.Parent;
            var className = containingClass.Identifier.Text;

            // Skip the View property (it's supposed to be an interface)
            if (propertyDeclaration.Identifier.Text == "View")
                return;

            var typeSymbol = context.SemanticModel.GetTypeInfo(propertyDeclaration.Type).Type;
            if (typeSymbol == null)
                return;

            CheckForUIElementType(context, typeSymbol, propertyDeclaration.Type.GetLocation(),
                "Presenter property", className);
        }

        private void AnalyzePresenterMethod(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;

            // Only analyze in Presenter classes
            if (!IsPresenterClass(methodDeclaration.Parent))
                return;

            var containingClass = (ClassDeclarationSyntax)methodDeclaration.Parent;
            var className = containingClass.Identifier.Text;

            // Check return type
            var returnTypeSymbol = context.SemanticModel.GetTypeInfo(methodDeclaration.ReturnType).Type;
            if (returnTypeSymbol != null && returnTypeSymbol.SpecialType != SpecialType.System_Void)
            {
                CheckForUIElementType(context, returnTypeSymbol, methodDeclaration.ReturnType.GetLocation(),
                    "Presenter method", className);
            }

            // Check parameters
            foreach (var parameter in methodDeclaration.ParameterList.Parameters)
            {
                var paramTypeSymbol = context.SemanticModel.GetTypeInfo(parameter.Type).Type;
                if (paramTypeSymbol != null)
                {
                    CheckForUIElementType(context, paramTypeSymbol, parameter.Type.GetLocation(),
                        "Presenter method parameter", className);
                }
            }
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

        private void CheckForUIElementType(
            SyntaxNodeAnalysisContext context,
            ITypeSymbol typeSymbol,
            Location location,
            string memberType,
            string containingTypeName)
        {
            var typeName = typeSymbol.Name;

            // Check if it's a UI element type
            if (UIElementTypes.Contains(typeName))
            {
                // Allow System.Windows.Forms.Form and Control only in specific contexts
                // (e.g., when casting View for window operations like Close)
                if ((typeName == "Form" || typeName == "Control") &&
                    typeSymbol.ContainingNamespace?.ToDisplayString() == "System.Windows.Forms")
                {
                    // Allow these only in local variables, not in members
                    return;
                }

                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.MVPRule04NoUIElementTypes,
                    location,
                    memberType,
                    containingTypeName,
                    typeName);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
