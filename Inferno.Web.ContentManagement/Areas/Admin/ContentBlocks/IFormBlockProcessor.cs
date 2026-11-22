using System.Net.Mail;
using Microsoft.AspNetCore.Http;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks;

/// <summary>Adjusts the mail message built from a form-block submission before it is sent.</summary>
public interface IFormBlockProcessor
{
    /// <summary>Changes <paramref name="mailMessage"/> from the posted fields.</summary>
    /// <param name="formCollection">The posted fields and files. Reserved keys are still present.</param>
    /// <param name="mailMessage">The message that will be sent.</param>
    /// <remarks>
    /// The submission service clears the recipients after every processor runs and sets the address sealed in the form token.
    /// A processor that throws is logged, and the email is still sent.
    /// </remarks>
    void Process(FormCollection formCollection, MailMessage mailMessage);
}