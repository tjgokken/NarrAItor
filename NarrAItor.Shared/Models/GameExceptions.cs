namespace NarrAItor.Shared.Models;

public class GameGenerationException : Exception
{
    public GameGenerationException(string message) : base(message)
    {
    }

    public GameGenerationException(string message, Exception? inner) : base(message, inner)
    {
    }
}

public class GameGenerationTimeoutException : GameGenerationException
{
    public GameGenerationTimeoutException(string message) : base(message)
    {
    }

    public GameGenerationTimeoutException(string message, Exception? inner) : base(message, inner)
    {
    }
}