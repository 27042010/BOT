namespace Domain.Models
{
    public class CurrentSmsUserRentService
    {
        public string ChatId { get; set; } = string.Empty;
        public string ActivationId { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public double ValueRent { get; set; }
        public string ActivationOperator { get; set; } = string.Empty;
        public string BalanceHistoryId { get; set; } = string.Empty;
        public int MessageId { get; set; } // USADO PARA ATUALIZAR A MENSSAGEM, REMOVENDO O BOTÃO DE CANCELAR
        public DateTime RequestedAt { get; set; }
    }
}
