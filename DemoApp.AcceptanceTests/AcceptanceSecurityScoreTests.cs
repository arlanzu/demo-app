using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace DemoApp.AcceptanceTests;

// Additional outside-in acceptance/security score tests. These tests use only
// public routes, HTTP responses, rendered HTML, forms, and user-visible effects.
[TestClass]
public sealed class AcceptanceSecurityScoreTests
{
    [TestMethod]
    public async Task DefaultRoute_ReturnsOrRedirectsToContactForm()
    {
        using var host = AcceptanceTestHost.Create();

        var response = await host.Client.GetAsync("/");
        var html = IsRedirect(response)
            ? await FollowRedirectAsync(host.Client, response, "/Contact/Contact")
            : await ReadSuccessHtmlAsync(response, "GET / should return the contact experience or redirect to it.");

        AssertHasFormField(html, "Name");
        AssertHasFormField(html, "Email");
        AssertHasFormField(html, "Message");
    }

    [TestMethod]
    public async Task ContactPost_WithoutAntiForgeryToken_IsRejectedOrDoesNotCreateSubmission()
    {
        using var host = AcceptanceTestHost.Create();
        var marker = Unique("missing-contact-token");
        var formHtml = await GetHtmlAsync(host.Client, "/Contact/Contact");

        var response = await host.Client.PostAsync("/Contact/Contact", BuildForm(formHtml, ContactData(marker), includeAntiForgery: false));

        AssertRejectedOrDoesNotPersist(response);
        var adminHtml = await GetHtmlAsync(host.Client, "/Admin/Index");
        AssertDoesNotContain(adminHtml, marker, "Contact post without anti-forgery token should not create a submission.");
    }

    [TestMethod]
    public async Task AdminCreatePost_WithoutAntiForgeryToken_IsRejectedOrDoesNotCreateSubmission()
    {
        using var host = AcceptanceTestHost.Create();
        var marker = Unique("missing-create-token");
        var createHtml = await GetHtmlAsync(host.Client, "/Admin/Create");

        var response = await host.Client.PostAsync("/Admin/Create", BuildForm(createHtml, ContactData(marker), includeAntiForgery: false));

        AssertRejectedOrDoesNotPersist(response);
        var adminHtml = await GetHtmlAsync(host.Client, "/Admin/Index");
        AssertDoesNotContain(adminHtml, marker, "Admin create without anti-forgery token should not create a submission.");
    }

    [TestMethod]
    public async Task AdminEditPost_WithoutAntiForgeryToken_DoesNotUpdateSubmission()
    {
        using var host = AcceptanceTestHost.Create();
        var original = await CreateAdminSubmissionAsync(host.Client);
        var indexHtml = await GetHtmlAsync(host.Client, "/Admin/Index");
        var editUrl = FindActionUrlNearText(indexHtml, original["Message"]!, "Edit");
        var editHtml = await GetHtmlAsync(host.Client, editUrl);
        var updateMarker = Unique("missing-edit-token");
        var updated = ContactData(updateMarker);
        updated["Id"] = GetInputValue(editHtml, "Id");

        var response = await host.Client.PostAsync(editUrl, BuildForm(editHtml, updated, includeAntiForgery: false));

        AssertRejectedOrDoesNotPersist(response);
        var adminHtml = await GetHtmlAsync(host.Client, "/Admin/Index");
        AssertVisibleSubmission(adminHtml, original);
        AssertDoesNotContain(adminHtml, updateMarker, "Admin edit without anti-forgery token should not update stored values.");
    }

    [TestMethod]
    public async Task AdminReplyPost_WithoutAntiForgeryToken_DoesNotSaveReply()
    {
        using var host = AcceptanceTestHost.Create();
        var data = await CreateAdminSubmissionAsync(host.Client);
        var indexHtml = await GetHtmlAsync(host.Client, "/Admin/Index");
        var replyUrl = FindActionUrlNearText(indexHtml, data["Message"]!, "Reply");
        var replyHtml = await GetHtmlAsync(host.Client, replyUrl);
        var reply = $"Unsafe reply {Unique("missing-reply-token")}";

        var response = await host.Client.PostAsync(replyUrl, BuildForm(replyHtml, new Dictionary<string, string?>
        {
            ["Id"] = GetInputValue(replyHtml, "Id"),
            ["Name"] = GetInputValue(replyHtml, "Name"),
            ["Email"] = GetInputValue(replyHtml, "Email"),
            ["Message"] = GetInputValue(replyHtml, "Message"),
            ["Reply"] = reply
        }, includeAntiForgery: false));

        AssertRejectedOrDoesNotPersist(response);
        var adminHtml = await GetHtmlAsync(host.Client, "/Admin/Index");
        AssertDoesNotContain(adminHtml, reply, "Admin reply without anti-forgery token should not be saved.");
    }

