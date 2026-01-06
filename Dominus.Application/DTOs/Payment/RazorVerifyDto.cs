namespace Dominus.Application.DTOs.Payment
{
    public class RazorVerifyDto
    {
        public int OrderId { get; set; }
        public string RazorOrderId { get; set; } = null!;
        public string PaymentId { get; set; } = null!;
        public string Signature { get; set; } = null!;
    }
}
