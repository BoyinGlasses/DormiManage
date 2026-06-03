using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.Abstractions.Data;
using DormitoryManagement.Application.Abstractions.Services;
using DormitoryManagement.Application.Common;
using DormitoryManagement.Application.DTOs.Forum;
using DormitoryManagement.Application.Validation;
using DormitoryManagement.Domain.Constants;
using DormitoryManagement.Domain.Entities;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.Services.Forum;

public sealed class ForumService : IForumService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPermissionService _permissions;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditLogService _auditLog;

    public ForumService(
        IUnitOfWork unitOfWork,
        IPermissionService permissions,
        ICurrentUserService currentUser,
        IAuditLogService auditLog)
    {
        _unitOfWork = unitOfWork;
        _permissions = permissions;
        _currentUser = currentUser;
        _auditLog = auditLog;
    }

    public async Task<IReadOnlyList<ForumCategoryDto>> GetCategoriesAsync(CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.ForumPostsRead, ct);
        return _unitOfWork.Repository<ForumCategory>().Query()
            .Where(category => !category.IsDeleted && category.IsActive)
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .Select(category => new ForumCategoryDto
            {
                Id = category.Id,
                Code = category.Code,
                Name = category.Name,
                Description = category.Description,
                SortOrder = category.SortOrder
            })
            .ToArray();
    }

    public async Task<IReadOnlyList<ForumTagDto>> GetTagsAsync(CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.ForumPostsRead, ct);
        return _unitOfWork.Repository<ForumTag>().Query()
            .Where(tag => !tag.IsDeleted && tag.IsActive)
            .OrderBy(tag => tag.Name)
            .Select(tag => new ForumTagDto
            {
                Id = tag.Id,
                Name = tag.Name,
                Slug = tag.Slug
            })
            .ToArray();
    }

    public async Task<PagedResult<ForumPostDto>> GetPostsAsync(ForumPostFilterRequest? request = null, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.ForumPostsRead, ct);
        request ??= new ForumPostFilterRequest();
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Max(1, request.PageSize);

        var posts = _unitOfWork.Repository<ForumPost>().Query()
            .Where(post => !post.IsDeleted && post.Status != ForumPostStatus.Hidden)
            .ToList()
            .Where(CanViewPost)
            .ToList();

        if (request.CategoryId.HasValue)
        {
            posts = posts.Where(post => post.CategoryId == request.CategoryId.Value).ToList();
        }

        if (request.VisibilityScope.HasValue)
        {
            posts = posts.Where(post => post.VisibilityScope == request.VisibilityScope.Value).ToList();
        }

        if (request.BuildingId.HasValue)
        {
            posts = posts.Where(post => post.BuildingId == request.BuildingId.Value).ToList();
        }

        if (request.RoomId.HasValue)
        {
            posts = posts.Where(post => post.RoomId == request.RoomId.Value).ToList();
        }

        if (!string.IsNullOrWhiteSpace(request.TagSlug))
        {
            var tag = _unitOfWork.Repository<ForumTag>().Query()
                .FirstOrDefault(candidate => candidate.Slug.Equals(request.TagSlug.Trim(), StringComparison.OrdinalIgnoreCase));
            posts = tag is null
                ? new List<ForumPost>()
                : posts.Where(post => _unitOfWork.Repository<ForumPostTag>().Query()
                    .Any(row => row.PostId == post.Id && row.TagId == tag.Id)).ToList();
        }

        var ordered = posts
            .OrderByDescending(post => post.PublishedAt ?? post.CreatedAt)
            .ThenByDescending(post => post.CreatedAt)
            .ToList();
        var totalCount = ordered.Count;
        var page = ordered.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        return new PagedResult<ForumPostDto>(MapPosts(page, includeComments: false), totalCount, pageNumber, pageSize);
    }

    public async Task<ForumPostDto?> GetPostAsync(Guid postId, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.ForumPostsRead, ct);
        var post = await _unitOfWork.Repository<ForumPost>().GetByIdAsync(postId, ct);
        if (post is null || post.IsDeleted || !CanViewPost(post))
        {
            return null;
        }

        return MapPosts(new[] { post }, includeComments: true).Single();
    }

    public async Task<ForumPostDto> CreatePostAsync(CreateForumPostRequest request, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.ForumPostsCreate, ct);
        RequestValidator.ValidateAndThrow(request);
        EnsureCurrentUser();
        EnsureCategoryAndTags(request.CategoryId, request.TagIds);
        EnsureScopeAllowed(request.VisibilityScope, request.BuildingId, request.RoomId, request.TargetRoleName);

        var now = DateTime.UtcNow;
        var post = new ForumPost
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Content = request.Content.Trim(),
            CategoryId = request.CategoryId,
            CreatedByUserId = _currentUser.UserId!.Value,
            VisibilityScope = request.VisibilityScope,
            BuildingId = request.BuildingId,
            RoomId = request.RoomId,
            TargetRoleName = NormalizeRoleName(request.TargetRoleName),
            Status = ForumPostStatus.Published,
            PublishedAt = now,
            CreatedAt = now
        };

        await _unitOfWork.Repository<ForumPost>().AddAsync(post, ct);
        foreach (var tagId in request.TagIds.Distinct())
        {
            await _unitOfWork.Repository<ForumPostTag>().AddAsync(new ForumPostTag
            {
                Id = Guid.NewGuid(),
                PostId = post.Id,
                TagId = tagId
            }, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);
        await _auditLog.WriteAsync("ForumPost.Created", "ForumPost", post.Id, post.Title, ct);
        return MapPosts(new[] { post }, includeComments: true).Single();
    }

    public async Task<ForumPostDto> UpdatePostAsync(UpdateForumPostRequest request, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.ForumPostsUpdateOwn, ct);
        RequestValidator.ValidateAndThrow(request);
        var post = await GetEditablePostAsync(request.PostId, ct);
        EnsureCanMutatePost(post);
        EnsureCategoryAndTags(request.CategoryId, request.TagIds);
        EnsureScopeAllowed(request.VisibilityScope, request.BuildingId, request.RoomId, request.TargetRoleName);

        post.Title = request.Title.Trim();
        post.Content = request.Content.Trim();
        post.CategoryId = request.CategoryId;
        post.VisibilityScope = request.VisibilityScope;
        post.BuildingId = request.BuildingId;
        post.RoomId = request.RoomId;
        post.TargetRoleName = NormalizeRoleName(request.TargetRoleName);
        post.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Repository<ForumPost>().Update(post);

        var existingTags = _unitOfWork.Repository<ForumPostTag>().Query()
            .Where(row => row.PostId == post.Id)
            .ToList();
        foreach (var existingTag in existingTags)
        {
            _unitOfWork.Repository<ForumPostTag>().Remove(existingTag);
        }

        foreach (var tagId in request.TagIds.Distinct())
        {
            await _unitOfWork.Repository<ForumPostTag>().AddAsync(new ForumPostTag
            {
                Id = Guid.NewGuid(),
                PostId = post.Id,
                TagId = tagId
            }, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);
        await _auditLog.WriteAsync("ForumPost.Updated", "ForumPost", post.Id, post.Title, ct);
        return MapPosts(new[] { post }, includeComments: true).Single();
    }

    public async Task DeletePostAsync(Guid postId, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.ForumPostsDeleteOwn, ct);
        var post = await GetEditablePostAsync(postId, ct);
        EnsureCanMutatePost(post);
        post.IsDeleted = true;
        post.DeletedAt = DateTime.UtcNow;
        post.Status = ForumPostStatus.Hidden;
        _unitOfWork.Repository<ForumPost>().Update(post);
        await _unitOfWork.SaveChangesAsync(ct);
        await _auditLog.WriteAsync("ForumPost.Deleted", "ForumPost", post.Id, post.Title, ct);
    }

    public async Task<ForumCommentDto> CreateCommentAsync(CreateForumCommentRequest request, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.ForumPostsCreate, ct);
        RequestValidator.ValidateAndThrow(request);
        EnsureCurrentUser();
        var post = await _unitOfWork.Repository<ForumPost>().GetByIdAsync(request.PostId, ct)
            ?? throw new InvalidOperationException("Forum post was not found.");
        if (!CanViewPost(post))
        {
            throw new InvalidOperationException("Current user cannot comment on this post.");
        }

        if (post.Status == ForumPostStatus.Locked)
        {
            throw new InvalidOperationException("This post is locked.");
        }

        if (request.ParentCommentId.HasValue)
        {
            var parent = await _unitOfWork.Repository<ForumComment>().GetByIdAsync(request.ParentCommentId.Value, ct);
            if (parent is null || parent.PostId != post.Id || parent.IsDeleted)
            {
                throw new InvalidOperationException("Parent comment was not found.");
            }
        }

        var comment = new ForumComment
        {
            Id = Guid.NewGuid(),
            PostId = post.Id,
            ParentCommentId = request.ParentCommentId,
            CreatedByUserId = _currentUser.UserId!.Value,
            Content = request.Content.Trim(),
            Status = ForumCommentStatus.Published,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<ForumComment>().AddAsync(comment, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        await _auditLog.WriteAsync("ForumComment.Created", "ForumComment", comment.Id, post.Id.ToString(), ct);
        return MapComments(new[] { comment }).Single();
    }

    public async Task DeleteCommentAsync(Guid commentId, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.ForumPostsDeleteOwn, ct);
        var comment = await _unitOfWork.Repository<ForumComment>().GetByIdAsync(commentId, ct)
            ?? throw new InvalidOperationException("Forum comment was not found.");
        var post = await _unitOfWork.Repository<ForumPost>().GetByIdAsync(comment.PostId, ct)
            ?? throw new InvalidOperationException("Forum post was not found.");
        if (!CanMutateAuthorContent(comment.CreatedByUserId, post))
        {
            throw new InvalidOperationException("Current user cannot delete this comment.");
        }

        comment.IsDeleted = true;
        comment.DeletedAt = DateTime.UtcNow;
        comment.Status = ForumCommentStatus.Hidden;
        _unitOfWork.Repository<ForumComment>().Update(comment);
        await _unitOfWork.SaveChangesAsync(ct);
        await _auditLog.WriteAsync("ForumComment.Deleted", "ForumComment", comment.Id, post.Id.ToString(), ct);
    }

    private ForumPostDto[] MapPosts(IReadOnlyList<ForumPost> posts, bool includeComments)
    {
        if (posts.Count == 0)
        {
            return Array.Empty<ForumPostDto>();
        }

        var postIds = posts.Select(post => post.Id).ToHashSet();
        var categoryIds = posts.Select(post => post.CategoryId).ToHashSet();
        var categories = _unitOfWork.Repository<ForumCategory>().Query()
            .Where(category => categoryIds.Contains(category.Id))
            .ToDictionary(category => category.Id);
        var postTags = _unitOfWork.Repository<ForumPostTag>().Query()
            .Where(row => postIds.Contains(row.PostId))
            .ToList();
        var tagIds = postTags.Select(row => row.TagId).ToHashSet();
        var tags = _unitOfWork.Repository<ForumTag>().Query()
            .Where(tag => tagIds.Contains(tag.Id))
            .ToDictionary(tag => tag.Id);
        var comments = includeComments
            ? _unitOfWork.Repository<ForumComment>().Query()
                .Where(comment => postIds.Contains(comment.PostId) && !comment.IsDeleted)
                .OrderBy(comment => comment.CreatedAt)
                .ToList()
                .GroupBy(comment => comment.PostId)
                .ToDictionary(group => group.Key, group => group.ToArray())
            : new Dictionary<Guid, ForumComment[]>();

        return posts.Select(post =>
        {
            categories.TryGetValue(post.CategoryId, out var category);
            comments.TryGetValue(post.Id, out var postComments);
            return new ForumPostDto
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                Category = category is null ? new ForumCategoryDto() : new ForumCategoryDto
                {
                    Id = category.Id,
                    Code = category.Code,
                    Name = category.Name,
                    Description = category.Description,
                    SortOrder = category.SortOrder
                },
                Tags = postTags
                    .Where(row => row.PostId == post.Id && tags.ContainsKey(row.TagId))
                    .Select(row =>
                    {
                        var tag = tags[row.TagId];
                        return new ForumTagDto { Id = tag.Id, Name = tag.Name, Slug = tag.Slug };
                    })
                    .ToArray(),
                Comments = includeComments ? MapComments(postComments ?? Array.Empty<ForumComment>()) : Array.Empty<ForumCommentDto>(),
                Author = MapAuthor(post.CreatedByUserId),
                VisibilityScope = post.VisibilityScope,
                BuildingId = post.BuildingId,
                RoomId = post.RoomId,
                TargetRoleName = post.TargetRoleName,
                Status = post.Status,
                CreatedAt = post.CreatedAt,
                CanEdit = CanMutateAuthorContent(post.CreatedByUserId, post),
                CanDelete = CanMutateAuthorContent(post.CreatedByUserId, post),
                CanModerate = CanModeratePost(post)
            };
        }).ToArray();
    }

    private ForumCommentDto[] MapComments(IReadOnlyList<ForumComment> comments) =>
        comments.Select(comment =>
        {
            var post = _unitOfWork.Repository<ForumPost>().Query().FirstOrDefault(candidate => candidate.Id == comment.PostId);
            var canMutate = post is not null && CanMutateAuthorContent(comment.CreatedByUserId, post);
            return new ForumCommentDto
            {
                Id = comment.Id,
                PostId = comment.PostId,
                ParentCommentId = comment.ParentCommentId,
                Content = comment.Content,
                Status = comment.Status,
                Author = MapAuthor(comment.CreatedByUserId),
                CreatedAt = comment.CreatedAt,
                CanEdit = canMutate,
                CanDelete = canMutate
            };
        }).ToArray();

    private ForumAuthorDto MapAuthor(Guid userId)
    {
        var user = _unitOfWork.Repository<User>().Query().FirstOrDefault(candidate => candidate.Id == userId);
        if (user is null)
        {
            return new ForumAuthorDto { UserId = userId, DisplayName = "Unknown user" };
        }

        var student = _unitOfWork.Repository<Student>().Query().FirstOrDefault(candidate => candidate.UserId == userId);
        var manager = _unitOfWork.Repository<Manager>().Query().FirstOrDefault(candidate => candidate.UserId == userId);
        return new ForumAuthorDto
        {
            UserId = user.Id,
            DisplayName = student?.FullName ?? manager?.FullName ?? user.FullName ?? user.Username,
            RoleName = user.Role?.Name ?? _unitOfWork.Repository<Role>().Query().FirstOrDefault(role => role.Id == user.RoleId)?.Name ?? string.Empty,
            StudentCode = student?.StudentCode,
            StaffCode = manager?.StaffCode,
            BuildingId = manager?.BuildingId
        };
    }

    private async Task<ForumPost> GetEditablePostAsync(Guid postId, CancellationToken ct)
    {
        var post = await _unitOfWork.Repository<ForumPost>().GetByIdAsync(postId, ct)
            ?? throw new InvalidOperationException("Forum post was not found.");
        if (post.IsDeleted)
        {
            throw new InvalidOperationException("Forum post was not found.");
        }

        return post;
    }

    private void EnsureCurrentUser()
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            throw new InvalidOperationException("No active user.");
        }
    }

    private void EnsureCanMutatePost(ForumPost post)
    {
        if (!CanMutateAuthorContent(post.CreatedByUserId, post))
        {
            throw new InvalidOperationException("Current user cannot edit or delete this post.");
        }
    }

    private bool CanMutateAuthorContent(Guid authorUserId, ForumPost post) =>
        _currentUser.UserId == authorUserId
        || CanModeratePost(post)
        || _currentUser.IsInRole(RoleNames.Admin);

    private bool CanModeratePost(ForumPost post)
    {
        if (_currentUser.IsInRole(RoleNames.Admin) || _currentUser.IsInRole(RoleNames.Manager) || _currentUser.IsInRole(RoleNames.Staff))
        {
            return true;
        }

        if (!_currentUser.IsInRole(RoleNames.BuildingManager))
        {
            return false;
        }

        var assignedBuildingId = _currentUser.CurrentUser?.BuildingId;
        if (!assignedBuildingId.HasValue)
        {
            return false;
        }

        return post.BuildingId == assignedBuildingId.Value
            || (post.RoomId.HasValue && _unitOfWork.Repository<Room>().Query()
                .Any(room => room.Id == post.RoomId.Value && room.BuildingId == assignedBuildingId.Value));
    }

    private bool CanViewPost(ForumPost post)
    {
        if (post.Status == ForumPostStatus.Hidden && !CanModeratePost(post))
        {
            return false;
        }

        if (_currentUser.IsInRole(RoleNames.Admin) || _currentUser.IsInRole(RoleNames.Manager) || _currentUser.IsInRole(RoleNames.Staff))
        {
            return true;
        }

        return post.VisibilityScope switch
        {
            ForumVisibilityScope.Campus => true,
            ForumVisibilityScope.Role => !string.IsNullOrWhiteSpace(post.TargetRoleName) && _currentUser.IsInRole(post.TargetRoleName),
            ForumVisibilityScope.Building => CanAccessBuilding(post.BuildingId),
            ForumVisibilityScope.Room => CanAccessRoom(post.RoomId),
            _ => false
        };
    }

    private bool CanAccessBuilding(Guid? buildingId)
    {
        if (!buildingId.HasValue)
        {
            return false;
        }

        if (_currentUser.CurrentUser?.BuildingId == buildingId.Value)
        {
            return true;
        }

        return _currentUser.CurrentUser?.StudentId is { } studentId
            && _unitOfWork.Repository<Student>().Query()
                .Any(student => student.Id == studentId
                    && student.CurrentRoomId.HasValue
                    && _unitOfWork.Repository<Room>().Query().Any(room => room.Id == student.CurrentRoomId.Value && room.BuildingId == buildingId.Value));
    }

    private bool CanAccessRoom(Guid? roomId)
    {
        if (!roomId.HasValue)
        {
            return false;
        }

        if (_currentUser.CurrentUser?.BuildingId is { } buildingId
            && _unitOfWork.Repository<Room>().Query().Any(room => room.Id == roomId.Value && room.BuildingId == buildingId))
        {
            return true;
        }

        return _currentUser.CurrentUser?.StudentId is { } studentId
            && _unitOfWork.Repository<Student>().Query().Any(student => student.Id == studentId && student.CurrentRoomId == roomId.Value);
    }

    private void EnsureCategoryAndTags(Guid categoryId, IReadOnlyList<Guid> tagIds)
    {
        var category = _unitOfWork.Repository<ForumCategory>().Query()
            .FirstOrDefault(candidate => candidate.Id == categoryId && !candidate.IsDeleted && candidate.IsActive);
        if (category is null)
        {
            throw new InvalidOperationException("Forum category was not found or inactive.");
        }

        var distinctTagIds = tagIds.Distinct().ToArray();
        if (distinctTagIds.Length == 0)
        {
            return;
        }

        var validTagCount = _unitOfWork.Repository<ForumTag>().Query()
            .Count(tag => distinctTagIds.Contains(tag.Id) && !tag.IsDeleted && tag.IsActive);
        if (validTagCount != distinctTagIds.Length)
        {
            throw new InvalidOperationException("One or more forum tags were not found or inactive.");
        }
    }

    private void EnsureScopeAllowed(ForumVisibilityScope scope, Guid? buildingId, Guid? roomId, string? targetRoleName)
    {
        switch (scope)
        {
            case ForumVisibilityScope.Campus:
                return;
            case ForumVisibilityScope.Building:
                if (!buildingId.HasValue)
                {
                    throw new InvalidOperationException("Building scope requires a building.");
                }
                EnsureCanCreateForBuilding(buildingId.Value);
                return;
            case ForumVisibilityScope.Room:
                if (!roomId.HasValue)
                {
                    throw new InvalidOperationException("Room scope requires a room.");
                }
                var room = _unitOfWork.Repository<Room>().Query().FirstOrDefault(candidate => candidate.Id == roomId.Value && !candidate.IsDeleted)
                    ?? throw new InvalidOperationException("Forum room scope was not found.");
                EnsureCanCreateForBuilding(room.BuildingId);
                return;
            case ForumVisibilityScope.Role:
                var roleName = NormalizeRoleName(targetRoleName);
                if (string.IsNullOrWhiteSpace(roleName)
                    || !_unitOfWork.Repository<Role>().Query().Any(role => role.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidOperationException("Role scope requires an existing role.");
                }
                if (_currentUser.IsInRole(RoleNames.Student) && !string.Equals(roleName, RoleNames.Student, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Students can create only student-scoped role posts.");
                }
                return;
            default:
                throw new InvalidOperationException("Unsupported forum visibility scope.");
        }
    }

    private void EnsureCanCreateForBuilding(Guid buildingId)
    {
        if (_currentUser.IsInRole(RoleNames.Admin) || _currentUser.IsInRole(RoleNames.Manager) || _currentUser.IsInRole(RoleNames.Staff))
        {
            return;
        }

        if (_currentUser.IsInRole(RoleNames.BuildingManager))
        {
            if (_currentUser.CurrentUser?.BuildingId != buildingId)
            {
                throw new InvalidOperationException("Building managers can create forum posts only for their assigned building.");
            }
            return;
        }

        if (_currentUser.IsInRole(RoleNames.Student) && CanAccessBuilding(buildingId))
        {
            return;
        }

        throw new InvalidOperationException("Current user cannot create posts for this building.");
    }

    private static string? NormalizeRoleName(string? roleName) =>
        string.IsNullOrWhiteSpace(roleName) ? null : roleName.Trim();
}
