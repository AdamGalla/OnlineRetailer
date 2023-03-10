using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedDTOs.Models;

public class OrderStatusChangedMessage
{
    public int? CustomerId { get; set; }
    public IList<OrderLineDto> OrderLines { get; set; }
}
