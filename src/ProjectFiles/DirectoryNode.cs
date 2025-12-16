class DirectoryNode
{
    public required string Path { get; init; }
    public required int Depth { get; init; }
    public Dictionary<string, DirectoryNode> Directories { get; } = [];
    public List<string> Files { get; } = [];
}