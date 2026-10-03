// Two sources (files or directories) that generate the same member name in one scope.
// Existing is null when the name is that of the enclosing type, which cannot be removed.
record MemberNameConflict(
    string Description,
    GeneratedMember? Existing,
    GeneratedMember Added,
    string Name);

// A file or directory, by project-relative path, that a member is generated for.
record GeneratedMember(string Path, bool IsFile);
