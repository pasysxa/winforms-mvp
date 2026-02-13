using System;

namespace WinformsMVP.Samples.EmailDemo.Models
{
    /// <summary>
    /// Email message data model
    /// </summary>
    public class EmailMessage : ICloneable
    {
        /// <summary>Email ID</summary>
        public int Id { get; set; }

        /// <summary>Sender email address</summary>
        public string From { get; set; }

        /// <summary>Recipient email address</summary>
        public string To { get; set; }

        /// <summary>Email subject</summary>
        public string Subject { get; set; }

        /// <summary>Email body</summary>
        public string Body { get; set; }

        /// <summary>Received/sent date</summary>
        public DateTime Date { get; set; }

        /// <summary>Whether the email has been read</summary>
        public bool IsRead { get; set; }

        /// <summary>Whether the email is starred</summary>
        public bool IsStarred { get; set; }

        /// <summary>Folder this email belongs to</summary>
        public EmailFolder Folder { get; set; }

        /// <summary>
        /// Create deep copy of email (for ChangeTracker)
        /// </summary>
        public object Clone()
        {
            return new EmailMessage
            {
                Id = this.Id,
                From = this.From,
                To = this.To,
                Subject = this.Subject,
                Body = this.Body,
                Date = this.Date,
                IsRead = this.IsRead,
                IsStarred = this.IsStarred,
                Folder = this.Folder
            };
        }

        /// <summary>
        /// Get email body preview (for list display)
        /// </summary>
        public string GetBodyPreview(int maxLength = 50)
        {
            if (string.IsNullOrWhiteSpace(Body))
                return "(No content)";

            var preview = Body.Replace("\r\n", " ").Replace("\n", " ");
            if (preview.Length > maxLength)
                return preview.Substring(0, maxLength) + "...";

            return preview;
        }

        /// <summary>
        /// Get sender display name (simplified display)
        /// </summary>
        public string GetFromDisplayName()
        {
            if (string.IsNullOrWhiteSpace(From))
                return "Unknown";

            // If contains @, extract username portion
            int atIndex = From.IndexOf('@');
            if (atIndex > 0)
                return From.Substring(0, atIndex);

            return From;
        }
    }
}
