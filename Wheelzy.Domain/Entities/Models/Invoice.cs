namespace Wheelzy.Domain.Entities.Models;

public class Invoice
{
    public int InvoiceId { get; set; }
    public int? CustomerId { get; set; }
    public decimal Total { get; set; }
}
