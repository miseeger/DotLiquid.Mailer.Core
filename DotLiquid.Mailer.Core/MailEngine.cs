﻿using System;
using System.IO;
using System.Net;
using System.Net.Mail;

namespace DotLiquid.Mailer.Core
{

    public sealed class MailEngine : IMailEngine
    {

        private static IMailEngine _instance;
        private static readonly object Padlock = new object();
        private readonly SmtpClient _smtpMailer;

        public string SmtpServer
        {
            get => _smtpMailer.Host;
            set => _smtpMailer.Host = value;
        }

        public int SmtpPort {
            get => _smtpMailer.Port;
            set => _smtpMailer.Port = value;
        }

        public bool UseDefaultCredentials
        {
            get => _smtpMailer.UseDefaultCredentials;
            set => _smtpMailer.UseDefaultCredentials = value;
        }

        public ICredentialsByHost Credentials
        {
            get => _smtpMailer.Credentials;
            set => _smtpMailer.Credentials = value;
        }

        public bool EnableSsl
        {
            get => _smtpMailer.EnableSsl;
            set => _smtpMailer.EnableSsl = value;
        }

        public string TemplateDir { get; set; }
        public string DefaultFromAddress { get; set; }
        public bool IsHtml { get; set; }
        public bool IsHigh { get; set; }

        public MailEngine()
        {
            _smtpMailer = new SmtpClient();
        }


        public void RegisterTag<T>(string tagName) where T : Tag, new()
        {
            Template.RegisterTag<T>(tagName);
        }


        public void RegisterFilter(Type filter)
        {
            Template.RegisterFilter(filter);
        }


        public bool SendFromFile<T>(string subject, string templateFile, T data, string to, 
            string from = "")
        {
            templateFile = templateFile.Contains(@"\") 
                                ? templateFile 
                                : $@"{TemplateDir}\{templateFile}";

            if (File.Exists(templateFile))
            {
                var liquidTemplate = File.ReadAllText(templateFile);
                Send<T>(subject, liquidTemplate, data, to, from);
            }
            else
            {
                throw new FileNotFoundException($"Templatefile {templateFile} not found.");
            }

            return true;
        }


        public bool Send<T>(string subject, string liquidTemplate, T data, string to, 
            string from = "")
        {
            LiquidFunctions.RegisterViewModel(typeof(T));
            var template = Template.Parse(liquidTemplate);

            var body = template.RenderViewModel(data);

            var mail = new MailMessage
            {
                From = new MailAddress(from == string.Empty ? DefaultFromAddress : from),
                Subject = subject,
                Body = body,
                IsBodyHtml = IsHtml,
                Priority = IsHigh ? MailPriority.High : MailPriority.Normal
            };
            mail.To.Add(to);

            _smtpMailer.Send(mail);

            if (IsHigh)
            {
                IsHigh = false;
            }

            return true;
        }


        public static IMailEngine Instance
        {
            get
            {
                lock (Padlock)
                {
                    return _instance ?? (_instance = new MailEngine());
                }
            }
        }

    }

}
