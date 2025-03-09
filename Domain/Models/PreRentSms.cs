namespace Domain.Models
{
    public class PreRentSms
    {
        public string ServiceCode { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public double ValueRent { get; set; }
        public string RentalCost { get; set; }
    }
}
