using Eticaret.Core.Entities;
using System.Net;
using System.Net.Mail;

namespace Eticaret.WebUI.Utils;

public class MailHelper
{
    public static async Task<bool> SendMailAsync(Contact contact)
    {
        SmtpClient smtpClient = new SmtpClient("mail.siteadi.com", 587);
        smtpClient.Credentials = new NetworkCredential("info@siteadi.com", "mailşifresi");
        smtpClient.EnableSsl = false;

        // E-posta içeriği
        MailMessage message = new MailMessage();
        message.From = new MailAddress("info@siteadi.com");
        message.To.Add("destek@siteadi.com"); // ya da contact.Email gibi
        message.Subject = "İletişim Formu Mesajı";
        message.Body = $@"
        Ad: {contact.Name}
        Soyad: {contact.Surname}
        E-posta: {contact.Email}
        Telefon: {contact.Phone}

         Mesaj:
        {contact.Message}
         ";
        message.IsBodyHtml = true;
        

        try
        {
         await smtpClient.SendMailAsync(message);
         smtpClient.Dispose();
         return true;

        }
        catch
        {
            return false;

        }

    }
    public static async Task<bool> SendMailAsync(string email,string subject , string mailBody)
    {
        SmtpClient smtpClient = new SmtpClient("mail.siteadi.com", 587);
        smtpClient.Credentials = new NetworkCredential("info@siteadi.com", "mailşifresi");
        smtpClient.EnableSsl = false;

        // E-posta içeriği
        MailMessage message = new MailMessage();
        message.From = new MailAddress("info@siteadi.com");
        message.To.Add(email); // ya da contact.Email gibi
        message.Subject = subject;
        message.Body = mailBody;
        message.IsBodyHtml = true;
        

        try
        {
         await smtpClient.SendMailAsync(message);
         smtpClient.Dispose();
         return true;

        }
        catch
        {
            return false;

        }

    }

}

