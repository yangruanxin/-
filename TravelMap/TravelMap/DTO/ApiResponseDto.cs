namespace TravelMap.DTO
{
    public class ApiResponseDto<T>
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public static ApiResponseDto<T> Success(T data, string message = "操作成功")
        {
            return new ApiResponseDto<T> { Code = 0, Message = message, Data = data };
        }

        public static ApiResponseDto<T> Error(string message, int code = 1)
        {
            return new ApiResponseDto<T> { Code = code, Message = message, Data = default(T) };
        }
    }
}
