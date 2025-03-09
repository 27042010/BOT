using MongoDB.Bson;

namespace Domain.Models
{
    public class SessionPaymentCache
    {
        public string UserId { get; set; }
        public int Quantity { get; set; }
        public long? PaymentId { get; set; }
    }
}
