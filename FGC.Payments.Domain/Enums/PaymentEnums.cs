namespace FGC.Payments.Domain.Enums
{
    public enum PaymentStatus
    {
        Pending = 0,
        Processing = 1,
        Completed = 2,
        Failed = 3,
        Refunded = 4,
        Cancelled = 5
    }

    public enum PaymentMethod
    {
        CreditCard = 0,
        DebitCard = 1,
        Pix = 2,
        BankSlip = 3,
        PayPal = 4,
        ApplePay = 5,
        GooglePay = 6
    }
}