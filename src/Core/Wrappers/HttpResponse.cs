namespace Core.Wrappers
{
    public class HttpResponse<T>
    {
        public bool Succeeded { get; set; }
        public T? Result { get; set; }
        public int ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
    }
}