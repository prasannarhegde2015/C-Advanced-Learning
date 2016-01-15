Feature: GoogleSearch
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@mytag
Scenario: Scenario: Verify the search Functionality of Google Search page
Given I navigate to the page "www.google.com"
And I see the page is loaded
When I enter Search Keyword in the Search Text box
| Keyword  |
| SpecFlow |
And I click on Search Button
Then Search items shows the items related to SpecFlow
