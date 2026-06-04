using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.Abstractions.Data;
using DormitoryManagement.Application.Abstractions.Repositories;
using DormitoryManagement.Application.Abstractions.Services;
using DormitoryManagement.Application.Common;
using DormitoryManagement.Application.DTOs.Forum;
using DormitoryManagement.Domain.Constants;
using DormitoryManagement.Domain.Entities;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.Services.Forum;

public sealed class ForumModerationService : IForumModerationService
{
    private readonly IForumPostRepository _posts;
    private readonly IForumCommentRepository _comments;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IPermissionService _permissions;
    private readonly INotificationService _notifications;
    private readonly IAuditLogService _audit;

    public ForumModerationService(
        IForumPostRepository posts,
        IForumCommentRepository comments,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IPermissionService permissions,
        INotificationService notifications,
        IAuditLogService audit)
    {
        _posts = posts;
        _comments = comments;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _permissions = permissions;
        _notifications = notifications;
        _audit = audit;
    }

    public async Task<Result<ForumReportDto>> ReportAsync(CreateForumReportRequest request, CancellationToken ct = default)
    {
        if (_currentUser.UserId is not { } reporterUserId)
        {
            return Result<ForumReportDto>.Failure("Current user must be authenticated.");
        }

        if (!await _permissions.HasPermissionAsync(PermissionNames.ForumRead, ct))
        {
            return Result<ForumReportDto>.Failure("You do not have permission to report forum content.");
        }

        var validation = ValidateReportRequest(request);
        if (validation is not null)
        {
            return Result<ForumReportDto>.Failure(validation);
        }

        var reports = _unitOfWork.Repository<ForumReport>();
        if (request.ForumPostId is { } postId)
        {
            var post = await _posts.GetByIdWithDetailsAsync(postId, ct);
            if (post is null || post.Status is ForumPostStatus.Deleted)
            {
                return Result<ForumReportDto>.Failure("Forum post was not found.");
            }

            if (reports.Query().Any(report => report.ReporterUserId == reporterUserId &&
                                              report.ForumPostId == postId &&
                                              report.Status == ForumReportStatus.Pending))
            {
                return Result<ForumReportDto>.Failure("You already have a pending report for this post.");
            }

            var report = CreateReport(reporterUserId, ForumReportTargetType.Post, postId, null, request);
            await reports.AddAsync(report, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            await NotifyModeratorsAsync("Forum post reported", $"Post \"{post.Title}\" was reported for review.", ct);
            await _audit.WriteAsync("Forum.ReportCreated", nameof(ForumReport), report.Id, $"PostId={post.Id}; Reason={report.Reason}", ct);
            return Result<ForumReportDto>.Success(Map(report, post, null));
        }

        var commentId = request.ForumCommentId!.Value;
        var comment = await _comments.GetByIdWithDetailsAsync(commentId, ct);
        if (comment is null || comment.Status is ForumCommentStatus.Deleted)
        {
            return Result<ForumReportDto>.Failure("Forum comment was not found.");
        }

        if (reports.Query().Any(report => report.ReporterUserId == reporterUserId &&
                                          report.ForumCommentId == commentId &&
                                          report.Status == ForumReportStatus.Pending))
        {
            return Result<ForumReportDto>.Failure("You already have a pending report for this comment.");
        }

        var commentReport = CreateReport(reporterUserId, ForumReportTargetType.Comment, null, commentId, request);
        await reports.AddAsync(commentReport, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        await NotifyModeratorsAsync("Forum comment reported", "A forum comment was reported for review.", ct);
        await _audit.WriteAsync("Forum.ReportCreated", nameof(ForumReport), commentReport.Id, $"CommentId={comment.Id}; Reason={commentReport.Reason}", ct);
        return Result<ForumReportDto>.Success(Map(commentReport, null, comment));
    }

    public async Task<IReadOnlyList<ForumReportDto>> GetPendingReportsAsync(CancellationToken ct = default)
    {
        if (!await _permissions.HasPermissionAsync(PermissionNames.ForumModerate, ct))
        {
            return Array.Empty<ForumReportDto>();
        }

        var pending = _unitOfWork.Repository<ForumReport>()
            .Query()
            .Where(report => report.Status == ForumReportStatus.Pending)
            .OrderBy(report => report.CreatedAt)
            .ToArray();
        return pending.Select(report => Map(report, null, null)).ToArray();
    }

    public async Task<Result<ForumReportDto>> ResolveReportAsync(ResolveForumReportRequest request, CancellationToken ct = default)
    {
        if (!await _permissions.HasPermissionAsync(PermissionNames.ForumModerate, ct))
        {
            return Result<ForumReportDto>.Failure("Only Admin or Manager can moderate reports.");
        }

        var reports = _unitOfWork.Repository<ForumReport>();
        var report = await reports.GetByIdAsync(request.ReportId, ct);
        if (report is null)
        {
            return Result<ForumReportDto>.Failure("Forum report was not found.");
        }

        if (report.Status != ForumReportStatus.Pending)
        {
            return Result<ForumReportDto>.Failure("Only pending reports can be resolved.");
        }

        if (!string.IsNullOrWhiteSpace(request.ResolutionNote) && request.ResolutionNote.Trim().Length > 1000)
        {
            return Result<ForumReportDto>.Failure("Resolution note must be 1000 characters or fewer.");
        }

        ForumPost? post = null;
        ForumComment? comment = null;
        Guid? authorUserId;
        if (report.TargetType == ForumReportTargetType.Post)
        {
            post = await _posts.GetByIdWithDetailsAsync(report.ForumPostId!.Value, ct);
            if (post is null)
            {
                return Result<ForumReportDto>.Failure("Reported post was not found.");
            }

            ApplyPostAction(post, request.Action);
            _posts.Update(post);
            authorUserId = post.AuthorUserId;
        }
        else
        {
            comment = await _comments.GetByIdWithDetailsAsync(report.ForumCommentId!.Value, ct);
            if (comment is null)
            {
                return Result<ForumReportDto>.Failure("Reported comment was not found.");
            }

            ApplyCommentAction(comment, request.Action);
            _comments.Update(comment);
            authorUserId = comment.AuthorUserId;
        }

        report.Status = request.Action == ForumModerationAction.Reject
            ? ForumReportStatus.Rejected
            : ForumReportStatus.Resolved;
        report.ResolutionAction = request.Action;
        report.ResolutionNote = NormalizeOptional(request.ResolutionNote);
        report.ReviewedByUserId = _currentUser.UserId;
        report.ReviewedAt = DateTime.UtcNow;
        report.UpdatedAt = DateTime.UtcNow;
        report.UpdatedBy = _currentUser.UserName;
        reports.Update(report);
        await _unitOfWork.SaveChangesAsync(ct);

        await _notifications.NotifyUserAsync(report.ReporterUserId, "Forum report reviewed", $"Your report was {report.Status}.", ct);
        if (authorUserId.HasValue && authorUserId != report.ReporterUserId)
        {
            await _notifications.NotifyUserAsync(authorUserId.Value, "Forum content moderated", $"A moderator applied action: {request.Action}.", ct);
        }

        await _audit.WriteAsync("Forum.ReportResolved", nameof(ForumReport), report.Id, $"Action={request.Action}; Status={report.Status}", ct);
        return Result<ForumReportDto>.Success(Map(report, post, comment));
    }

    private async Task NotifyModeratorsAsync(string title, string message, CancellationToken ct)
    {
        await _notifications.NotifyRoleAsync(RoleNames.Manager, title, message, ct);
        await _notifications.NotifyRoleAsync(RoleNames.Admin, title, message, ct);
    }

    private static ForumReport CreateReport(
        Guid reporterUserId,
        ForumReportTargetType targetType,
        Guid? postId,
        Guid? commentId,
        CreateForumReportRequest request) =>
        new()
        {
            Id = Guid.NewGuid(),
            ReporterUserId = reporterUserId,
            TargetType = targetType,
            ForumPostId = postId,
            ForumCommentId = commentId,
            Reason = request.Reason.Trim(),
            Details = NormalizeOptional(request.Details),
            Status = ForumReportStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

    private static string? ValidateReportRequest(CreateForumReportRequest request)
    {
        var hasPost = request.ForumPostId.HasValue;
        var hasComment = request.ForumCommentId.HasValue;
        if (hasPost == hasComment)
        {
            return "Report must target exactly one post or comment.";
        }

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return "Report reason is required.";
        }

        if (request.Reason.Trim().Length > 100)
        {
            return "Report reason must be 100 characters or fewer.";
        }

        return !string.IsNullOrWhiteSpace(request.Details) && request.Details.Trim().Length > 1000
            ? "Report details must be 1000 characters or fewer."
            : null;
    }

    private static void ApplyPostAction(ForumPost post, ForumModerationAction action)
    {
        if (action == ForumModerationAction.Hide)
        {
            post.Status = ForumPostStatus.Hidden;
        }
        else if (action == ForumModerationAction.Delete)
        {
            post.Status = ForumPostStatus.Deleted;
        }
        else if (action == ForumModerationAction.Restore)
        {
            post.Status = ForumPostStatus.Published;
        }

        post.UpdatedAt = DateTime.UtcNow;
    }

    private static void ApplyCommentAction(ForumComment comment, ForumModerationAction action)
    {
        if (action == ForumModerationAction.Hide)
        {
            comment.Status = ForumCommentStatus.Hidden;
        }
        else if (action == ForumModerationAction.Delete)
        {
            comment.Status = ForumCommentStatus.Deleted;
        }
        else if (action == ForumModerationAction.Restore)
        {
            comment.Status = ForumCommentStatus.Published;
        }

        comment.UpdatedAt = DateTime.UtcNow;
    }

    private static ForumReportDto Map(ForumReport report, ForumPost? post, ForumComment? comment)
    {
        var targetId = report.TargetType == ForumReportTargetType.Post
            ? report.ForumPostId!.Value
            : report.ForumCommentId!.Value;
        return new ForumReportDto
        {
            Id = report.Id,
            ReporterUserId = report.ReporterUserId,
            ReporterName = report.ReporterUser?.FullName ?? report.ReporterUser?.Username ?? "User",
            TargetType = report.TargetType,
            TargetId = targetId,
            TargetTitle = post?.Title ?? comment?.ForumPost?.Title ?? comment?.Content ?? targetId.ToString("N"),
            Reason = report.Reason,
            Details = report.Details,
            Status = report.Status,
            ResolutionAction = report.ResolutionAction,
            ResolutionNote = report.ResolutionNote,
            CreatedAt = report.CreatedAt,
            ReviewedAt = report.ReviewedAt
        };
    }

    private static string? NormalizeOptional(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }
}
