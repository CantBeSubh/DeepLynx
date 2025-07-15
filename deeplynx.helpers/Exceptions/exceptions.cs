namespace deeplynx.helpers.exceptions;

public class DependencyDeletionException : Exception
{
    public DependencyDeletionException(string message) : base(message) { }
}

public class NoResultsException : Exception
{
    public NoResultsException(string message) : base(message) { }
}

public class InvalidRequestException : Exception
{
    public InvalidRequestException(string message) : base(message) { }
}
