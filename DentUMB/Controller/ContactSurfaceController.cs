using Clean.Core.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Cms.Web.Website.ActionResults;
using Umbraco.Cms.Core;

namespace DentUMB.Controller
{
    public class ContactSurfaceController : SurfaceController
    {
        private readonly IPublishedContentQuery _contentQuery;
        public ContactSurfaceController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider, IPublishedContentQuery contentQuery) : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _contentQuery = contentQuery;
        }

        [HttpPost]
        public ActionResult SubmitForm(ContactViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Formda doğrulama hatası varsa
               // return CurrentUmbracoPage();
            }

            try
            {

                var homepage = _contentQuery.Content(1121);

                // SMTP ayarlarını ve alıcı e-posta adresini çekelim
                #region  Mail atma
                var smtpHost = homepage.Value<string>("MailSmtpHost");
                var smtpPort = homepage.Value<int>("MailSmtpPort"); // Port genellikle int tipindedir
                var smtpUsername = homepage.Value<string>("MailSmtpUsername");
                var smtpPassword = homepage.Value<string>("MailSmtpPassword");
                var recipientEmail = model.Email;

                if (string.IsNullOrEmpty(recipientEmail))
                {
                    recipientEmail = "default@example.com";
                }

                if (string.IsNullOrEmpty(smtpHost))
                {
                    smtpHost = "smtp.example.com"; // Varsayılan SMTP sunucusu
                }

                if (smtpPort == 0)
                {
                    smtpPort = 587; // Varsayılan port (TLS için 587, SSL için 465)
                }

                if (string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
                {
                    smtpUsername = "your-email@example.com"; // Varsayılan kullanıcı adı
                    smtpPassword = "your-password";          // Varsayılan şifre
                }

                // E-posta gönderim ayarları
                var fromAddress = new MailAddress(smtpUsername,smtpUsername);
                var toAddress = new MailAddress(model.Email, model.Name);
                const string subject = "New Contact Form Submission";
                string body = $"Name: {model.Name}\nEmail: {model.Email}\nMessage: {model.Message}";

                var smtp = new SmtpClient
                {
                    Host = smtpHost, // Umbraco'dan alınan SMTP Host
                    Port = smtpPort, // Umbraco'dan alınan Port
                    EnableSsl = true, // Genellikle SMTP SSL kullanır, gerekirse bu alanı düzenleyin
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, smtpPassword) // SMTP Username ve Password
                };

                try
                {
                    smtp.Send(new MailMessage(fromAddress, toAddress)
                    {
                        Subject = subject,
                        Body = body
                    });
                    Console.WriteLine("Email sent successfully!");
                }
                catch (SmtpFailedRecipientsException ex)
                {
                    foreach (var t in ex.InnerExceptions)
                    {
                        var status = t.StatusCode;
                        Console.WriteLine($"Failed to deliver message to {t.FailedRecipient}: {status}");
                    }
                }

                #endregion


                //// Örnek: E-posta gönderme veya diğer işlemler
                TempData["Success"] = "Form successfully submitted!";
                // return RedirectToCurrentUmbracoPage();
                return Redirect("~/Error");
            }
            catch (Exception ex)
            {
                //Logger.Error<ContactSurfaceController>("Error while submitting the form", ex);
                //TempData["Error"] = "An error occurred while submitting the form.";
                //return CurrentUmbracoPage();
            }
            return Ok();
        }

        //public class EmailService
        //{
        //    public static void SendEmail(ContactViewModel model)
        //    {
        //        // Anasayfa content'ini çekelim (ID'yi Umbraco'dan bulun, örn. 1080)
                  

        //        // Eğer gerekli bilgiler bulunamazsa varsayılan değerlere geçelim
        //        if (string.IsNullOrEmpty(recipientEmail))
        //        {
        //            recipientEmail = "default@example.com";
        //        }

        //        if (string.IsNullOrEmpty(smtpHost))
        //        {
        //            smtpHost = "smtp.example.com"; // Varsayılan SMTP sunucusu
        //        }

        //        if (smtpPort == 0)
        //        {
        //            smtpPort = 587; // Varsayılan port (TLS için 587, SSL için 465)
        //        }

        //        if (string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
        //        {
        //            smtpUsername = "your-email@example.com"; // Varsayılan kullanıcı adı
        //            smtpPassword = "your-password";          // Varsayılan şifre
        //        }

        //        // E-posta gönderim ayarları
        //        var fromAddress = new MailAddress(smtpUsername, "Your Name");
        //        var toAddress = new MailAddress(recipientEmail, "Recipient Name");
        //        const string subject = "New Contact Form Submission";
        //        string body = $"Name: {model.Name}\nEmail: {model.Email}\nMessage: {model.Message}";

        //        var smtp = new SmtpClient
        //        {
        //            Host = smtpHost, // Umbraco'dan alınan SMTP Host
        //            Port = smtpPort, // Umbraco'dan alınan Port
        //            EnableSsl = true, // Genellikle SMTP SSL kullanır, gerekirse bu alanı düzenleyin
        //            DeliveryMethod = SmtpDeliveryMethod.Network,
        //            UseDefaultCredentials = false,
        //            Credentials = new NetworkCredential(fromAddress.Address, smtpPassword) // SMTP Username ve Password
        //        };

        //        using (var message = new MailMessage(fromAddress, toAddress)
        //        {
        //            Subject = subject,
        //            Body = body
        //        })
        //        {
        //            smtp.Send(message);
        //        }
        //    }
        //}
    }
}
