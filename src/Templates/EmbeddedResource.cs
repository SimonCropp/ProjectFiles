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

        if (!stream.CanSeek)
        {
            using var memory = new MemoryStream();
            stream.CopyTo(memory);
            return memory.ToArray();
        }

        // The length is known, so read straight into a single array of the right size
        var bytes = new byte[stream.Length];
        var offset = 0;
        while (offset < bytes.Length)
        {
            var read = stream.Read(bytes, offset, bytes.Length - offset);
            if (read == 0)
            {
                throw new EndOfStreamException($"Embedded resource '{Name}' ended before its reported length.");
            }

            offset += read;
        }

        return bytes;
    }

    public async Task<string> ReadAllTextAsync(CancellationToken cancel = default)
    {
        using var reader = OpenText();
#if NET7_0_OR_GREATER
        return await reader.ReadToEndAsync(cancel).ConfigureAwait(false);
#else
        cancel.ThrowIfCancellationRequested();
        return await reader.ReadToEndAsync().ConfigureAwait(false);
#endif
    }
}
