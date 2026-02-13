using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace WinformsMVP.Analyzers
{
    /// <summary>
    /// Analyzer for MVP Rule 3: Presenter should not create UI controls.
    /// Detects when Presenters instantiate WinForms control types.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MVPRule03NoUIInPresenterAnalyzer : DiagnosticAnalyzer
    {
        private static readonly string[] UIControlTypes = new[]
        {
            "Button", "TextBox", "Label", "ListBox", "DataGrid", "DataGridView",
            "TreeView", "ListView", "TabControl", "Panel", "GroupBox",
            "ComboBox", "CheckBox", "RadioButton", "PictureBox", "ProgressBar",
            "TrackBar", "NumericUpDown", "DateTimePicker", "MonthCalendar",
            "RichTextBox", "WebBrowser", "SplitContainer", "FlowLayoutPanel",
            "TableLayoutPanel", "ToolStrip", "MenuStrip", "StatusStrip",
            "ContextMenuStrip", "PropertyGrid", "MaskedTextBox", "Form"
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.MVPRule03NoUIInPresenter);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression);
        }

        private void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
        {
            var objectCreation = (ObjectCreationExpressionSyntax)context.Node;

            // Only analyze in Presenter classes
            var containingClass = objectCreation.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if (containingClass == null)
                return;

            var className = containingClass.Identifier.Text;
            if (!className.EndsWith("Presenter"))
                return;

            // Get the type being created
            var typeInfo = context.SemanticModel.GetTypeInfo(objectCreation.Type);
            if (typeInfo.Type == null)
                return;

            var typeName = typeInfo.Type.Name;

            // Check if it's a UI control type
            if (UIControlTypes.Contains(typeName))
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.MVPRule03NoUIInPresenter,
                    objectCreation.Type.GetLocation(),
                    className,
                    typeName);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
