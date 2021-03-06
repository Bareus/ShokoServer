﻿namespace Shoko.Server
{
    public class NamingLanguage
    {
        public string Language { get; set; }

        public string LanguageDescription
        {
            get { return Languages.GetLanguageDescription(Language.Trim().ToUpper()); }
        }

        public NamingLanguage()
        {
        }

        public NamingLanguage(string language)
        {
            this.Language = language;
        }

        public override string ToString()
        {
            return string.Format("{0} - ({1})", Language, LanguageDescription);
        }
    }
}