using System;
using System.ComponentModel;
using System.Net;
using System.Net.Mail;
using System.Text;
using LazyBones.Config;
using LazyBones.Log.Config;
using LazyBones.Log.Layouts;

namespace LazyBones.Log.Targets
{
    [Target("mail")]
    class MailTarget : TargetWithLayout
    {
        [Required]
        public string Host { get; set; }
        [DefaultValue(25)]
        public int Port { get; set; }
        [DefaultValue(SmtpAuthenticationMode.None)]
        public SmtpAuthenticationMode Mode { get; set; }
        [DefaultValue("{windowsIdentity}")]
        public Layout UserName { get; set; }
        [DefaultValue("")]
        public Layout Password { get; set; }
        [DefaultValue(MailPriority.Normal)]
        public MailPriority Priority { get; set; }
        [DefaultValue(false)]
        public bool EnableSsl { get; set; }
        [Required]
        public Layout From { get; set; }
        [Required]
        public Layout To { get; set; }
        public Layout Cc { get; set; }
        public Layout Bcc { get; set; }
        [DefaultValue("{level}: message from {machinename}")]
        public Layout Subject { get; set; }
        [DefaultValue("utf-8")]
        public Encoding Encoding { get; set; }

        protected override void Write(LogEvent logEvent)
        {
            SendMail(logEvent);
        }
        void SendMail(LogEvent logEvent)
        {
            using (var msg = CreateMessage(logEvent))
            {
                var client = new SmtpClient(Host, Port);
                //从.net4.0开始SmtpClient实现IDisposable，之前的版本没有实现
                using (client as IDisposable)   
                {
                    client.EnableSsl = EnableSsl;
                    SetCredentials(client, logEvent);
                    client.Send(msg);
                }
            }
        }
        MailMessage CreateMessage(LogEvent logEvent)
        {
            var msg = new MailMessage();
            var form = From.GetFormatMessage(logEvent).Split(';');
            if(form.Length == 1)
                msg.From = new MailAddress(form[0]);
            else if(form.Length == 2)
                msg.From = new MailAddress(form[0],form[1]);
            foreach (string mail in To.GetFormatMessage(logEvent).Split(';'))
            {
                msg.To.Add(mail);
            }

            if (this.Bcc != null)
            {
                foreach (string mail in Bcc.GetFormatMessage(logEvent).Split(';'))
                {
                    msg.Bcc.Add(mail);
                }
            }

            if (Cc != null)
            {
                foreach (string mail in Cc.GetFormatMessage(logEvent).Split(';'))
                {
                    msg.CC.Add(mail);
                }
            }

            msg.Subject = this.Subject.GetFormatMessage(logEvent).Trim();
            msg.BodyEncoding = this.Encoding;

            var bodyBuffer = new StringBuilder();
            if (Header != null)
            {
                bodyBuffer.Append(Header.GetFormatMessage(logEvent));
                bodyBuffer.AppendLine();
            }

            bodyBuffer.Append(Body.GetFormatMessage(logEvent));
            if (Footer != null)
            {
                bodyBuffer.AppendLine();
                bodyBuffer.Append(Footer.GetFormatMessage(logEvent));
            }

            msg.Body = bodyBuffer.ToString();
            msg.Priority = Priority;
            return msg;
        }
        void SetCredentials(SmtpClient client, LogEvent logEvent)
        {
            switch (Mode)
            {
                case SmtpAuthenticationMode.Ntlm:
                    client.Credentials = CredentialCache.DefaultNetworkCredentials;
                    break;
                case SmtpAuthenticationMode.Basic:
                    var username = UserName.GetFormatMessage(logEvent);
                    var password = Password.GetFormatMessage(logEvent);
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(username, password);
                    break;
                default:
                    break;
            }
        }

    }
}
