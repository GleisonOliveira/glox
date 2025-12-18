namespace glox.Core
{
    public abstract class Result<TSuccess, TFail>
    {
        public sealed class Success : Result<TSuccess, TFail>
        {
            public TSuccess Value { get; private init; }
            public Success(TSuccess value) => Value = value;
        }

        public sealed class Failure : Result<TSuccess, TFail>
        {
            public TFail Error { get; private init; }
            public Failure(TFail error) => Error = error;
        }

        public Task<TResult> MatchAsync<TResult>(
            Func<TSuccess, Task<TResult>> onSuccess,
            Func<TFail, Task<TResult>> onFailure)
        {
            return this switch
            {
                Success s => onSuccess(s.Value),
                Failure f => onFailure(f.Error),
                _ => throw new InvalidOperationException("Tipo inesperado em Result")
            };
        }
    }
}