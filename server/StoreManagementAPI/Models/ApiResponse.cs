using System.Collections;

namespace StoreManagementAPI.Models
{
    public class ApiResponse<T>
    {
        public int Code { get; set; } = 200;
        public string Message { get; set; } = "";
        public List<T> Data { get; set; } = new List<T>();

        public ApiResponse(int code, string message, List<T> data)
        {
            Code = code;
            Message = message;
            Data = data;
        }
    }
}
