using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using NUnit.Framework;

namespace GoogleTestSearch
{
    [Binding]
    public partial class GoogleSearchFeatureSteps
    {
        IWebDriver driver;
        [Given(@"I navigate to the page ""(.*)""")]
        public void GivenINavigateToThePage(string p0)
        {
            driver = new FirefoxDriver();
            driver.Navigate().GoToUrl("http://www.goolge.com");

        }

        [Given(@"I see the page is loaded")]
        public void GivenISeeThePageIsLoaded()
        {
            Assert.AreEqual("Google", driver.Title);
        }

        [When(@"I enter Search Keyword in the Search Text box")]
        public void WhenIEnterSearchKeywordInTheSearchTextBox(Table table)
        {
            string searchtext = table.Rows[0]["Keyword"].ToString();
            driver.FindElement(By.Name("q")).SendKeys(searchtext);
        }

        [When(@"I click on Search Button")]
        public void WhenIClickOnSearchButton()
        {
            driver.FindElement(By.Name("btnG")).Click();
            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(5));
        }


        [Then(@"Search items shows the items related to SpecFlow")]
        public void ThenSearchItemsShowsTheItemsRelatedToSpecFlow()
        {
            Assert.AreEqual("SpecFlow - Cucumber for .NET", driver.FindElement(By.XPath("//h3/a")).Text);
        }



    }
}
