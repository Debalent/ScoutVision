using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScoutVision.Infrastructure.Collaboration;

namespace ScoutVision.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CollaborationController : ControllerBase
{
    private readonly ICollaborationService _collaborationService;
    private readonly IApprovalWorkflowService _approvalService;

    public CollaborationController(
        ICollaborationService collaborationService,
        IApprovalWorkflowService approvalService)
    {
        _collaborationService = collaborationService;
        _approvalService = approvalService;
    }

    #region Workspaces

    /// <summary>
    /// Create a new workspace
    /// </summary>
    [HttpPost("workspaces")]
    public async Task<ActionResult<Workspace>> CreateWorkspace([FromBody] CreateWorkspaceRequest request)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        var workspace = await _collaborationService.CreateWorkspaceAsync(request.Name, userId);
        return CreatedAtAction(nameof(GetWorkspace), new { workspaceId = workspace.Id }, workspace);
    }

    /// <summary>
    /// Get workspace by ID
    /// </summary>
    [HttpGet("workspaces/{workspaceId}")]
    public async Task<ActionResult<Workspace>> GetWorkspace(string workspaceId)
    {
        var workspace = await _collaborationService.GetWorkspaceAsync(workspaceId);
        return Ok(workspace);
    }

    /// <summary>
    /// Get user's workspaces
    /// </summary>
    [HttpGet("workspaces")]
    public async Task<ActionResult<List<Workspace>>> GetUserWorkspaces()
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        var workspaces = await _collaborationService.GetUserWorkspacesAsync(userId);
        return Ok(workspaces);
    }

    /// <summary>
    /// Update workspace
    /// </summary>
    [HttpPut("workspaces/{workspaceId}")]
    public async Task<IActionResult> UpdateWorkspace(string workspaceId, [FromBody] Workspace workspace)
    {
        workspace.Id = workspaceId;
        await _collaborationService.UpdateWorkspaceAsync(workspace);
        return NoContent();
    }

    /// <summary>
    /// Delete workspace
    /// </summary>
    [HttpDelete("workspaces/{workspaceId}")]
    public async Task<IActionResult> DeleteWorkspace(string workspaceId)
    {
        await _collaborationService.DeleteWorkspaceAsync(workspaceId);
        return NoContent();
    }

    #endregion

    #region Workspace Members

    /// <summary>
    /// Add member to workspace
    /// </summary>
    [HttpPost("workspaces/{workspaceId}/members")]
    public async Task<IActionResult> AddMember(
        string workspaceId,
        [FromBody] AddMemberRequest request)
    {
        await _collaborationService.AddMemberAsync(workspaceId, request.UserId, request.Role);
        return NoContent();
    }

    /// <summary>
    /// Get workspace members
    /// </summary>
    [HttpGet("workspaces/{workspaceId}/members")]
    public async Task<ActionResult<List<WorkspaceMember>>> GetWorkspaceMembers(string workspaceId)
    {
        var members = await _collaborationService.GetWorkspaceMembersAsync(workspaceId);
        return Ok(members);
    }

    /// <summary>
    /// Update member role
    /// </summary>
    [HttpPut("workspaces/{workspaceId}/members/{userId}")]
    public async Task<IActionResult> UpdateMemberRole(
        string workspaceId,
        string userId,
        [FromBody] UpdateMemberRoleRequest request)
    {
        await _collaborationService.UpdateMemberRoleAsync(workspaceId, userId, request.Role);
        return NoContent();
    }

    /// <summary>
    /// Remove member from workspace
    /// </summary>
    [HttpDelete("workspaces/{workspaceId}/members/{userId}")]
    public async Task<IActionResult> RemoveMember(string workspaceId, string userId)
    {
        await _collaborationService.RemoveMemberAsync(workspaceId, userId);
        return NoContent();
    }

    #endregion

    #region Scouting Lists

    /// <summary>
    /// Create scouting list
    /// </summary>
    [HttpPost("workspaces/{workspaceId}/lists")]
    public async Task<ActionResult<ScoutingList>> CreateScoutingList(
        string workspaceId,
        [FromBody] ScoutingList list)
    {
        list.WorkspaceId = workspaceId;
        var createdList = await _collaborationService.CreateScoutingListAsync(workspaceId, list);
        return CreatedAtAction(nameof(GetWorkspaceScoutingLists), 
            new { workspaceId }, createdList);
    }

    /// <summary>
    /// Get workspace scouting lists
    /// </summary>
    [HttpGet("workspaces/{workspaceId}/lists")]
    public async Task<ActionResult<List<ScoutingList>>> GetWorkspaceScoutingLists(string workspaceId)
    {
        var lists = await _collaborationService.GetWorkspaceScoutingListsAsync(workspaceId);
        return Ok(lists);
    }

    /// <summary>
    /// Add player to scouting list
    /// </summary>
    [HttpPost("lists/{listId}/players/{playerId}")]
    public async Task<IActionResult> AddPlayerToList(string listId, int playerId)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        await _collaborationService.AddPlayerToListAsync(listId, playerId, userId);
        return NoContent();
    }

    /// <summary>
    /// Remove player from scouting list
    /// </summary>
    [HttpDelete("lists/{listId}/players/{playerId}")]
    public async Task<IActionResult> RemovePlayerFromList(string listId, int playerId)
    {
        await _collaborationService.RemovePlayerFromListAsync(listId, playerId);
        return NoContent();
    }

    /// <summary>
    /// Update player notes in scouting list
    /// </summary>
    [HttpPut("lists/{listId}/players/{playerId}/notes")]
    public async Task<IActionResult> UpdatePlayerNotes(
        string listId,
        int playerId,
        [FromBody] UpdateNotesRequest request)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        await _collaborationService.UpdatePlayerNotesAsync(listId, playerId, request.Notes, userId);
        return NoContent();
    }

    #endregion

    #region Comments

    /// <summary>
    /// Add comment to entity
    /// </summary>
    [HttpPost("comments")]
    public async Task<ActionResult<Comment>> AddComment([FromBody] AddCommentRequest request)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        var userName = User.Identity?.Name ?? "Unknown";
        
        var comment = new Comment
        {
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            UserId = userId,
            UserName = userName,
            Content = request.Content,
            ParentCommentId = request.ParentCommentId
        };

        var createdComment = await _collaborationService.AddCommentAsync(
            request.EntityType, request.EntityId, comment);
        
        return CreatedAtAction(nameof(GetComments), 
            new { entityType = request.EntityType, entityId = request.EntityId }, 
            createdComment);
    }

    /// <summary>
    /// Get comments for entity
    /// </summary>
    [HttpGet("comments/{entityType}/{entityId}")]
    public async Task<ActionResult<List<Comment>>> GetComments(string entityType, string entityId)
    {
        var comments = await _collaborationService.GetCommentsAsync(entityType, entityId);
        return Ok(comments);
    }

    /// <summary>
    /// Update comment
    /// </summary>
    [HttpPut("comments/{commentId}")]
    public async Task<IActionResult> UpdateComment(
        string commentId,
        [FromBody] UpdateCommentRequest request)
    {
        await _collaborationService.UpdateCommentAsync(commentId, request.Content);
        return NoContent();
    }

    /// <summary>
    /// Delete comment
    /// </summary>
    [HttpDelete("comments/{commentId}")]
    public async Task<IActionResult> DeleteComment(string commentId)
    {
        await _collaborationService.DeleteCommentAsync(commentId);
        return NoContent();
    }

    /// <summary>
    /// Add reaction to comment
    /// </summary>
    [HttpPost("comments/{commentId}/reactions")]
    public async Task<IActionResult> AddReaction(
        string commentId,
        [FromBody] AddReactionRequest request)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        await _collaborationService.AddReactionAsync(commentId, userId, request.Reaction);
        return NoContent();
    }

    #endregion

    #region Activity Feed

    /// <summary>
    /// Get workspace activity feed
    /// </summary>
    [HttpGet("workspaces/{workspaceId}/activity")]
    public async Task<ActionResult<List<Activity>>> GetWorkspaceActivity(
        string workspaceId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var activities = await _collaborationService.GetWorkspaceActivityAsync(workspaceId, page, pageSize);
        return Ok(activities);
    }

    #endregion

    #region Approval Workflows

    /// <summary>
    /// Create approval request
    /// </summary>
    [HttpPost("approvals")]
    public async Task<ActionResult<ApprovalRequest>> CreateApprovalRequest(
        [FromBody] ApprovalRequest request)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        request.RequestedBy = userId;
        
        var createdRequest = await _approvalService.CreateApprovalRequestAsync(request);
        return CreatedAtAction(nameof(GetApprovalRequest), 
            new { requestId = createdRequest.Id }, createdRequest);
    }

    /// <summary>
    /// Get approval request
    /// </summary>
    [HttpGet("approvals/{requestId}")]
    public async Task<ActionResult<ApprovalRequest>> GetApprovalRequest(string requestId)
    {
        var request = await _approvalService.GetApprovalRequestAsync(requestId);
        return Ok(request);
    }

    /// <summary>
    /// Get pending approvals for user
    /// </summary>
    [HttpGet("approvals/pending")]
    public async Task<ActionResult<List<ApprovalRequest>>> GetPendingApprovals()
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        var approvals = await _approvalService.GetPendingApprovalsAsync(userId);
        return Ok(approvals);
    }

    /// <summary>
    /// Approve request
    /// </summary>
    [HttpPost("approvals/{requestId}/approve")]
    public async Task<IActionResult> ApproveRequest(
        string requestId,
        [FromBody] ApprovalActionRequest request)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        await _approvalService.ApproveRequestAsync(requestId, userId, request.Comments);
        return NoContent();
    }

    /// <summary>
    /// Reject request
    /// </summary>
    [HttpPost("approvals/{requestId}/reject")]
    public async Task<IActionResult> RejectRequest(
        string requestId,
        [FromBody] ApprovalActionRequest request)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        await _approvalService.RejectRequestAsync(requestId, userId, request.Comments ?? "No reason provided");
        return NoContent();
    }

    #endregion
}

#region Request Models

public class CreateWorkspaceRequest
{
    public string Name { get; set; } = string.Empty;
}

public class AddMemberRequest
{
    public string UserId { get; set; } = string.Empty;
    public WorkspaceRole Role { get; set; }
}

public class UpdateMemberRoleRequest
{
    public WorkspaceRole Role { get; set; }
}

public class UpdateNotesRequest
{
    public string Notes { get; set; } = string.Empty;
}

public class AddCommentRequest
{
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ParentCommentId { get; set; }
}

public class UpdateCommentRequest
{
    public string Content { get; set; } = string.Empty;
}

public class AddReactionRequest
{
    public string Reaction { get; set; } = string.Empty;
}

public class ApprovalActionRequest
{
    public string? Comments { get; set; }
}

#endregion

