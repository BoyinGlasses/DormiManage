using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.Abstractions.Repositories;
using DormitoryManagement.Application.Common;
using DormitoryManagement.Application.DTOs.Forum;
using DormitoryManagement.Application.Security;
using DormitoryManagement.Application.Services.Forum;
using DormitoryManagement.Domain.Constants;
using DormitoryManagement.Domain.Entities;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.Tests;

public sealed class ForumModerationServiceTests
{
    [Fact]
    public async Task ReportAsync_CreatesPostReportAndNotifiesModerators()
    {
        var fixture = Fixture.Create(RoleNames.Student);
        var authorId = Guid.NewGuid();
        var post = CreatePost(authorId);
        fixture.UnitOfWork.Set<ForumPost>().Items.Add(post);

        var result = await fixture.Service.ReportAsync(new CreateForumReportRequest
        {
            ForumPostId = post.Id,
            Reason = "Spam",
            Details = "Repeated promotional content."
        });

        Assert.True(result.Succeeded, result.Error);
        var report = Assert.Single(fixture.UnitOfWork.Set<ForumReport>().Items);
        Assert.Equal(ForumReportTargetType.Post, report.TargetType);
        Assert.Equal(ForumReportStatus.Pending, report.Status);
        Assert.Equal(post.Id, report.ForumPostId);
        Assert.Contains(fixture.Notifications.RoleNotifications, notification => notification.RoleName == RoleNames.Manager);
        Assert.Contains(fixture.Notifications.RoleNotifications, notification => notification.RoleName == RoleNames.Admin);
        Assert.Contains(fixture.Audit.Entries, entry => entry.Action == "Forum.ReportCreated");
    }

    [Fact]
    public async Task ReportAsync_RejectsDuplicatePendingReport()
    {
        var fixture = Fixture.Create(RoleNames.Student);
        var post = CreatePost(Guid.NewGuid());
        fixture.UnitOfWork.Set<ForumPost>().Items.Add(post);
        await fixture.Service.ReportAsync(new CreateForumReportRequest { ForumPostId = post.Id, Reason = "Spam" });

        var result = await fixture.Service.ReportAsync(new CreateForumReportRequest { ForumPostId = post.Id, Reason = "Spam" });

        Assert.False(result.Succeeded);
        Assert.Single(fixture.UnitOfWork.Set<ForumReport>().Items);
    }

