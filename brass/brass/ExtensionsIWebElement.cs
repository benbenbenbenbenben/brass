
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using brass.Properties;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace brass
{
    public static class ExtensionsIWebElement
    {
        public static object Execute(this IWebElement element, string script, params object[] arguments)
        {
            var driver = ((RemoteWebElement) element).WrappedDriver;
            var executor = ((IJavaScriptExecutor) driver);

            InstallExtensions(executor);

            var wrapper = new StringBuilder();
            wrapper.Append("return (function(){");
            wrapper.Append(script);
            wrapper.Append("}).apply(arguments[0], arguments[1])");

            var wrappedString = wrapper.ToString();
            return executor.ExecuteScript(wrappedString, element, arguments);
        }

        public static T Execute<T>(this IWebElement element, string script, params object[] arguments)
        {
            var result = element.Execute(script, arguments);
            return (T) result;
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

        public static void InstallExtensions(this IJavaScriptExecutor executor)
        {
            var names = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("brass.extensions.js"))
            {
                using (var reader = new StreamReader(stream))
                {
                    var script = reader.ReadToEnd();
                    executor.ExecuteScript(script);
                }
            }

        }

        public static IWebElement GetParentElement(this IWebElement element)
        {
            return (IWebElement)element.Execute("return this.parentElement;");
        }

        public static IEnumerable<IWebElement> GetChildren(this IWebElement element)
        {
            var children = element.Execute("return Array.from(this.children);");
            if (children == null)
                yield break;
            if (children is IWebElement)
                yield return (IWebElement)children;
            if (children is ReadOnlyCollection<object>)
                foreach (var f in ((ReadOnlyCollection<object>) children).Select(x => (IWebElement) x))
                    yield return f;
            if (children is ReadOnlyCollection<IWebElement>)
                foreach (var f in ((ReadOnlyCollection<IWebElement>)children).Select(x => (IWebElement)x))
                    yield return f;
        }

        public static IEnumerable<IWebElement> GetSiblings(this IWebElement element)
        {
            var siblings = element.GetParentElement().GetChildren().ToList();
            siblings.Remove(element);
            return siblings;
        }
    }
}
