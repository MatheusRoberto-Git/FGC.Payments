namespace FGC.Payments.Presentation.Models.Requests
{
    public class CreatePaymentRequest
    {
        public Guid UserId { get; set; }
        public Guid GameId { get; set; }
        public decimal Amount { get; set; }
        public int PaymentMethod { get; set; }
    }

    public class ProcessPaymentRequest
    {
        public Guid PaymentId { get; set; }
    }
}