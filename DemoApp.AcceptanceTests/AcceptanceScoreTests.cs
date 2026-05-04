using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace DemoApp.AcceptanceTests;

[TestClass]
public sealed class AcceptanceScoreTests
{
    [TestMethod]
    public async Task ContactPage_StartsApplicationAndReturnsSuccess()
    {
        using var host = AcceptanceTestHost.Create();

        var response = await host.Client.GetAsync("/Contact/Contact");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task ContactForm_RendersExpectedUserFieldsAndTokens()
    {
        using var host = AcceptanceTestHost.Create();

        var html = await GetHtmlAsync(host.Client, "/Contact/Contact");

        AssertContains(html, "<form", "The contact page should contain a form.");
        AssertHasFormField(html, "Name");
        AssertHasFormField(html, "Email");
        AssertHasFormField(html, "Phone");
        AssertHasFormField(html, "Message");
        Assert.IsFalse(string.IsNullOrWhiteSpace(GetAntiForgeryToken(html)), "The form should contain an anti-forgery token.");
        Assert.IsTrue(HasField(html, "SubmissionToken"), "The form should expose a duplicate-prevention submission token.");
    }

    [TestMethod]
    public async Task ContactForm_InvalidSubmission_ReturnsFormOrValidationWithoutAccepting()
    {
        using var host = AcceptanceTestHost.Create();
        var formHtml = await GetHtmlAsync(host.Client, "/Contact/Contact");

        var response = await host.Client.PostAsync("/Contact/Contact", BuildForm(formHtml, new Dictionary<string, string?>
        {
            ["Name"] = "",
            ["Email"] = "not-an-email",
            ["Phone"] = "555-0100",
            ["Message"] = ""
        }));

        var html = await response.Content.ReadAsStringAsync();
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Invalid data should return the form instead of completing the submission.");
        AssertContains(html, "<form", "The response should still show a form.");
        Assert.IsTrue(ContainsAny(html, "validation", "field-validation", "text-danger", "Name", "Email", "Message"));

        var adminHtml = await GetHtmlAsync(host.Client, "/Admin/Index");
        AssertDoesNotContain(adminHtml, "not-an-email", "Invalid contact data should not be visible in the admin list.");
    }

    [TestMethod]
    public async Task ContactForm_ValidSubmission_ReachesThankYouAndAppearsInAdminList()
    {
        using var host = AcceptanceTestHost.Create();
        var unique = Unique();
        var data = ContactData(unique);

        var response = await SubmitContactAsync(host.Client, data);

        AssertRedirectsTo(response, "/ThankYou/Index");
        var thankYouHtml = await FollowRedirectAsync(host.Client, response);
        Assert.IsTrue(ContainsAny(thankYouHtml, "Thank", "submitted", "Send Another Message"));

        var adminHtml = await GetHtmlAsync(host.Client, "/Admin/Index");
        AssertVisibleSubmission(adminHtml, data);
    }

    [TestMethod]
    public async Task ContactForm_DuplicateSubmissionToken_DoesNotCreateDuplicateVisibleRows()
    {
        using var host = AcceptanceTestHost.Create();
        var data = ContactData(Unique());
        var formHtml = await GetHtmlAsync(host.Client, "/Contact/Contact");
        var content = BuildForm(formHtml, data);

        var first = await host.Client.PostAsync("/Contact/Contact", content);
        AssertRedirectsTo(first, "/ThankYou/Index");

        var second = await host.Client.PostAsync("/Contact/Contact", BuildForm(formHtml, data));
        Assert.IsTrue(IsRedirect(second), "A duplicate post should redirect or otherwise complete with duplicate handling.");

        var adminHtml = await GetHtmlAsync(host.Client, "/Admin/Index");
        Assert.AreEqual(1, CountOccurrences(adminHtml, data["Message"]!), "The same duplicate-prevention token should not create two visible submissions.");
    }

    [TestMethod]
    public async Task AdminIndex_ReturnsSuccessAndListLikeContent()
    {
        using var host = AcceptanceTestHost.Create();

        var html = await GetHtmlAsync(host.Client, "/Admin/Index");

        Assert.IsTrue(ContainsAny(html, "Submitted Requests", "<table", "No requests", "Create Request"));
    }

    [TestMethod]
    public async Task AdminCreate_CreatesSubmissionVisibleInAdminList()
    {
        using var host = AcceptanceTestHost.Create();
        var data = ContactData(Unique());
        var createHtml = await GetHtmlAsync(host.Client, "/Admin/Create");

        var response = await host.Client.PostAsync("/Admin/Create", BuildForm(createHtml, data));

        AssertRedirectsTo(response, "/Admin/Index");
        var adminHtml = await GetHtmlAsync(host.Client, "/Admin/Index");
        AssertVisibleSubmission(adminHtml, data);
    }

    [TestMethod]
    public async Task AdminEdit_UpdatesSubmissionThroughPublicEditPage()
    {
        using var host = AcceptanceTestHost.Create();
        var original = await CreateAdminSubmissionAsync(host.Client);
        var indexHtml = await GetHtmlAsync(host.Client, "/Admin/Index");
        var editUrl = FindActionUrlNearText(indexHtml, original["Message"]!, "Edit");
        var editHtml = await GetHtmlAsync(host.Client, editUrl);
        var updated = ContactData(Unique("updated"));
        updated["Id"] = GetInputValue(editHtml, "Id");

        var response = await host.Client.PostAsync(editUrl, BuildForm(editHtml, updated));

        AssertRedirectsTo(response, "/Admin/Index");
        var adminHtml = await GetHtmlAsync(host.Client, "/Admin/Index");
        AssertVisibleSubmission(adminHtml, updated);
        AssertDoesNotContain(adminHtml, original["Message"]!, "The old message should no longer be visible after edit.");
    }

    [TestMethod]
    public async Task AdminDetails_ShowsSubmissionFields()
    {
        using var host = AcceptanceTestHost.Create();
        var data = await CreateAdminSubmissionAsync(host.Client);
        var indexHtml = await GetHtmlAsync(host.Client, "/Admin/Index");
        var detailsUrl = FindActionUrlNearText(indexHtml, data["Message"]!, "Details");

        var detailsHtml = await GetHtmlAsync(host.Client, detailsUrl);

        AssertVisibleSubmission(detailsHtml, data);
    }

    [TestMethod]
    public async Task AdminReply_SavesReplyThroughPublicReplyPage()
    {
        using var host = AcceptanceTestHost.Create();
        var data = await CreateAdminSubmissionAsync(host.Client);
        var indexHtml = await GetHtmlAsync(host.Client, "/Admin/Index");
        var replyUrl = FindActionUrlNearText(indexHtml, data["Message"]!, "Reply");
        var replyHtml = await GetHtmlAsync(host.Client, replyUrl);
        var reply = $"Reply {Unique()}";

        var response = await host.Client.PostAsync(replyUrl, BuildForm(replyHtml, new Dictionary<string, string?>
        {
            ["Id"] = GetInputValue(replyHtml, "Id"),
            ["Name"] = GetInputValue(replyHtml, "Name"),
            ["Email"] = GetInputValue(replyHtml, "Email"),
            ["Message"] = GetInputValue(replyHtml, "Message"),
            ["Reply"] = reply
        }));

        AssertRedirectsTo(response, "/Admin/Index");
        var adminHtml = await GetHtmlAsync(host.Client, "/Admin/Index");
        AssertContains(adminHtml, reply, "The saved reply should be visible from the admin experience.");
    }

    [TestMethod]
    public async Task AdminDelete_RemovesSubmissionThroughPublicDeleteAction()
    {
        using var host = AcceptanceTestHost.Create();
        var data = await CreateAdminSubmissionAsync(host.Client);
        var indexHtml = await GetHtmlAsync(host.Client, "/Admin/Index");
        var deleteForm = FindDeleteFormNearText(indexHtml, data["Message"]!);

        var response = await host.Client.PostAsync(deleteForm.Action, BuildForm(deleteForm.Html, new Dictionary<string, string?>()));

        AssertRedirectsTo(response, "/Admin/Index");
        var adminHtml = await GetHtmlAsync(host.Client, "/Admin/Index");
        AssertDoesNotContain(adminHtml, data["Message"]!, "Deleted submissions should no longer appear in the admin list.");
    }

    private static async Task<Dictionary<string, string?>> CreateAdminSubmissionAsync(HttpClient client)
    {
        var data = ContactData(Unique());
        var createHtml = await GetHtmlAsync(client, "/Admin/Create");
        var response = await client.PostAsync("/Admin/Create", BuildForm(createHtml, data));
        AssertRedirectsTo(response, "/Admin/Index");
        return data;
    }

    private static async Task<HttpResponseMessage> SubmitContactAsync(HttpClient client, Dictionary<string, string?> data)
    {
        var formHtml = await GetHtmlAsync(client, "/Contact/Contact");
        return await client.PostAsync("/Contact/Contact", BuildForm(formHtml, data));
    }

    private static async Task<string> GetHtmlAsync(HttpClient client, string path)
    {
        var response = await client.GetAsync(path);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, $"GET {path} should return success.");
        return await response.Content.ReadAsStringAsync();
    }

