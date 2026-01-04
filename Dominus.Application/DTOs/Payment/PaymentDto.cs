using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominus.Application.DTOs.Payment
{
    public class PaymentDto
    {
        [DefaultValue("URO_PAY_9f8d7s6a5d4")]
        public string PaymentReference { get; set; } = null!;
    }

}
