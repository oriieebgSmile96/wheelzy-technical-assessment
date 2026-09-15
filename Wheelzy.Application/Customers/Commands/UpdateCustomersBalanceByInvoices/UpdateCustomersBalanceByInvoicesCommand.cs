using MediatR;
using Wheelzy.Domain.Entities.Models;

namespace Wheelzy.Application.Customers.Commands.UpdateCustomersBalanceByInvoices;

public class UpdateCustomersBalanceByInvoicesCommand : IRequest
{
    public IReadOnlyCollection<Invoice> Invoices { get; set; } = [];
}
