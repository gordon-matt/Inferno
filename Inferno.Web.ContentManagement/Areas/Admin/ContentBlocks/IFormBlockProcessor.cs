using System.Net.Mail;
using Microsoft.AspNetCore.Http;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks
{
    public interface IFormBlockProcessor
    {
        void Process(FormCollection formCollection, MailMessage mailMessage);
    }
}