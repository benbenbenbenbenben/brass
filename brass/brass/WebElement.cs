using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using brass;
using PowerAssert;

namespace brass
{
    public class WebElement
    {

        private IWebElement reference;
        private WebElement parent;

        private WebElement(IWebElement elementRoot)
        {
            this.reference = elementRoot;
        }

        private WebElement(IWebElement elementRoot, WebElement parent)
            : this(elementRoot)
        {
            this.parent = parent;
        }

        public string Text
        {
            get { return this.reference.Execute<string>("return this.textContent;"); }
            set { this.reference.Execute($"this.textContent = \"{value}\""); }
        }

        public static WebElement Root(IWebElement rootElement)
        {
            return new WebElement(rootElement);
        }

        public WebElement Find(string query)
        {
            var max = new TimeSpan(0, 0, 0, 30);
            var stopWatch = Stopwatch.StartNew();

            IWebElement element = null;
            while (true)
            {
                var agg = this.Try(
                    () => element = this.reference.FindElement(By.Id(query)),
                    () => element = this.reference.FindElement(By.CssSelector(query)),
                    () => element = (IWebElement) this.reference.Execute($"return this.$({query})"),
                    () => element = this.reference.Execute<IReadOnlyCollection<object>>($"return window.findTextInElement('{query}', this)")
                                    .Cast<IWebElement>()
                                    .Last(e => e.Displayed)
                );

                if (element != null)
                {
                    break;
                }

                if (stopWatch.Elapsed > max)
                {
                    throw new TimeoutException($"WebElement.Find(\"{query}\")", agg);
                }
            }
            return new WebElement(element, this);
        }

        public T Execute<T>(string script, params object[] args)
        {
            return reference.Execute<T>(script, args);
        }

        public void AssertEquals<T>(Func<WebElement, T> actualFunc, T expected)
        {
            var actual = actualFunc(this);
            IsTrue(() => actual.Equals(expected));
        }

        private void IsTrue(Expression<Func<bool>> func)
        {
            var timespan = new TimeSpan(0, 0, 0, 30);
            var stopwatch = Stopwatch.StartNew();
            while (true)
            {
                try
                {
                    PAssert.IsTrue(func);
                    return;
                }
                catch
                {
                    if (stopwatch.Elapsed > timespan)
                    {
                        throw;
                    }
                }
            }
        }

        public WebElement Type(string value)
        {
            // label tag handling (nested and sibling)
            if (this.reference.TagName.ToLower() == "label")
            {
                var parent = this.reference.Execute("return this.parentElement");
                var input = this.reference.FindElements(By.TagName("input"));
                if (input.Any())
                {
                    new WebElement(input.First()).Type(value);
                    return this;
                }
                var inputAlt = this.reference.FindElements(By.TagName("textarea"));
                if (inputAlt.Any())
                {
                    new WebElement(inputAlt.First()).Type(value);
                    return this;
                }
                var forid = this.reference.GetAttribute("for");
                if (string.IsNullOrWhiteSpace(forid) == false)
                {
                    var candidates = this.reference.GetSiblings().Where(sibling => sibling.GetAttribute("id") == forid);
                    if (candidates.Any())
                    {
                        new WebElement(candidates.First()).Type(value);
                        return this;
                    }
                }
                var inputFromParent = this.reference.GetParentElement().FindElements(By.TagName("input"));
                if (inputFromParent.Any())
                {
                    new WebElement(inputFromParent.First()).Type(value);
                    return this;
                }
                var inputAltFromParent = this.reference.GetParentElement().FindElements(By.TagName("textarea"));
                if (inputAltFromParent.Any())
                {
                    new WebElement(inputAltFromParent.First()).Type(value);
                    return this;
                }
            }

            if (this.reference.TagName.ToLower() != "input")
            {
                var input = this.reference.FindElements(By.TagName("input"));
                if (input.Any())
                {
                    new WebElement(input.First()).Type(value);
                    return this;
                }
            }

            var agg = this.Try(
                () => this.reference.SendKeys(value)
            );

            return this;
        }

        public WebElement Click()
        {
            this.Try(
                () => this.reference.Click(),
                () => this.reference.Execute("this.click();")
            );
            return this;
        }

        public WebElement Click(string query)
        {
            return Find(query).Click();
        }

        public WebElement Wait(int i)
        {
            System.Threading.Thread.Sleep(i);
            return this;
        }
    }
}
