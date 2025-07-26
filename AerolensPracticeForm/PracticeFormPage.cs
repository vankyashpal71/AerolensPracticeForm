using Bogus;
using Microsoft.Playwright;
using NUnit.Framework;
using System.Threading.Tasks;
using static Bogus.DataSets.Name;

public class PracticeFormPage
{
    private readonly IPage _page;

    public PracticeFormPage(IPage page)
    {
        _page = page;
    }

    private ILocator FirstNameInput => _page.Locator("#firstName");
    private ILocator LastNameInput => _page.Locator("#lastName");
    private ILocator EmailInput => _page.Locator("#userEmail");

    private ILocator GenderInput(string gender) => _page.Locator($"//label[text()='{gender}']");
    private ILocator PhoneInput => _page.Locator("#userNumber");
    private ILocator DobInput => _page.Locator("#dateOfBirthInput");
    private ILocator SubjectInput => _page.Locator("#subjectsInput");
    private ILocator HobbiesInput(string hobby) => _page.Locator($"//label[text()='{hobby}']");
    private ILocator SubmitButton => _page.Locator("#submit");
    private ILocator CurrentAddressInput => _page.Locator("#currentAddress");
    public ILocator ModalTitle => _page.Locator("#example-modal-sizes-title-lg");
    public ILocator ConfirmationTable => _page.Locator(".table-responsive tbody");
    public ILocator CloseModalButton => _page.Locator("#closeLargeModal");



    public async Task GotoAsync()
    {
        await _page.GotoAsync("https://demoqa.com/automation-practice-form");
        await _page.WaitForSelectorAsync("#firstName");
    }

   
   
    public async Task FillFormAsync(
        string firstName, string lastName, string email, string gender,
        string phone, string dob, string subject, string[] hobbies,
        string filePath, string address, string state, string city)
    {       

        await FirstNameInput.FillAsync(firstName);
        await LastNameInput.FillAsync(lastName);
        await EmailInput.FillAsync(email);
        
        await GenderInput(gender).ClickAsync();

        await PhoneInput.FillAsync(phone);
        
        string[] parts = dob.Split(' ');
        
        await DobInput.ClickAsync();
        await _page.SelectOptionAsync(".react-datepicker__month-select", parts[1]);
        await _page.SelectOptionAsync(".react-datepicker__year-select", parts[2]);
        await _page.ClickAsync($".react-datepicker__day--0{parts[0]}:not(.react-datepicker__day--outside-month)");        
        await RandomDelay(100, 300); // Random delay to avoid unexpected popup issues
        await SubjectInput.ClickAsync();


        await SubjectInput.FillAsync(subject);
            await RandomDelay(100, 300); // Random delay to avoid unexpected popup issues
        await _page.Keyboard.PressAsync("Enter");

            await RandomDelay(500, 1000); //Random delay to avoid unexpected popup issues
        foreach (var hobby in hobbies)
        {
            await HobbiesInput(hobby).ClickAsync();
        }

           
            await _page.SetInputFilesAsync("#uploadPicture", filePath);

        await CurrentAddressInput.FillAsync(address);

        // State and City
        await _page.ClickAsync("#state");
        await _page.ClickAsync($"//div[text()='{state}']");
        await _page.ClickAsync("#city");
        await _page.ClickAsync($"//div[text()='{city}']");
        
    }

    public async Task SubmitAsync()
    {
        await SubmitButton.ClickAsync();
        await _page.WaitForSelectorAsync("#example-modal-sizes-title-lg");
    }

    public async Task<bool> IsSubmissionSuccessfulAsync()
    {
        return await _page.IsVisibleAsync("#example-modal-sizes-title-lg");
    }
    public async Task<string> GetModalTitleAsync()
    {
        return await ModalTitle.InnerTextAsync();
    }
    public async Task<Dictionary<string, string>> GetConfirmationTableValuesAsync()
    {
        var rows = await ConfirmationTable.Locator("tr").AllAsync();
        var values = new Dictionary<string, string>();
        foreach (var row in rows)
        {
            var cells = await row.Locator("td").AllAsync();
            if (cells.Count == 2)
            {
                var key = await cells[0].InnerTextAsync();
                var value = await cells[1].InnerTextAsync();
                values[key.Trim()] = value.Trim();
            }
        }
        return values;
    }
    public async Task CloseModalAsync()
    {
        await CloseModalButton.ClickAsync();
    }
    public async Task PreCleanPageAsync()
    {
        
        
        await _page.EvaluateAsync("document.getElementById('fixedban')?.remove();");
        await _page.EvaluateAsync("window.scrollTo(0, 0);");
        await _page.EvaluateAsync(@"() => {
    const ad = document.querySelector('.Advertisement-Section');
    if (ad) {
        ad.style.display = 'none';
    }
}");

    }
    private async Task RandomDelay(int minMs, int maxMs)
    {
        await Task.Delay(Random.Shared.Next(minMs, maxMs));
    }

}

