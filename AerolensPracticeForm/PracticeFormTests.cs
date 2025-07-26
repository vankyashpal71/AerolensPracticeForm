using System;
using System.IO;
using System.Threading.Tasks;
using Bogus;
using Microsoft.Playwright;
using NUnit.Framework;



[Parallelizable(ParallelScope.All)]
[TestFixture]
public class PracticeFormTests 
{
    private IPlaywright _playwright;
    private string _browserType;
  


    private PracticeFormData GenerateFormData()
    {
        var faker = new Faker();
        return new PracticeFormData
        {
            FirstName = faker.Name.FirstName(),
            LastName = faker.Name.LastName(),
            Email = faker.Internet.Email(),
            Gender = faker.PickRandom(new[] { "Male", "Female", "Other" }),
            Phone = faker.Random.Replace("##########"),
            Dob = faker.Date.Past(30, DateTime.Today.AddYears(-18)).ToString("dd MMMM yyyy"),
            Subject = "Maths",
            Hobbies = new[] { "Sports", "Reading" },
            FilePath = Path.GetFullPath("test-image.png"),
            Address = faker.Address.FullAddress(),
            State = "NCR",
            City = "Delhi"
        };
    }

    [TestCase("chromium")]
    [TestCase("firefox")]
    public async Task FillPracticeForm_CrossBrowser(string browserType)
    {
        await using var browser = await CreateBrowserAsync(browserType);
        await using var context = await browser.NewContextAsync(
        new BrowserNewContextOptions
        {
            ViewportSize = null // This is done to avoid unexpected failure in firefox
        });

        var data = GenerateFormData();
            
        var page = await context.NewPageAsync();
        var formPage = new PracticeFormPage(page);

        await formPage.GotoAsync();
        await page.WaitForSelectorAsync("#firstName", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
        await formPage.PreCleanPageAsync();

        await formPage.FillFormAsync(data.FirstName, data.LastName, data.Email, data.Gender, data.Phone,
            data.Dob, data.Subject, data.Hobbies, data.FilePath, data.Address, data.State, data.City);

        await formPage.SubmitAsync();

        Assert.That(await formPage.IsSubmissionSuccessfulAsync(), Is.True);

        await context.CloseAsync();
    }

    [TestCase("chromium")]
    [TestCase("firefox")]
    public async Task ValidateSubmissionModal(string browserType)
    {
        await using var browser = await CreateBrowserAsync(browserType);
        await using var context = await browser.NewContextAsync(
        new BrowserNewContextOptions
        {
           ViewportSize = new ViewportSize { Width = 1920, Height = 1080 } // This disables the default 1280x720 viewport and uses the full screen
        });

        var data = GenerateFormData();
                
        var page = await context.NewPageAsync();
        var formPage = new PracticeFormPage(page);

        await formPage.GotoAsync();
        await page.WaitForSelectorAsync("#firstName", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
        await formPage.PreCleanPageAsync();

        await formPage.FillFormAsync(
            data.FirstName, data.LastName, data.Email, data.Gender, data.Phone,
            data.Dob, data.Subject, data.Hobbies, data.FilePath, data.Address, data.State, data.City);
        await formPage.SubmitAsync();

        string modalTitle = await formPage.GetModalTitleAsync();
        Assert.That(modalTitle, Is.EqualTo("Thanks for submitting the form"));

        var tableValues = await formPage.GetConfirmationTableValuesAsync();

        Assert.That(tableValues["Student Name"], Is.EqualTo($"{data.FirstName} {data.LastName}"));
        Assert.That(tableValues["Student Email"], Is.EqualTo(data.Email));
        Assert.That(tableValues["Gender"], Is.EqualTo(data.Gender));
        Assert.That(tableValues["Mobile"], Is.EqualTo(data.Phone));
        Assert.That(tableValues["Date of Birth"].Replace(",", " "), Does.Contain(data.Dob));
        Assert.That(tableValues["Subjects"], Is.EqualTo(data.Subject));
        foreach (var hobby in data.Hobbies)
            Assert.That(tableValues["Hobbies"], Does.Contain(hobby));
        Assert.That(tableValues["Picture"], Is.EqualTo(Path.GetFileName(data.FilePath)));
        Assert.That(tableValues["Address"], Is.EqualTo(data.Address));
        Assert.That(tableValues["State and City"], Is.EqualTo($"{data.State} {data.City}"));

        TestContext.WriteLine("Submitted values:");
        foreach (var kvp in tableValues)
        {
            TestContext.WriteLine($"{kvp.Key}: {kvp.Value}");
        }

        await formPage.CloseModalAsync();
        await context.CloseAsync();
    }
    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        _playwright = await Playwright.CreateAsync();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _playwright?.Dispose();
    }

    private async Task<IBrowser> CreateBrowserAsync(string browserType)
    {
        _browserType = browserType;
        var launchOptions = new BrowserTypeLaunchOptions
        {
            Headless = bool.Parse(_headless ?? "true"),
            // Recommended for parallel execution
            Args = new[] { "--disable-dev-shm-usage","--start-maximized" }
        };

        return browserType.ToLower() switch
        {
            "firefox" => await _playwright.Firefox.LaunchAsync(launchOptions),
            "chromium" => await _playwright.Chromium.LaunchAsync(launchOptions)
        };
    }
}

