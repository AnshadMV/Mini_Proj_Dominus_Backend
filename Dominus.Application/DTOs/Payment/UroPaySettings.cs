using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominus.Application.DTOs.Payment
{
    public class UroPaySettings
    {
        public string ApiKey { get; set; }
        public string Secret { get; set; }
        public string VPA { get; set; }
        public string VPAName { get; set; }
    }
}