    [TestMethod]
    public async Task AdminDeletePost_WithoutAntiForgeryToken_DoesNotDeleteSubmission()
    {
        using var host = AcceptanceTestHost.Create();
        var data = await CreateAdminSubmissionAsync(host.Client);
        var indexHtml = await GetHtmlAsync(host.Client, "/Admin/Index");
        var deleteForm = FindDeleteFormNearText(indexHtml, data["Message"]!);

        var response = await host.Client.PostAsync(deleteForm.Action, BuildForm(deleteForm.Html, new Dictionary<string, string?>(), includeAntiForgery: false));

        AssertRejectedOrDoesNotPersist(response);
        var adminHtml = await GetHtmlAsync(host.Client, "/Admin/Index");
        AssertVisibleSubmission(adminHtml, data);
    }

    [TestMethod]
    public async Task AdminCreatePost_WithInvalidData_DoesNotPersistInvalidValues()
    {
        using var host = AcceptanceTestHost.Create();
        var marker = Unique("invalid-create");
        var createHtml = await GetHtmlAsync(host.Client, "/Admin/Create");

        var response = await host.Client.PostAsync("/Admin/Create", BuildForm(createHtml, new Dictionary<string, string?>
        {
            ["Name"] = "",
            ["Email"] = $"not-an-email-{marker}",
            ["Phone"] = "555-0100",
            ["Message"] = ""
        }));

        Assert.IsTrue(response.StatusCode == HttpStatusCode.OK || !IsSuccess(response), "Invalid create should return the form, validation, or another non-success result.");
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var html = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(ContainsAny(html, "<form", "validation", "field-validation", "text-danger", "Name", "Email", "Message"));
        }

