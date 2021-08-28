using System.Net.Http;

namespace Navi_Server_Test.Helper
{
    public class ResponseEntity<T>
    {
        public T Body { get; set; }
        public int StatusCode { get; set; }
        public HttpResponseMessage RawMessage { get; set; }
    }
}