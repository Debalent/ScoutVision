using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScoutVision.Infrastructure.Collaboration;

/// <summary>
/// Real-time collaboration service for team workspaces
/// </summary>
public interface ICollaborationService
{
    // Workspaces
    Task<Workspace> CreateWorkspaceAsync(string name, string ownerId);
    Task<Workspace> GetWorkspaceAsync(string workspaceId);
    Task<List<Workspace>> GetUserWorkspacesAsync(string userId);
    Task UpdateWorkspaceAsync(Workspace workspace);
    Task DeleteWorkspaceAsync(string workspaceId);
    
    // Workspace members
    Task<bool> AddMemberAsync(string workspaceId, string userId, WorkspaceRole role);
    Task<bool> RemoveMemberAsync(string workspaceId, string userId);
    Task<bool> UpdateMemberRoleAsync(string workspaceId, string userId, WorkspaceRole role);
    Task<List<WorkspaceMember>> GetWorkspaceMembersAsync(string workspaceId);
    
    // Shared scouting lists
    Task<ScoutingList> CreateScoutingListAsync(string workspaceId, ScoutingList list);
    Task<List<ScoutingList>> GetWorkspaceScoutingListsAsync(string workspaceId);
    Task<bool> AddPlayerToListAsync(string listId, int playerId, string addedBy);
    Task<bool> RemovePlayerFromListAsync(string listId, int playerId);
    Task<bool> UpdatePlayerNotesAsync(string listId, int playerId, string notes, string updatedBy);
    
    // Comments and annotations
    Task<Comment> AddCommentAsync(string entityType, string entityId, Comment comment);
    Task<List<Comment>> GetCommentsAsync(string entityType, string entityId);
    Task<bool> UpdateCommentAsync(string commentId, string content);
    Task<bool> DeleteCommentAsync(string commentId);
    Task<bool> AddReactionAsync(string commentId, string userId, string reaction);
    
    // Real-time presence
    Task<bool> UpdatePresenceAsync(string userId, string workspaceId, PresenceInfo presence);
    Task<List<PresenceInfo>> GetActiveUsersAsync(string workspaceId);
    Task<bool> BroadcastCursorPositionAsync(string userId, string workspaceId, CursorPosition position);
    
    // Activity feed
    Task<List<Activity>> GetWorkspaceActivityAsync(string workspaceId, int page = 1, int pageSize = 20);
    Task LogActivityAsync(Activity activity);
}

public class Workspace
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public string? ClubId { get; set; }
    public WorkspaceSettings Settings { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}

public class WorkspaceSettings
{
    public bool AllowGuestAccess { get; set; }
    public bool RequireApprovalForNewMembers { get; set; }
    public bool EnableComments { get; set; } = true;
    public bool EnableRealTimeCollaboration { get; set; } = true;
    public bool EnableActivityFeed { get; set; } = true;
    public WorkspaceVisibility Visibility { get; set; } = WorkspaceVisibility.Private;
}

public enum WorkspaceVisibility
{
    Private,
    Internal,
    Public
}

public class WorkspaceMember
{
    public string WorkspaceId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? UserEmail { get; set; }
    public string? UserAvatar { get; set; }
    public WorkspaceRole Role { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastActive { get; set; }
    public bool IsOnline { get; set; }
}

public enum WorkspaceRole
{
    Owner,
    Admin,
    Editor,
    Viewer,
    Guest
}

public class ScoutingList
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string WorkspaceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ScoutingListType Type { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public List<ScoutingListPlayer> Players { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum ScoutingListType
{
    Watchlist,
    Shortlist,
    TransferTargets,
    LoanTargets,
    YouthProspects,
    CompetitorAnalysis,
    Custom
}

public class ScoutingListPlayer
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string? Position { get; set; }
    public string? Club { get; set; }
    public int Priority { get; set; } = 0; // 1-5 stars
    public string? Notes { get; set; }
    public string AddedBy { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    public string? LastUpdatedBy { get; set; }
    public DateTime? LastUpdatedAt { get; set; }
    public Dictionary<string, object>? CustomFields { get; set; }
}

public class Comment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EntityType { get; set; } = string.Empty; // "Player", "Match", "Report", etc.
    public string EntityId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ParentCommentId { get; set; } // For threaded comments
    public List<CommentReaction> Reactions { get; set; } = new();
    public List<string> Mentions { get; set; } = new(); // @username mentions
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsEdited => UpdatedAt.HasValue;
    public bool IsDeleted { get; set; }
}

public class CommentReaction
{
    public string UserId { get; set; } = string.Empty;
    public string Reaction { get; set; } = string.Empty; // 👍, ❤️, 😂, etc.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class PresenceInfo
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public string WorkspaceId { get; set; } = string.Empty;
    public string? CurrentPage { get; set; }
    public string? CurrentEntity { get; set; }
    public PresenceStatus Status { get; set; } = PresenceStatus.Online;
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    public CursorPosition? CursorPosition { get; set; }
}

public enum PresenceStatus
{
    Online,
    Away,
    Busy,
    Offline
}

public class CursorPosition
{
    public double X { get; set; }
    public double Y { get; set; }
    public string? ElementId { get; set; }
}

public class Activity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string WorkspaceId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public ActivityType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? EntityName { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public enum ActivityType
{
    PlayerAdded,
    PlayerRemoved,
    PlayerUpdated,
    CommentAdded,
    ListCreated,
    ListUpdated,
    ListDeleted,
    MemberJoined,
    MemberLeft,
    ReportGenerated,
    ReportShared,
    WorkspaceCreated,
    WorkspaceUpdated,
    SettingsChanged
}

/// <summary>
/// Service for approval workflows
/// </summary>
public interface IApprovalWorkflowService
{
    Task<ApprovalRequest> CreateApprovalRequestAsync(ApprovalRequest request);
    Task<List<ApprovalRequest>> GetPendingApprovalsAsync(string userId);
    Task<bool> ApproveRequestAsync(string requestId, string approverId, string? comments);
    Task<bool> RejectRequestAsync(string requestId, string approverId, string reason);
    Task<ApprovalRequest> GetApprovalRequestAsync(string requestId);
    Task<List<ApprovalRequest>> GetRequestHistoryAsync(string workspaceId);
}

public class ApprovalRequest
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string WorkspaceId { get; set; } = string.Empty;
    public ApprovalType Type { get; set; }
    public string RequestedBy { get; set; } = string.Empty;
    public string RequestedByName { get; set; } = string.Empty;
    public List<string> Approvers { get; set; } = new();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
    public List<ApprovalAction> Actions { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}

public enum ApprovalType
{
    TransferRecommendation,
    BudgetRequest,
    ReportPublication,
    PlayerRelease,
    ContractExtension,
    ScoutingTrip,
    DataExport
}

public enum ApprovalStatus
{
    Pending,
    Approved,
    Rejected,
    Cancelled
}

public class ApprovalAction
{
    public string ApproverId { get; set; } = string.Empty;
    public string ApproverName { get; set; } = string.Empty;
    public ApprovalDecision Decision { get; set; }
    public string? Comments { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public enum ApprovalDecision
{
    Approved,
    Rejected
}

