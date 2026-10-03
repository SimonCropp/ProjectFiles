// Path is the root-level file, or the root-level directory when IsDirectory is true.
record ReservedNameConflict(
    string Path,
    string PropertyName,
    bool IsDirectory);
