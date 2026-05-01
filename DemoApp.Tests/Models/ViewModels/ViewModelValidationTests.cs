using System.ComponentModel.DataAnnotations;
using DemoApp.Models.ViewModels;

namespace DemoApp.Tests.Models.ViewModels;

[TestClass]
public sealed class ViewModelValidationTests
{
    [TestMethod]
    public void ContactFormViewModel_WithValidValues_IsValid()
    {
        var model = CreateValidContactForm();

        var results = Validate(model);

        Assert.AreEqual(0, results.Count);
    }

    [TestMethod]
    public void ContactFormViewModel_RequiresNameEmailMessageAndSubmissionToken()
    {
        var model = CreateValidContactForm();
        model.Name = string.Empty;
        model.Email = string.Empty;
        model.Message = string.Empty;
        model.SubmissionToken = string.Empty;

        var results = Validate(model);

        AssertHasValidationError(results, nameof(ContactFormViewModel.Name));
        AssertHasValidationError(results, nameof(ContactFormViewModel.Email));
        AssertHasValidationError(results, nameof(ContactFormViewModel.Message));
        AssertHasValidationError(results, nameof(ContactFormViewModel.SubmissionToken));
    }

    [TestMethod]
    public void ContactFormViewModel_InvalidEmail_IsInvalid()
    {
        var model = CreateValidContactForm();
        model.Email = "not-an-email";

        var results = Validate(model);

        AssertHasValidationError(results, nameof(ContactFormViewModel.Email));
    }

    [TestMethod]
    public void ContactFormViewModel_OptionalPhoneCanBeNull()
    {
        var model = CreateValidContactForm();
        model.Phone = null;

        var results = Validate(model);

        AssertHasNoValidationError(results, nameof(ContactFormViewModel.Phone));
    }

    [TestMethod]
    public void ContactFormViewModel_InvalidPhone_IsInvalid()
    {
        var model = CreateValidContactForm();
        model.Phone = "abc";

        var results = Validate(model);

        AssertHasValidationError(results, nameof(ContactFormViewModel.Phone));
    }

    [TestMethod]
    public void ContactFormViewModel_MessageShorterThanMinimum_IsInvalid()
    {
        var model = CreateValidContactForm();
        model.Message = "too short";

        var results = Validate(model);

        AssertHasValidationError(results, nameof(ContactFormViewModel.Message));
    }

    [TestMethod]
    public void ContactFormViewModel_TooLongValues_AreInvalid()
    {
        var model = CreateValidContactForm();
        model.Name = new string('a', 101);
        model.Email = $"{new string('a', 250)}@example.com";
        model.Phone = new string('1', 31);
        model.Message = new string('m', 2001);

        var results = Validate(model);

        AssertHasValidationError(results, nameof(ContactFormViewModel.Name));
        AssertHasValidationError(results, nameof(ContactFormViewModel.Email));
        AssertHasValidationError(results, nameof(ContactFormViewModel.Phone));
        AssertHasValidationError(results, nameof(ContactFormViewModel.Message));
    }

    [TestMethod]
    public void ContactFormViewModel_InvalidSubmissionTokenFormat_IsInvalid()
    {
        var model = CreateValidContactForm();
        model.SubmissionToken = "not-a-token";

        var results = Validate(model);

        AssertHasValidationError(results, nameof(ContactFormViewModel.SubmissionToken));
    }

    [TestMethod]
    public void ContactFormViewModel_SubmissionTokenMustBeThirtyTwoHexCharacters()
    {
        var model = CreateValidContactForm();
        model.SubmissionToken = "abcdef";

        var results = Validate(model);

        AssertHasValidationError(results, nameof(ContactFormViewModel.SubmissionToken));
    }

    [TestMethod]
    public void AdminSubmissionFormViewModel_WithValidValues_IsValid()
    {
        var model = CreateValidAdminForm();

        var results = Validate(model);

        Assert.AreEqual(0, results.Count);
    }

    [TestMethod]
    public void AdminSubmissionFormViewModel_RequiresNameEmailAndMessage()
    {
        var model = CreateValidAdminForm();
        model.Name = string.Empty;
        model.Email = string.Empty;
        model.Message = string.Empty;

        var results = Validate(model);

        AssertHasValidationError(results, nameof(AdminSubmissionFormViewModel.Name));
        AssertHasValidationError(results, nameof(AdminSubmissionFormViewModel.Email));
        AssertHasValidationError(results, nameof(AdminSubmissionFormViewModel.Message));
    }

