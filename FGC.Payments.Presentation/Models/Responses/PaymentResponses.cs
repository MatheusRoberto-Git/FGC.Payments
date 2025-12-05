namespace FGC.Payments.Presentation.Models.Responses
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public DateTime Timestamp { get; set; }

        public ApiResponse()
        {
            Message = string.Empty;
            Timestamp = DateTime.UtcNow;
        }

        public static ApiResponse<T> SuccessResult(T data, string message = "Operação realizada com sucesso")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow
            };
        }

        public static ApiResponse<T> ErrorResult(string message, T data = default)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    public class PaymentResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid GameId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string FailureReason { get; set; }
    }

    public class PaymentStatusResponse
    {
        public Guid PaymentId { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? ProcessedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string FailureReason { get; set; }
    }
}