    private static async Task<string> FollowRedirectAsync(HttpClient client, HttpResponseMessage response)
    {
        Assert.IsNotNull(response.Headers.Location, "Expected a redirect location.");
        return await GetHtmlAsync(client, response.Headers.Location!.ToString());
    }

    private static FormUrlEncodedContent BuildForm(string html, IReadOnlyDictionary<string, string?> values)
    {
        var fields = ExtractHiddenInputs(html);

        foreach (var value in values)
        {
            fields[value.Key] = value.Value ?? "";
        }

        return new FormUrlEncodedContent(fields);
    }

    private static Dictionary<string, string> ExtractHiddenInputs(string html)
    {
        var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (Match match in Regex.Matches(html, "<input\\b[^>]*>", RegexOptions.IgnoreCase))
        {
            var input = match.Value;
            if (!string.Equals(GetAttribute(input, "type"), "hidden", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var name = GetAttribute(input, "name");
            if (!string.IsNullOrWhiteSpace(name))
            {
                fields[name] = GetAttribute(input, "value") ?? "";
            }
        }

        return fields;
    }

    private static string GetAntiForgeryToken(string html)
    {
        return ExtractHiddenInputs(html).TryGetValue("__RequestVerificationToken", out var token) ? token : "";
    }

    private static string GetInputValue(string html, string name)
    {
        foreach (Match match in Regex.Matches(html, "<input\\b[^>]*>", RegexOptions.IgnoreCase))
        {
            if (string.Equals(GetAttribute(match.Value, "name"), name, StringComparison.OrdinalIgnoreCase))
            {
                return GetAttribute(match.Value, "value") ?? "";
            }
        }

        return "";
    }

    private static bool HasField(string html, string name)
    {
        return Regex.IsMatch(html, $"\\bname=[\"']{Regex.Escape(name)}[\"']", RegexOptions.IgnoreCase);
    }

    private static void AssertHasFormField(string html, string name)
    {
        var hasInput = Regex.IsMatch(html, $"<(input|textarea)\\b[^>]*\\bname=[\"']{Regex.Escape(name)}[\"']", RegexOptions.IgnoreCase);
        var hasLabel = Regex.IsMatch(html, $">\\s*{Regex.Escape(name)}\\s*<", RegexOptions.IgnoreCase);
        Assert.IsTrue(hasInput || hasLabel, $"Expected the form to expose a visible/input field for {name}.");
    }

    private static string? GetAttribute(string tag, string name)
    {
        var match = Regex.Match(tag, $"\\b{Regex.Escape(name)}\\s*=\\s*(?:\"(?<value>[^\"]*)\"|'(?<value>[^']*)')", RegexOptions.IgnoreCase);
        return match.Success ? HttpUtility.HtmlDecode(match.Groups["value"].Value) : null;
    }

    private static string FindActionUrlNearText(string html, string uniqueText, string action)
    {
        var area = FindContainingRowOrWindow(html, uniqueText);
        var match = Regex.Match(area, $"<a\\b[^>]*href=[\"'](?<href>[^\"']*/Admin/{Regex.Escape(action)}/\\d+[^\"']*)[\"'][^>]*>", RegexOptions.IgnoreCase);
        Assert.IsTrue(match.Success, $"Could not find a public {action} link near the created submission.");
        return HttpUtility.HtmlDecode(match.Groups["href"].Value);
    }

    private static (string Action, string Html) FindDeleteFormNearText(string html, string uniqueText)
    {
        var area = FindContainingRowOrWindow(html, uniqueText);
        var match = Regex.Match(area, "<form\\b[^>]*action=[\"'](?<action>[^\"']*/Admin/Delete/\\d+[^\"']*)[\"'][^>]*>.*?</form>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        Assert.IsTrue(match.Success, "Could not find a public delete form near the created submission.");
        return (HttpUtility.HtmlDecode(match.Groups["action"].Value), match.Value);
    }

    private static string FindContainingRowOrWindow(string html, string uniqueText)
    {
        foreach (Match row in Regex.Matches(html, "<tr\\b[^>]*>.*?</tr>", RegexOptions.IgnoreCase | RegexOptions.Singleline))
        {
            if (row.Value.Contains(uniqueText, StringComparison.Ordinal))
            {
                return row.Value;
            }
        }

        var index = html.IndexOf(uniqueText, StringComparison.Ordinal);
        Assert.IsTrue(index >= 0, "Could not find the created submission in the page HTML.");
        var start = Math.Max(0, index - 2_000);
        var length = Math.Min(html.Length - start, 4_000);
        return html.Substring(start, length);
    }

    private static Dictionary<string, string?> ContactData(string unique)
    {
        return new Dictionary<string, string?>
        {
            ["Name"] = $"Score User {unique}",
            ["Email"] = $"score-{unique}@example.com",
            ["Phone"] = "555-0100",
            ["Message"] = $"Score acceptance message {unique}"
        };
    }

    private static string Unique(string prefix = "case")
    {
        return $"{prefix}-{Guid.NewGuid():N}";
    }

    private static bool IsRedirect(HttpResponseMessage response)
    {
        return response.StatusCode is HttpStatusCode.Redirect or HttpStatusCode.RedirectMethod or HttpStatusCode.SeeOther;
    }

    private static void AssertRedirectsTo(HttpResponseMessage response, string expectedPath)
    {
        Assert.IsTrue(IsRedirect(response), $"Expected a redirect but received {(int)response.StatusCode}.");
        Assert.IsNotNull(response.Headers.Location, "Expected a redirect location.");
        Assert.IsTrue(response.Headers.Location!.ToString().Contains(expectedPath, StringComparison.OrdinalIgnoreCase),
            $"Expected redirect to contain '{expectedPath}' but was '{response.Headers.Location}'.");
    }

    private static void AssertVisibleSubmission(string html, IReadOnlyDictionary<string, string?> data)
    {
        AssertContains(html, data["Name"]!, "Submitted name should be visible.");
        AssertContains(html, data["Email"]!, "Submitted email should be visible.");
        AssertContains(html, data["Phone"]!, "Submitted phone should be visible.");
        AssertContains(html, data["Message"]!, "Submitted message should be visible.");
    }

    private static void AssertContains(string html, string expected, string message)
    {
        Assert.IsTrue(html.Contains(expected, StringComparison.OrdinalIgnoreCase), message);
    }

    private static void AssertDoesNotContain(string html, string unexpected, string message)
    {
        Assert.IsFalse(html.Contains(unexpected, StringComparison.OrdinalIgnoreCase), message);
    }

    private static bool ContainsAny(string html, params string[] values)
    {
        return values.Any(value => html.Contains(value, StringComparison.OrdinalIgnoreCase));
    }

    private static int CountOccurrences(string html, string text)
    {
        return Regex.Matches(html, Regex.Escape(text), RegexOptions.IgnoreCase).Count;
    }
}