    [TestMethod]
    public void AdminSubmissionFormViewModel_InvalidEmail_IsInvalid()
    {
        var model = CreateValidAdminForm();
        model.Email = "not-an-email";

        var results = Validate(model);

        AssertHasValidationError(results, nameof(AdminSubmissionFormViewModel.Email));
    }

    [TestMethod]
    public void AdminSubmissionFormViewModel_OptionalPhoneCanBeNull()
    {
        var model = CreateValidAdminForm();
        model.Phone = null;

        var results = Validate(model);

        AssertHasNoValidationError(results, nameof(AdminSubmissionFormViewModel.Phone));
    }

    [TestMethod]
    public void AdminSubmissionFormViewModel_TooLongValues_AreInvalid()
    {
        var model = CreateValidAdminForm();
        model.Name = new string('a', 101);
        model.Email = $"{new string('a', 250)}@example.com";
        model.Phone = new string('1', 31);
        model.Message = new string('m', 2001);

        var results = Validate(model);

        AssertHasValidationError(results, nameof(AdminSubmissionFormViewModel.Name));
        AssertHasValidationError(results, nameof(AdminSubmissionFormViewModel.Email));
        AssertHasValidationError(results, nameof(AdminSubmissionFormViewModel.Phone));
        AssertHasValidationError(results, nameof(AdminSubmissionFormViewModel.Message));
    }

    [TestMethod]
    public void AdminReplyViewModel_WithValidValues_IsValid()
    {
        var model = CreateValidReply();

        var results = Validate(model);

        Assert.AreEqual(0, results.Count);
    }

    [TestMethod]
    public void AdminReplyViewModel_RequiresReply()
    {
        var model = CreateValidReply();
        model.Reply = string.Empty;

        var results = Validate(model);

        AssertHasValidationError(results, nameof(AdminReplyViewModel.Reply));
    }

    [TestMethod]
    public void AdminReplyViewModel_NullReply_IsInvalid()
    {
        var model = CreateValidReply();
        model.Reply = null!;

        var results = Validate(model);

        AssertHasValidationError(results, nameof(AdminReplyViewModel.Reply));
    }

    [TestMethod]
    public void AdminReplyViewModel_TooLongReply_IsInvalid()
    {
        var model = CreateValidReply();
        model.Reply = new string('r', 2001);

        var results = Validate(model);

        AssertHasValidationError(results, nameof(AdminReplyViewModel.Reply));
    }

    [TestMethod]
    public void AdminListAndDetailsViewModels_HaveNoValidationRequirements()
    {
        var list = new AdminSubmissionsViewModel();
        var item = new AdminSubmissionItemViewModel();
        var details = new AdminSubmissionDetailsViewModel();
        var thankYou = new ContactThankYouViewModel();

        Assert.AreEqual(0, Validate(list).Count);
        Assert.AreEqual(0, Validate(item).Count);
        Assert.AreEqual(0, Validate(details).Count);
        Assert.AreEqual(0, Validate(thankYou).Count);
    }

    private static ContactFormViewModel CreateValidContactForm()
    {
        return new ContactFormViewModel
        {
            Name = "Ana",
            Email = "ana@example.com",
            Phone = "+12345",
            Message = "This is a valid contact message.",
            SubmissionToken = "abcdefabcdefabcdefabcdefabcdefab"
        };
    }

    private static AdminSubmissionFormViewModel CreateValidAdminForm()
    {
        return new AdminSubmissionFormViewModel
        {
            Id = 7,
            Name = "Ana",
            Email = "ana@example.com",
            Phone = "+12345",
            Message = "This is a valid admin message."
        };
    }

    private static AdminReplyViewModel CreateValidReply()
    {
        return new AdminReplyViewModel
        {
            Id = 7,
            Name = "Ana",
            Email = "ana@example.com",
            Message = "Original message",
            Reply = "Thanks for writing."
        };
    }

    private static List<ValidationResult> Validate(object model)
    {
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        Validator.TryValidateObject(model, context, results, validateAllProperties: true);

        return results;
    }

    private static void AssertHasValidationError(IEnumerable<ValidationResult> results, string memberName)
    {
        Assert.IsTrue(
            results.Any(r => r.MemberNames.Contains(memberName)),
            $"Expected validation error for {memberName}.");
    }

    private static void AssertHasNoValidationError(IEnumerable<ValidationResult> results, string memberName)
    {
        Assert.IsFalse(
            results.Any(r => r.MemberNames.Contains(memberName)),
            $"Did not expect validation error for {memberName}.");
    }
}