    [Fact]
    public async Task ResolveReportAsync_WhenManagerHidesPost_UpdatesReportContentNotificationAndAudit()
    {
        var fixture = Fixture.Create(RoleNames.Manager);
        var reporterId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var post = CreatePost(authorId);
        var report = new ForumReport
        {
            Id = Guid.NewGuid(),
            ReporterUserId = reporterId,
            TargetType = ForumReportTargetType.Post,
            ForumPostId = post.Id,
            Reason = "Abuse",
            Status = ForumReportStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        fixture.UnitOfWork.Set<ForumPost>().Items.Add(post);
        fixture.UnitOfWork.Set<ForumReport>().Items.Add(report);

        var result = await fixture.Service.ResolveReportAsync(new ResolveForumReportRequest
        {
            ReportId = report.Id,
            Action = ForumModerationAction.Hide,
            ResolutionNote = "Hidden pending further review."
        });

        Assert.True(result.Succeeded, result.Error);
        Assert.Equal(ForumPostStatus.Hidden, post.Status);
        Assert.Equal(ForumReportStatus.Resolved, report.Status);
        Assert.Equal(ForumModerationAction.Hide, report.ResolutionAction);
        Assert.Contains(fixture.Notifications.UserNotifications, notification => notification.UserId == reporterId);
        Assert.Contains(fixture.Notifications.UserNotifications, notification => notification.UserId == authorId);
        Assert.Contains(fixture.Audit.Entries, entry => entry.Action == "Forum.ReportResolved");
    }

    [Fact]
    public async Task ResolveReportAsync_WhenNotModerator_ReturnsFailure()
    {
        var fixture = Fixture.Create(RoleNames.Student, canModerate: false);
        var post = CreatePost(Guid.NewGuid());
        var report = new ForumReport
        {
            Id = Guid.NewGuid(),
            ReporterUserId = Guid.NewGuid(),
            TargetType = ForumReportTargetType.Post,
            ForumPostId = post.Id,
            Reason = "Spam",
            Status = ForumReportStatus.Pending
        };
        fixture.UnitOfWork.Set<ForumPost>().Items.Add(post);
        fixture.UnitOfWork.Set<ForumReport>().Items.Add(report);

        var result = await fixture.Service.ResolveReportAsync(new ResolveForumReportRequest
        {
            ReportId = report.Id,
            Action = ForumModerationAction.Delete
        });

        Assert.False(result.Succeeded);
        Assert.Equal(ForumPostStatus.Published, post.Status);
        Assert.Equal(ForumReportStatus.Pending, report.Status);
    }

    private static ForumPost CreatePost(Guid authorId) =>
        new()
        {
            Id = Guid.NewGuid(),
            AuthorUserId = authorId,
            Title = "Post",
            Content = "Post content",
            Excerpt = "Post",
            Category = "General",
            Status = ForumPostStatus.Published,
            CreatedAt = DateTime.UtcNow
        };

    private sealed class Fixture
    {
        private Fixture(string roleName, bool canModerate)
        {
            UnitOfWork = new InMemoryUnitOfWork();
            CurrentUser = new TestCurrentUser(roleName);
            Notifications = new RecordingNotificationService();
            Audit = new RecordingAuditLogService();
            Service = new ForumModerationService(
                new InMemoryForumPostRepository(UnitOfWork),
                new InMemoryForumCommentRepository(UnitOfWork),
                UnitOfWork,
                CurrentUser,
                new ForumPermissionService(CurrentUser, canModerate),
                Notifications,
                Audit);
        }

        public InMemoryUnitOfWork UnitOfWork { get; }
        public TestCurrentUser CurrentUser { get; }
        public RecordingNotificationService Notifications { get; }
        public RecordingAuditLogService Audit { get; }
        public ForumModerationService Service { get; }

        public static Fixture Create(string roleName, bool canModerate = true) => new(roleName, canModerate);
    }

    private sealed class ForumPermissionService : IPermissionService
    {
        private readonly TestCurrentUser _currentUser;
        private readonly bool _canModerate;

        public ForumPermissionService(TestCurrentUser currentUser, bool canModerate)
        {
            _currentUser = currentUser;
            _canModerate = canModerate;
        }

        public Task<bool> HasPermissionAsync(string permissionName, CancellationToken ct = default)
        {
            if (permissionName == PermissionNames.ForumRead)
            {
                return Task.FromResult(true);
            }

            if (permissionName == PermissionNames.ForumModerate)
            {
                return Task.FromResult(_canModerate && (_currentUser.IsInRole(RoleNames.Admin) || _currentUser.IsInRole(RoleNames.Manager)));
            }

            return Task.FromResult(false);
        }

        public async Task<AuthorizationResult> AuthorizeAsync(string permissionName, CancellationToken ct = default) =>
            await HasPermissionAsync(permissionName, ct)
                ? AuthorizationResult.Success()
                : AuthorizationResult.Denied("Denied");

        public async Task EnsurePermissionAsync(string permissionName, CancellationToken ct = default)
        {
            if (!await HasPermissionAsync(permissionName, ct))
            {
                throw new AccessDeniedException("Denied");
            }
        }
    }

    private sealed class InMemoryForumPostRepository : IForumPostRepository
    {
        private readonly InMemoryUnitOfWork _unitOfWork;

        public InMemoryForumPostRepository(InMemoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IQueryable<ForumPost> Query() => _unitOfWork.Set<ForumPost>().Items.AsQueryable();
        public Task<ForumPost?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default) =>
            Task.FromResult(Query().FirstOrDefault(post => post.Id == id));
        public Task<PagedResult<ForumPost>> GetFeedAsync(ForumPostFilterRequest filter, CancellationToken ct = default) =>
            Task.FromResult(PagedResult<ForumPost>.Empty(filter.PageNumber, filter.PageSize));
        public Task AddAsync(ForumPost post, CancellationToken ct = default)
        {
            _unitOfWork.Set<ForumPost>().Items.Add(post);
            return Task.CompletedTask;
        }

        public void Update(ForumPost post)
        {
        }
    }

    private sealed class InMemoryForumCommentRepository : IForumCommentRepository
    {
        private readonly InMemoryUnitOfWork _unitOfWork;

        public InMemoryForumCommentRepository(InMemoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<IReadOnlyList<ForumComment>> GetPostCommentsWithDetailsAsync(Guid postId, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<ForumComment>>(_unitOfWork.Set<ForumComment>().Items.Where(comment => comment.ForumPostId == postId).ToArray());

        public Task<ForumComment?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default) =>
            Task.FromResult(_unitOfWork.Set<ForumComment>().Items.FirstOrDefault(comment => comment.Id == id));

        public Task AddAsync(ForumComment comment, CancellationToken ct = default)
        {
            _unitOfWork.Set<ForumComment>().Items.Add(comment);
            return Task.CompletedTask;
        }

        public void Update(ForumComment comment)
        {
        }
    }
}
