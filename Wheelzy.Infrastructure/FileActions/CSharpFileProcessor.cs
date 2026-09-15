using System.Text;
using System.Text.RegularExpressions;
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

    // A blank line may contain spaces or tabs, so "\n    \n" also counts.
    private static readonly Regex BlankLinePattern = new(@"\n[ \t]*\n", RegexOptions.Compiled);

    public void ProcessFolder(string rootFolder, FileActions actions = FileActions.All)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootFolder);
        if (!Directory.Exists(rootFolder))
        {
            throw new DirectoryNotFoundException($"Folder not found: {rootFolder}");
        }

        foreach (var filePath in EnumerateCsharpFiles(rootFolder))
        {
            string original;
            Encoding encoding;

            // Keep the file's original encoding (for example UTF-8 with BOM) when writing it back.
            using (var reader = new StreamReader(filePath, detectEncodingFromByteOrderMarks: true))
            {
                original = reader.ReadToEnd();
                encoding = reader.CurrentEncoding;
            }

            var updated = ApplyActions(original, actions);
            if (!string.Equals(original, updated, StringComparison.Ordinal))
            {
                File.WriteAllText(filePath, updated, encoding);
            }
        }
    }

    public string ApplyActions(string source, FileActions actions = FileActions.All)
    {
        ArgumentNullException.ThrowIfNull(source);

        var tree = CSharpSyntaxTree.ParseText(source);
        var root = tree.GetRoot();
        var newLine = DetectNewLine(source);

        // Suffixes run first so "LoadDto" becomes "LoadDTO" and then "LoadDTOAsync".
        if (actions.HasFlag(FileActions.NormalizeDtoVmSuffixes))
        {
            root = NormalizeDtoVmSuffixes(root);
        }

        if (actions.HasFlag(FileActions.RenameAsyncMethods))
        {
            root = RenameAsyncMethodsWithoutSuffix(root);
        }

        if (actions.HasFlag(FileActions.InsertBlankLinesBetweenMethods))
        {
            root = InsertBlankLinesBetweenMethods(root, newLine);
        }

        return root.ToFullString();
    }

    private static string DetectNewLine(string source) =>
        source.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";

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
        !string.Equals(RewriteSuffix(identifier), identifier, StringComparison.Ordinal);

    private static string RewriteSuffix(string identifier)
    {
        // Longest suffixes first so "Dtos" is not treated as "Dto" + "s".
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

    private static SyntaxNode InsertBlankLinesBetweenMethods(SyntaxNode root, string newLine)
    {
        return root.ReplaceNodes(
            root.DescendantNodes().OfType<TypeDeclarationSyntax>(),
            (_, type) => AddBlankLines(type, newLine));
    }

    private static TypeDeclarationSyntax AddBlankLines(TypeDeclarationSyntax type, string newLine)
    {
        var members = type.Members;
        if (members.Count < 2)
        {
            return type;
        }

        for (var i = 0; i < members.Count - 1; i++)
        {
            if (members[i] is not MethodDeclarationSyntax previous
                || members[i + 1] is not MethodDeclarationSyntax next)
            {
                continue;
            }

            if (HasBlankLineBetween(previous, next))
            {
                continue;
            }

            var previousEndsWithNewLine = previous.GetTrailingTrivia()
                .Any(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia));

            // If the previous method already ends its line, one extra line break makes a blank line.
            // If both methods are on the same line, two line breaks are needed.
            var breaksToInsert = previousEndsWithNewLine ? 1 : 2;
            var leading = next.GetLeadingTrivia();
            for (var n = 0; n < breaksToInsert; n++)
            {
                leading = leading.Insert(0, SyntaxFactory.EndOfLine(newLine));
            }

            members = members.Replace(next, next.WithLeadingTrivia(leading));
        }

        return type.WithMembers(members);
    }

    private static bool HasBlankLineBetween(SyntaxNode first, SyntaxNode second)
    {
        var between = (first.GetTrailingTrivia().ToFullString() + second.GetLeadingTrivia().ToFullString())
            .Replace("\r\n", "\n", StringComparison.Ordinal);
        return BlankLinePattern.IsMatch(between);
    }
}
