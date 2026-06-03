using System.Collections;
using System.Reflection;
using System.Windows.Input;

namespace DormitoryManagement.WPF.Tests;

public sealed class ForumPostDetailViewModelTests
{
    private const string ViewModelTypeName = "DormitoryManagement.WPF.ViewModels.Forum.ForumPostDetailViewModel, DormitoryManagement.WPF";

    [Fact]
    public void Constructor_exposes_deterministic_detail_preview_state_contract()
    {
        var viewModel = CreateViewModel();

        Assert.Equal("DMForum", GetRequiredProperty<string>(viewModel, "BrandText"));
        Assert.Equal("Search forum...", GetRequiredProperty<string>(viewModel, "SearchPlaceholder"));
        Assert.True(GetRequiredProperty<bool>(viewModel, "IsUsingPreviewData"));
        Assert.Null(GetReferenceProperty(viewModel, "StatusMessage"));

        AssertCollectionProperty(viewModel, "BreadcrumbItems", minimumCount: 3);
        AssertCollectionProperty(viewModel, "Comments", minimumCount: 1);
        AssertCollectionProperty(viewModel, "Categories", minimumCount: 1);
        AssertCollectionProperty(viewModel, "RelatedPosts", minimumCount: 1);
        AssertCollectionProperty(viewModel, "TrendingTags", minimumCount: 1);

        var article = GetReferenceProperty(viewModel, "Article");
        Assert.NotNull(article);
        Assert.Equal("Thông báo lịch bảo trì điện nước khu B", GetRequiredProperty<string>(article!, "Title"));
        Assert.Equal("Ban Quản Lý", GetRequiredProperty<string>(article!, "Author"));
        Assert.Equal("2 hours ago", GetRequiredProperty<string>(article!, "RelativeTimeText"));

        var composer = GetReferenceProperty(viewModel, "Composer");
        Assert.NotNull(composer);
        Assert.Equal("Add to the discussion...", GetRequiredProperty<string>(composer!, "Placeholder"));
        Assert.Equal("Post", GetRequiredProperty<string>(composer!, "SubmitLabel"));
    }

    [Fact]
    public void Safe_action_commands_update_local_feedback_without_live_mutation()
    {
        var viewModel = CreateViewModel();
        var article = GetReferenceProperty(viewModel, "Article");
        var firstComment = GetFirstCollectionItem(viewModel, "Comments");
        var firstRelatedPost = GetFirstCollectionItem(viewModel, "RelatedPosts");
        var firstCategory = GetFirstCollectionItem(viewModel, "Categories");
        var firstTrendingTag = GetFirstCollectionItem(viewModel, "TrendingTags");
        var composer = GetReferenceProperty(viewModel, "Composer");

        Assert.NotNull(article);
        Assert.NotNull(composer);

        var initialLikeState = GetRequiredProperty<bool>(article!, "IsLiked");
        var initialCommentIds = GetCollectionItems(viewModel, "Comments").Select(item => GetRequiredProperty<string>(item, "Id")).ToArray();
        SetRequiredProperty(composer!, "DraftText", "Bình luận xem trước");

        ExecuteRequiredCommand(viewModel, "ToggleLikeCommand", article);
        Assert.Equal(!initialLikeState, GetRequiredProperty<bool>(article!, "IsLiked"));
        Assert.True(GetRequiredProperty<bool>(viewModel, "IsUsingPreviewData"));
        Assert.False(string.IsNullOrWhiteSpace(GetRequiredProperty<string>(viewModel, "StatusMessage")));

        ExecuteRequiredCommand(viewModel, "ReplyCommand", firstComment);
        ExecuteRequiredCommand(viewModel, "ReportCommand", firstComment);
        ExecuteRequiredCommand(viewModel, "SubmitCommentDraftCommand");
        ExecuteRequiredCommand(viewModel, "OpenRelatedPostCommand", firstRelatedPost);
        ExecuteRequiredCommand(viewModel, "OpenCategoryCommand", firstCategory);
        ExecuteRequiredCommand(viewModel, "OpenTrendingTagCommand", firstTrendingTag);
        ExecuteRequiredCommand(viewModel, "OpenCreatePostCommand");
        ExecuteRequiredCommand(viewModel, "BackToForumCommand");

        Assert.Equal(initialCommentIds, GetCollectionItems(viewModel, "Comments").Select(item => GetRequiredProperty<string>(item, "Id")).ToArray());
        Assert.Equal(string.Empty, GetRequiredProperty<string>(composer!, "DraftText"));
        Assert.True(GetRequiredProperty<bool>(viewModel, "IsUsingPreviewData"));
        Assert.False(string.IsNullOrWhiteSpace(GetRequiredProperty<string>(viewModel, "StatusMessage")));
    }

    private static object CreateViewModel()
    {
        var viewModelType = Type.GetType(ViewModelTypeName);

        Assert.NotNull(viewModelType);

        var constructor = viewModelType!.GetConstructor(Type.EmptyTypes);
        Assert.NotNull(constructor);

        return constructor!.Invoke([]);
    }

    private static void AssertCollectionProperty(object instance, string propertyName, int minimumCount)
    {
        var value = GetReferenceProperty(instance, propertyName);

        Assert.NotNull(value);
        Assert.IsAssignableFrom<IEnumerable>(value);
        Assert.True(Count((IEnumerable)value) >= minimumCount, $"{propertyName} should contain at least {minimumCount} preview items.");
    }

    private static object GetFirstCollectionItem(object instance, string propertyName)
    {
        var value = GetReferenceProperty(instance, propertyName);

        Assert.NotNull(value);
        Assert.IsAssignableFrom<IEnumerable>(value);

        foreach (var item in (IEnumerable)value)
        {
            if (item is not null)
            {
                return item;
            }
        }

        throw new Xunit.Sdk.XunitException($"{propertyName} should contain at least one item.");
    }

    private static void ExecuteRequiredCommand(object instance, string propertyName, object? parameter = null)
    {
        var command = GetRequiredProperty<ICommand>(instance, propertyName);

        Assert.True(command.CanExecute(parameter), $"{propertyName} should be executable for the review-build placeholder flow.");
        command.Execute(parameter);
    }

    private static T GetRequiredProperty<T>(object instance, string propertyName)
    {
        var property = instance.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(property);

        var value = property!.GetValue(instance);

        Assert.NotNull(value);
        return Assert.IsAssignableFrom<T>(value);
    }

    private static object? GetReferenceProperty(object instance, string propertyName)
    {
        var property = instance.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(property);
        return property!.GetValue(instance);
    }

    private static void SetRequiredProperty(object instance, string propertyName, object? value)
    {
        var property = instance.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(property);
        Assert.True(property!.CanWrite, $"{propertyName} should be writable for local preview interaction.");
        property.SetValue(instance, value);
    }

    private static List<object> GetCollectionItems(object instance, string propertyName)
    {
        var value = GetReferenceProperty(instance, propertyName);

        Assert.NotNull(value);
        Assert.IsAssignableFrom<IEnumerable>(value);

        return ((IEnumerable)value)
            .Cast<object?>()
            .Where(item => item is not null)
            .Cast<object>()
            .ToList();
    }

    private static int Count(IEnumerable source)
    {
        var count = 0;
        foreach (var _ in source)
        {
            count++;
        }

        return count;
    }
}
