
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace brass
{
    public static class ExtensionsIWebElement
    {
        public static object Execute(this IWebElement element, string script)
        {
            var driver = ((RemoteWebElement)element).WrappedDriver;
            var executor = ((IJavaScriptExecutor)driver);

            var wrapper = new StringBuilder();
            wrapper.Append("(function(){");
            wrapper.Append(script);
            wrapper.Append("}).bind(arguments[0])(Array.splice(arguments, 1))");

            var wrappedString = wrapper.ToString();
            return executor.ExecuteScript(wrappedString);
        }

        public static AggregateException Try(this object anything, params Action[] actions)
        {
            var exceptions = new List<Exception>();
            foreach (var action in actions)
            {
                try
                {
                    action();
                    break;
                }
                catch (Exception x)
                {
                    exceptions.Add(x);
                }
            }
            if (exceptions.Any())
            {
                return new AggregateException(exceptions);
            }
            else
            {
                return null;
            }
        }
    }
}
