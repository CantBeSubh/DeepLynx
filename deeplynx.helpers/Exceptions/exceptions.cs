namespace deeplynx.helpers.exceptions;

public class ProjectDependencyDeletionException : Exception
{
    public ProjectDependencyDeletionException(string message) : base(message) { }
}

public class ClassDependencyDeletionException : Exception
{
    public ClassDependencyDeletionException(string message) : base(message) { }
}