        var adminHtml = await GetHtmlAsync(host.Client, "/Admin/Index");
        AssertDoesNotContain(adminHtml, marker, "Invalid admin create values should not be persisted.");
    }

    [TestMethod]
    public async Task AdminEditPost_WithInvalidData_PreservesOriginalStoredValues()
    {
        using var host = AcceptanceTestHost.Create();
        var original = await CreateAdminSubmissionAsync(host.Client);
        var indexHtml = await GetHtmlAsync(host.Client, "/Admin/Index");
        var editUrl = FindActionUrlNearText(indexHtml, original["Message"]!, "Edit");
        var editHtml = await GetHtmlAsync(host.Client, editUrl);
        var marker = Unique("invalid-edit");

        var response = await host.Client.PostAsync(editUrl, BuildForm(editHtml, new Dictionary<string, string?>
        {
            ["Id"] = GetInputValue(editHtml, "Id"),
            ["Name"] = "",
            ["Email"] = $"not-an-email-{marker}",
            ["Phone"] = "555-0100",
            ["Message"] = ""
        }));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Invalid edit should keep the user on the edit form.");
        var adminHtml = await GetHtmlAsync(host.Client, "/Admin/Index");
        AssertVisibleSubmission(adminHtml, original);
        AssertDoesNotContain(adminHtml, marker, "Invalid edit values should not replace the original stored values.");
    }

    [TestMethod]
    public async Task MissingAdminResources_ReturnClearNonSuccessResponses()
    {
        using var host = AcceptanceTestHost.Create();

        await AssertClearNonSuccessAsync(await host.Client.GetAsync("/Admin/Details/999999"), "GET missing details");
        await AssertClearNonSuccessAsync(await host.Client.GetAsync("/Admin/Edit/999999"), "GET missing edit");
        await AssertClearNonSuccessAsync(await host.Client.GetAsync("/Admin/Reply/999999"), "GET missing reply");

        var createHtml = await GetHtmlAsync(host.Client, "/Admin/Create");
        var deleteResponse = await host.Client.PostAsync("/Admin/Delete/999999", BuildForm(createHtml, new Dictionary<string, string?>()));
        await AssertClearNonSuccessAsync(deleteResponse, "POST missing delete");
    }

    [TestMethod]
    public async Task ContactPost_WithBlankPhone_SucceedsAndRendersInAdminList()
    {
        using var host = AcceptanceTestHost.Create();
        var data = ContactData(Unique("blank-phone"));
        data["Phone"] = "";
        var formHtml = await GetHtmlAsync(host.Client, "/Contact/Contact");

        var response = await host.Client.PostAsync("/Contact/Contact", BuildForm(formHtml, data));

        AssertRedirectsTo(response, "/ThankYou/Index");
        var adminHtml = await GetHtmlAsync(host.Client, "/Admin/Index");
        AssertContains(adminHtml, data["Name"]!, "Submission with blank optional phone should be visible.");
        AssertContains(adminHtml, data["Email"]!, "Submission with blank optional phone should keep email visible.");
        AssertContains(adminHtml, data["Message"]!, "Submission with blank optional phone should keep message visible.");
    }

    [TestMethod]
    public async Task UserSubmittedHtml_IsEncodedAndNotRenderedAsRawExecutableHtml()
    {
        using var host = AcceptanceTestHost.Create();
        var marker = Unique("xss");
        var script = $"<script>alert('{marker}')</script>";
        var data = ContactData(marker);
        data["Name"] = $"Score User {script}";
        data["Message"] = $"Score acceptance message {script}";
        var formHtml = await GetHtmlAsync(host.Client, "/Contact/Contact");

        var response = await host.Client.PostAsync("/Contact/Contact", BuildForm(formHtml, data));

        AssertRedirectsTo(response, "/ThankYou/Index");
        var adminHtml = await GetHtmlAsync(host.Client, "/Admin/Index");
        AssertDoesNotContain(adminHtml, script, "Submitted script content should not render as raw executable HTML.");
        Assert.IsTrue(
            adminHtml.Contains(HttpUtility.HtmlEncode(script), StringComparison.OrdinalIgnoreCase) ||
            adminHtml.Contains("&lt;script&gt;", StringComparison.OrdinalIgnoreCase),
            "Encoded submitted HTML should remain visible as text.");
    }

    [TestMethod]
    public async Task AdminCreatePost_WithOverlongInput_IsRejectedAndNotPersisted()
    {
        using var host = AcceptanceTestHost.Create();
        var marker = Unique("overlong");
        var createHtml = await GetHtmlAsync(host.Client, "/Admin/Create");

        var response = await host.Client.PostAsync("/Admin/Create", BuildForm(createHtml, new Dictionary<string, string?>
        {
            ["Name"] = $"Name {marker} {new string('n', 140)}",
            ["Email"] = $"{marker}-{new string('e', 260)}@example.com",
            ["Phone"] = "555-0100",
            ["Message"] = $"Message {marker} {new string('m', 2_100)}"
        }));

        Assert.IsTrue(response.StatusCode == HttpStatusCode.OK || !IsSuccess(response), "Overlong values should not complete as a successful create.");
        var adminHtml = await GetHtmlAsync(host.Client, "/Admin/Index");
        AssertDoesNotContain(adminHtml, marker, "Overlong input should not be persisted.");
    }

    private static async Task<Dictionary<string, string?>> CreateAdminSubmissionAsync(HttpClient client)
    {
        var data = ContactData(Unique("admin"));
        var createHtml = await GetHtmlAsync(client, "/Admin/Create");
        var response = await client.PostAsync("/Admin/Create", BuildForm(createHtml, data));
        AssertRedirectsTo(response, "/Admin/Index");
        return data;
    }

    private static async Task<string> GetHtmlAsync(HttpClient client, string path)
    {
        var response = await client.GetAsync(path);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, $"GET {path} should return success.");
        return await response.Content.ReadAsStringAsync();
    }

    private static async Task<string> ReadSuccessHtmlAsync(HttpResponseMessage response, string message)
    {
        Assert.IsTrue(IsSuccess(response), message);
        return await response.Content.ReadAsStringAsync();
    }

    private static async Task<string> FollowRedirectAsync(HttpClient client, HttpResponseMessage response, string expectedPath)
    {
        Assert.IsNotNull(response.Headers.Location, "Expected a redirect location.");
        Assert.IsTrue(response.Headers.Location!.ToString().Contains(expectedPath, StringComparison.OrdinalIgnoreCase),
            $"Expected redirect to contain '{expectedPath}' but was '{response.Headers.Location}'.");
        return await GetHtmlAsync(client, response.Headers.Location.ToString());
    }

    private static FormUrlEncodedContent BuildForm(string html, IReadOnlyDictionary<string, string?> values, bool includeAntiForgery = true)
    {
        var fields = ExtractHiddenInputs(html);
        if (!includeAntiForgery)
        {
            fields.Remove("__RequestVerificationToken");
        }

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

    private static void AssertHasFormField(string html, string name)
    {
        var hasInput = Regex.IsMatch(html, $"<(input|textarea)\\b[^>]*\\bname=[\"']{Regex.Escape(name)}[\"']", RegexOptions.IgnoreCase);
        var hasLabel = Regex.IsMatch(html, $">\\s*{Regex.Escape(name)}\\s*<", RegexOptions.IgnoreCase);
        Assert.IsTrue(hasInput || hasLabel, $"Expected the page to expose a visible/input field for {name}.");
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

    private static string Unique(string prefix)
    {
        return $"{prefix}-{Guid.NewGuid():N}";
    }

    private static bool IsRedirect(HttpResponseMessage response)
    {
        return response.StatusCode is HttpStatusCode.Redirect or HttpStatusCode.RedirectMethod or HttpStatusCode.SeeOther;
    }

    private static bool IsSuccess(HttpResponseMessage response)
    {
        return (int)response.StatusCode >= 200 && (int)response.StatusCode < 300;
    }

    private static void AssertRejectedOrDoesNotPersist(HttpResponseMessage response)
    {
        Assert.IsTrue(!IsSuccess(response) || response.StatusCode == HttpStatusCode.OK || IsRedirect(response),
            $"Unexpected response status {(int)response.StatusCode}.");
    }

    private static async Task AssertClearNonSuccessAsync(HttpResponseMessage response, string scenario)
    {
        if ((int)response.StatusCode >= 500)
        {
            var body = await response.Content.ReadAsStringAsync();
            Assert.Fail($"{scenario} should not server-crash. Status: {(int)response.StatusCode}. Body length: {body.Length}.");
        }

        Assert.IsFalse(IsSuccess(response), $"{scenario} should return 404 or another clear non-success response.");
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
}
