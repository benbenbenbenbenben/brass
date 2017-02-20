using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace brass
{
    public class WebElement
    {
        const string find = @"window.find = function(pattern, parent){ return [(parent.outerHTML.indexOf(pattern) > 0) ? parent : null].concat(Array.from(parent.children).map(function(n){return find(pattern, n)} ))  }";
        const string flatter = @"window.flatten = arr => arr.reduce((acc, val) => acc.concat(Array.isArray(val) ? flatten(val) : val), []);";
        

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
        public static WebElement Root(IWebElement rootElement)
        {
            return new WebElement(rootElement);
        }

        public WebElement Find(string query)
        {
            IWebElement element = null;
            var agg = this.Try(
                () => element = this.reference.FindElement(By.CssSelector(query)),
                () => element = (IWebElement)this.reference.Execute($"return this.$({query})")
            );
            if (element == null)
            {
                throw agg;
            }
            return new WebElement(element, this);

        }

        public WebElement Click()
        {
            this.Try(
                () => this.reference.Click(),
                () => this.reference.Execute("this.click();")
            );
            return this;
        }
    }
}
