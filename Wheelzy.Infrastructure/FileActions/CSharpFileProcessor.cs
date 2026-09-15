using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Wheelzy.Infrastructure.FileActions;

[Flags]
public enum FileActions
{
    None = 0,
    RenameAsyncMethods = 1,
    NormalizeDtoVmSuffixes = 2,
    InsertBlankLinesBetweenMethods = 4,
    All = RenameAsyncMethods | NormalizeDtoVmSuffixes | InsertBlankLinesBetweenMethods
}

public sealed class CSharpFileProcessor
{
    private static readonly HashSet<string> IgnoredDirectoryNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "bin", "obj", ".git", ".vs"
    };

    public void ProcessFolder(string rootFolder, FileActions actions = FileActions.All)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootFolder);
        if (!Directory.Exists(rootFolder))
        {
            throw new DirectoryNotFoundException($"Folder not found: {rootFolder}");
        }

        foreach (var filePath in EnumerateCsharpFiles(rootFolder))
        {
            var original = File.ReadAllText(filePath);
            var updated = ApplyActions(original, actions);
            if (!string.Equals(original, updated, StringComparison.Ordinal))
            {
                File.WriteAllText(filePath, updated);
            }
        }
    }

    public string ApplyActions(string source, FileActions actions = FileActions.All)
    {
        var tree = CSharpSyntaxTree.ParseText(source);
        var root = tree.GetRoot();

        if (actions.HasFlag(FileActions.RenameAsyncMethods))
        {
            root = RenameAsyncMethodsWithoutSuffix(root);
        }

        if (actions.HasFlag(FileActions.NormalizeDtoVmSuffixes))
        {
            root = NormalizeDtoVmSuffixes(root);
        }

        if (actions.HasFlag(FileActions.InsertBlankLinesBetweenMethods))
        {
            root = InsertBlankLinesBetweenMethods(root);
        }

        return root.ToFullString();
    }

    private static IEnumerable<string> EnumerateCsharpFiles(string rootFolder)
    {
        return Directory.EnumerateFiles(rootFolder, "*.cs", SearchOption.AllDirectories)
            .Where(path =>
            {
                var relative = Path.GetRelativePath(rootFolder, path);
                return !relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .Any(segment => IgnoredDirectoryNames.Contains(segment));
            });
    }

    private static SyntaxNode RenameAsyncMethodsWithoutSuffix(SyntaxNode root)
    {
        var methods = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Where(method =>
                method.Modifiers.Any(SyntaxKind.AsyncKeyword) &&
                !method.Identifier.ValueText.EndsWith("Async", StringComparison.Ordinal));

        return root.ReplaceNodes(methods, (_, method) =>
        {
            var newName = method.Identifier.ValueText + "Async";
            return method.WithIdentifier(SyntaxFactory.Identifier(
                method.Identifier.LeadingTrivia,
                newName,
                method.Identifier.TrailingTrivia));
        });
    }

    private static SyntaxNode NormalizeDtoVmSuffixes(SyntaxNode root)
    {
        return root.ReplaceTokens(
            root.DescendantTokens().Where(token =>
                token.IsKind(SyntaxKind.IdentifierToken) && NeedsSuffixRewrite(token.ValueText)),
            (original, _) => SyntaxFactory.Identifier(
                original.LeadingTrivia,
                RewriteSuffix(original.ValueText),
                original.TrailingTrivia));
    }

    private static bool NeedsSuffixRewrite(string identifier) =>
        identifier.EndsWith("Vm", StringComparison.Ordinal)
        || identifier.EndsWith("Vms", StringComparison.Ordinal)
        || identifier.EndsWith("Dto", StringComparison.Ordinal)
        || identifier.EndsWith("Dtos", StringComparison.Ordinal);

    private static string RewriteSuffix(string identifier)
    {
        if (identifier.EndsWith("Dtos", StringComparison.Ordinal))
        {
            return identifier[..^4] + "DTOs";
        }

        if (identifier.EndsWith("Dto", StringComparison.Ordinal))
        {
            return identifier[..^3] + "DTO";
        }

        if (identifier.EndsWith("Vms", StringComparison.Ordinal))
        {
            return identifier[..^3] + "VMs";
        }

        if (identifier.EndsWith("Vm", StringComparison.Ordinal))
        {
            return identifier[..^2] + "VM";
        }

        return identifier;
    }

    private static SyntaxNode InsertBlankLinesBetweenMethods(SyntaxNode root)
    {
        return root.ReplaceNodes(
            root.DescendantNodes().OfType<TypeDeclarationSyntax>(),
            (_, type) => AddBlankLines(type));
    }

    private static TypeDeclarationSyntax AddBlankLines(TypeDeclarationSyntax type)
    {
        var members = type.Members;
        if (members.Count < 2)
        {
            return type;
        }

        for (var i = 0; i < members.Count - 1; i++)
        {
            if (members[i] is not MethodDeclarationSyntax || members[i + 1] is not MethodDeclarationSyntax next)
            {
                continue;
            }

            if (HasBlankLineBetween(members[i], next))
            {
                continue;
            }

            members = members.Replace(next, next.WithLeadingTrivia(EnsureBlankLine(next.GetLeadingTrivia())));
        }

        return type.WithMembers(members);
    }

    private static bool HasBlankLineBetween(SyntaxNode first, SyntaxNode second)
    {
        var between = (first.GetTrailingTrivia().ToFullString() + second.GetLeadingTrivia().ToFullString())
            .Replace("\r\n", "\n", StringComparison.Ordinal);
        return between.Contains("\n\n", StringComparison.Ordinal);
    }

    private static SyntaxTriviaList EnsureBlankLine(SyntaxTriviaList leading)
    {
        var text = leading.ToFullString().Replace("\r\n", "\n");
        if (text.Contains("\n\n", StringComparison.Ordinal))
        {
            return leading;
        }

        if (leading.Count == 0)
        {
            return SyntaxFactory.TriviaList(
                SyntaxFactory.EndOfLine(Environment.NewLine),
                SyntaxFactory.EndOfLine(Environment.NewLine));
        }

        return leading.Insert(0, SyntaxFactory.EndOfLine(Environment.NewLine));
    }
}
