
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserValidationThroughLink
{
    public interface IEmailService
    {
        Task SendEmailAsync(Mailrequest mailrequest);
    }
}
