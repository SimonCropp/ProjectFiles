namespace ProjectFilesGenerator;

using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

partial class EmbeddedResource(string name)
{
    public string Name { get; } = name;

    static Assembly assembly = typeof(EmbeddedResource).Assembly;

    public override string ToString() => Name;

    public static implicit operator string(EmbeddedResource resource) =>
        resource.Name;

    public Stream OpenRead() =>
        assembly.GetManifestResourceStream(Name) ??
        throw new InvalidOperationException($"Could not find embedded resource '{Name}'.");

    public StreamReader OpenText() =>
        new(OpenRead());

    public StreamReader OpenText(Encoding encoding) =>
        new(OpenRead(), encoding);

    public string ReadAllText()
    {
        using var reader = OpenText();
        return reader.ReadToEnd();
    }

    public string ReadAllText(Encoding encoding)
    {
        using var reader = OpenText(encoding);
        return reader.ReadToEnd();
    }

    public byte[] ReadAllBytes()
    {
        using var stream = OpenRead();
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }

    public async Task<string> ReadAllTextAsync(CancellationToken cancel = default)
    {
        using var reader = OpenText();
#if NET7_0_OR_GREATER
        return await reader.ReadToEndAsync(cancel);
#else
        cancel.ThrowIfCancellationRequested();
        return await reader.ReadToEndAsync();
#endif
    }
}
