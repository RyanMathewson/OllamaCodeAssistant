using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;

namespace OllamaCodeAssistant {

  public static class CodeAnalysisManager {

    public static async Task ListClassesAndMethodsAsync(AsyncPackage package) {
      await package.JoinableTaskFactory.SwitchToMainThreadAsync();

      var componentModel = await package.GetServiceAsync(typeof(SComponentModel)) as IComponentModel;
      var workspace = componentModel?.GetService<VisualStudioWorkspace>();
      if (workspace == null) {
        Debug.WriteLine("Workspace not found.");
        return;
      }

      var solution = workspace.CurrentSolution;

      foreach (var project in solution.Projects) {
        foreach (var document in project.Documents) {
          var syntaxRoot = await document.GetSyntaxRootAsync();
          var semanticModel = await document.GetSemanticModelAsync();

          if (syntaxRoot == null || semanticModel == null) continue;

          var classNodes = syntaxRoot.DescendantNodes().OfType<ClassDeclarationSyntax>();

          foreach (var classNode in classNodes) {
            var classSymbol = semanticModel.GetDeclaredSymbol(classNode);
            Debug.WriteLine($"Class: {classSymbol?.ToDisplayString()}");

            var methodNodes = classNode.Members.OfType<MethodDeclarationSyntax>();

            foreach (var methodNode in methodNodes) {
              var methodSymbol = semanticModel.GetDeclaredSymbol(methodNode);
              Debug.WriteLine($"  Method: {methodSymbol?.ToDisplayString()}");

              var methodText = methodNode.ToFullString(); // Full method source code
              Debug.WriteLine($"    Code:\n{methodText}");
            }
          }
        }
      }
    }
  }
}