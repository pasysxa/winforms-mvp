namespace WinformsMVP.Common
{
    public class InteractionResult<T>
    {
        private readonly bool _isOk;
        public T Value { get; }
        public bool IsOk => _isOk;
        public bool IsCancelled => !IsOk;

        protected InteractionResult(bool isOk, T value)
        {
            _isOk = isOk;
            Value = value;
        }

        public static InteractionResult<T> Ok<T>(T value)
            => new InteractionResult<T>(true, value);

        public static InteractionResult<T> Cancel<T>()
            => new InteractionResult<T>(false, default(T));

    }
}
