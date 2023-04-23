namespace Inferno.Web.Models
{
    public class StandardJsonResult
    {
        public StandardJsonResult()
        {
        }

        internal StandardJsonResult(bool succeeded, string error)
        {
            Succeeded = succeeded;
            Errors = new List<object> { error };
        }

        internal StandardJsonResult(bool succeeded, ICollection<object> errors)
        {
            Succeeded = succeeded;
            Errors = errors;
        }

        internal StandardJsonResult(bool succeeded, ICollection<string> errors)
        {
            Succeeded = succeeded;
            Errors = errors.ToList<object>();
        }

        public bool Succeeded { get; set; }

        public ICollection<object> Errors { get; set; }

        public static StandardJsonResult Success() => new(true, new List<object> { });

        public static StandardJsonResult Failure(ICollection<object> errors) => new(false, errors);

        public static StandardJsonResult Failure(ICollection<string> errors) => new(false, errors);

        public static StandardJsonResult Failure(string error) => new(false, error);
    }

    public class StandardJsonResult<T>
    {
        internal StandardJsonResult(bool succeeded, string error)
        {
            Succeeded = succeeded;
            Errors = new List<object> { error };
        }

        internal StandardJsonResult(bool succeeded, ICollection<object> errors)
        {
            Succeeded = succeeded;
            Errors = errors;
        }

        internal StandardJsonResult(bool succeeded, T data, ICollection<object> errors)
        {
            Succeeded = succeeded;
            Errors = errors;
            Data = data;
        }

        public bool Succeeded { get; set; }

        public T Data { get; set; }

        public ICollection<object> Errors { get; set; }

        public static StandardJsonResult<T> Success(T data) => new(true, data, new List<object> { });

        public static StandardJsonResult<T> Failure(string error) => new(false, error);

        public static StandardJsonResult<T> Failure(ICollection<object> errors) => new(false, errors);
    }
}