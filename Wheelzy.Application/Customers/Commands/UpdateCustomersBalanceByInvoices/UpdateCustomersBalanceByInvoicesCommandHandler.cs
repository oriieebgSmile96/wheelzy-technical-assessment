using MediatR;
using Microsoft.EntityFrameworkCore;
using Wheelzy.Application.Common.Interfaces;

namespace Wheelzy.Application.Customers.Commands.UpdateCustomersBalanceByInvoices;

public class UpdateCustomersBalanceByInvoicesCommandHandler : IRequestHandler<UpdateCustomersBalanceByInvoicesCommand>
{
    private readonly IApplicationDbContext _applicationDbContext;

    public UpdateCustomersBalanceByInvoicesCommandHandler(IApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public async Task Handle(UpdateCustomersBalanceByInvoicesCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request.Invoices);
        if (request.Invoices.Count == 0)
        {
            return;
        }

        var totalsByCustomer = request.Invoices
            .Where(invoice => invoice.CustomerId.HasValue)
            .GroupBy(invoice => invoice.CustomerId!.Value)
            .ToDictionary(group => group.Key, group => group.Sum(invoice => invoice.Total));

        if (totalsByCustomer.Count == 0)
        {
            return;
        }

        var customerIds = totalsByCustomer.Keys.ToList();
        var customers = await _applicationDbContext.Customers
            .Where(customer => customerIds.Contains(customer.Id))
            .ToListAsync(cancellationToken);

        foreach (var customer in customers)
        {
            customer.Balance -= totalsByCustomer[customer.Id];
        }

        await _applicationDbContext.SaveChangesAsync(cancellationToken);
    }
}
