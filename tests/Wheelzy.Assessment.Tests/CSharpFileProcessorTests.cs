using System.Text;
using Wheelzy.Infrastructure.FileActions;

namespace Wheelzy.Assessment.Tests;

public sealed class CSharpFileProcessorTests
{
    private readonly CSharpFileProcessor _processor = new();

    [Fact]
    public void RenameAsyncMethods_AddsAsyncSuffixWithoutUpdatingReferences()
    {
        const string source = """
            public class Sample
            {
                public async Task Save() { await Task.CompletedTask; }

                public async Task SaveAsync() { await Task.CompletedTask; }

                public void Call() { Save(); }
            }
            """;

        var result = _processor.ApplyActions(source, FileActions.RenameAsyncMethods);

        Assert.Contains("public async Task SaveAsync()", result);
        Assert.Contains("public void Call() { Save(); }", result);
        Assert.DoesNotContain("public async Task Save()", result);
    }

    [Fact]
    public void NormalizeDtoVmSuffixes_RewritesWordsThatEndWithKnownSuffixes()
    {
        const string source = """
            public class CustomerDto
            {
                public List<OrderDto> OrderDtos { get; set; }
                public AccountVm AccountVm { get; set; }
                public List<ItemVm> ItemVms { get; set; }
            }
            """;

        var result = _processor.ApplyActions(source, FileActions.NormalizeDtoVmSuffixes);

        Assert.Contains("public class CustomerDTO", result);
        Assert.Contains("List<OrderDTO> OrderDTOs", result);
        Assert.Contains("AccountVM AccountVM", result);
        Assert.Contains("List<ItemVM> ItemVMs", result);
        Assert.DoesNotContain("CustomerDto", result);
        Assert.DoesNotContain("OrderDtos", result);
        Assert.DoesNotContain("ItemVms", result);
    }

    [Fact]
    public void InsertBlankLines_AddsMissingLineBetweenConsecutiveMethods()
    {
        const string source = """
            public class Sample
            {
                public void First() { }
                public void Second() { }
            }
            """;

        var result = _processor.ApplyActions(source, FileActions.InsertBlankLinesBetweenMethods);
        var normalized = result.Replace("\r\n", "\n");

        Assert.Contains("public void First() { }\n\n    public void Second() { }", normalized);
    }

    [Fact]
    public void InsertBlankLines_LeavesExistingBlankLineAlone()
    {
        const string source = """
            public class Sample
            {
                public void First() { }

                public void Second() { }
            }
            """;

        var result = _processor.ApplyActions(source, FileActions.InsertBlankLinesBetweenMethods);

        Assert.Equal(source.Replace("\r\n", "\n"), result.Replace("\r\n", "\n"));
    }

    [Fact]
    public void ProcessFolder_AppliesAllActionsToNestedCsharpFiles()
    {
        var root = Path.Combine(Path.GetTempPath(), "wheelzy-q5-" + Guid.NewGuid().ToString("N"));
        var nested = Path.Combine(root, "nested");
        Directory.CreateDirectory(nested);

        var target = Path.Combine(nested, "Demo.cs");
        File.WriteAllText(target, """
            public class OrderDto
            {
                public async Task Save() { await Task.CompletedTask; }
                public void Other() { }
            }
            """);

        try
        {
            _processor.ProcessFolder(root, FileActions.All);

            var updated = File.ReadAllText(target).Replace("\r\n", "\n");
            Assert.Contains("public class OrderDTO", updated);
            Assert.Contains("public async Task SaveAsync()", updated);
            Assert.Contains("public async Task SaveAsync() { await Task.CompletedTask; }\n\n    public void Other() { }", updated);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
    [Fact]
    public void NormalizeDtoVmSuffixes_LeavesWordsWhereSuffixIsNotAtTheEnd()
    {
        const string source = """
            public class DtoMapper
            {
                public VmFactory Factory { get; set; }
            }
            """;

        var result = _processor.ApplyActions(source, FileActions.NormalizeDtoVmSuffixes);

        Assert.Equal(source, result);
    }

    [Fact]
    public void AllActions_RenameSuffixBeforeAddingAsync()
    {
        const string source = """
            public class Loader
            {
                public async Task<OrderDto> LoadDto() { return await Task.FromResult(new OrderDto()); }
            }
            """;

        var result = _processor.ApplyActions(source, FileActions.All);

        Assert.Contains("Task<OrderDTO> LoadDTOAsync()", result);
    }

    [Fact]
    public void InsertBlankLines_KeepsWindowsLineEndings()
    {
        var source = "public class Sample\r\n{\r\n    public void First() { }\r\n    public void Second() { }\r\n}\r\n";

        var result = _processor.ApplyActions(source, FileActions.InsertBlankLinesBetweenMethods);

        Assert.Contains("public void First() { }\r\n\r\n    public void Second() { }", result);
        Assert.DoesNotContain("\n", result.Replace("\r\n", string.Empty));
    }

    [Fact]
    public void InsertBlankLines_SplitsMethodsWrittenOnTheSameLine()
    {
        const string source = "public class Sample { public void First() { } public void Second() { } }";

        var result = _processor.ApplyActions(source, FileActions.InsertBlankLinesBetweenMethods);

        Assert.Contains("public void First() { } \n\npublic void Second() { }", result);
    }

    [Fact]
    public void InsertBlankLines_TreatsWhitespaceOnlyLineAsBlank()
    {
        const string source = "public class Sample\n{\n    public void First() { }\n    \n    public void Second() { }\n}";

        var result = _processor.ApplyActions(source, FileActions.InsertBlankLinesBetweenMethods);

        Assert.Equal(source, result);
    }

    [Fact]
    public void ProcessFolder_SkipsBinAndObjFolders()
    {
        var root = Path.Combine(Path.GetTempPath(), "wheelzy-q5-" + Guid.NewGuid().ToString("N"));
        var bin = Path.Combine(root, "bin");
        Directory.CreateDirectory(bin);

        const string content = "public class Skipped { public async Task Save() { await Task.CompletedTask; } }";
        var target = Path.Combine(bin, "Skipped.cs");
        File.WriteAllText(target, content);

        try
        {
            _processor.ProcessFolder(root, FileActions.All);

            Assert.Equal(content, File.ReadAllText(target));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ProcessFolder_KeepsUtf8Bom()
    {
        var root = Path.Combine(Path.GetTempPath(), "wheelzy-q5-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        var target = Path.Combine(root, "Bom.cs");
        File.WriteAllText(target, "public class ItemDto { }", new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

        try
        {
            _processor.ProcessFolder(root, FileActions.All);

            var bytes = File.ReadAllBytes(target);
            Assert.True(bytes.Length > 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF);
            Assert.Contains("ItemDTO", File.ReadAllText(target));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
