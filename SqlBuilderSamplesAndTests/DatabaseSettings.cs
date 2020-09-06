using System;
using System.Collections.Generic;
using System.Text;

namespace SqlBuilderSamplesAndTests
{
    public class DatabaseSettings
    {
        public string Provider { get; set; }
        public string Server { get; set; }
        public string Database { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool TrustedConnection { get; set; }
        public string AdditionalSettings { get; set; }
    }
}
