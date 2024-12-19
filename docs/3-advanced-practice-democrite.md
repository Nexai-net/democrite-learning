Democrite - Advanced - Practice
___


# StreamQueue

// Add Democrite.Framework.Node.StreamQueue

 // Activate Stream Trigger management and configure the default stream in memory
 .UseStreamQueues()

# Repository instead of registry

// Change from RssFeedUrlSource to simply UrlSource to be reusable
// Remove the regitry but implementent a new grain RssMonitorVGrain

# Unit Test

// Internal visible too
// add Democrite toolkit

// 1 Simple test to validate Uid is well check by the update method
// 2 Test to ensure save and raise are only made when new element occurred
- This test show that current code doesn't respect the requirement due to IReadOnlyCollection that check the collection type instead of the content only.
- Fix by managing by hand the equality

MS bug report ?

# Python artifact

https://stackoverflow.com/questions/328356/extracting-text-from-html-file-using-python

# Search engine