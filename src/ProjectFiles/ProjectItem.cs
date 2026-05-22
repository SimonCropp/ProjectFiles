// A single item the generator emits a property for.
// Path is the project-relative path used for tree placement and property naming.
// For embedded resources, ResourceName carries the MSBuild-computed manifest resource name.
record ProjectItem(string Path, bool IsEmbeddedResource, string? ResourceName);
