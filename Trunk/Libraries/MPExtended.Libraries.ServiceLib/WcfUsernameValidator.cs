﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Text;

namespace MPExtended.Libraries.ServiceLib
{
    public class WcfUsernameValidator : UserNamePasswordValidator
    {
        public static String UserName { get; set; }
        public static String Password { get; set; }

        public static void Init()
        {
            string username;
            string password;
            Configuration.GetCredentials(out username, out password, false);
            UserName = username;
            Password = password;
        }

        public override void Validate(string userName, string password)
        {
            // This isn't secure, though
            if ((WcfUsernameValidator.UserName != null && WcfUsernameValidator.Password != null) &&
                (userName != WcfUsernameValidator.UserName || password != WcfUsernameValidator.Password))
            {
                SecurityTokenException ex = new SecurityTokenException("Validation Failed!");
                //needs ms fix -> http://blogs.msdn.com/b/drnick/archive/2010/02/02/fix-to-allow-customizing-the-status-code-when-validation-fails.aspx
                ex.Data["HttpStatusCode"] = HttpStatusCode.Unauthorized;
                throw ex;
            }
        }
    }
}
