using System;
using System.Collections.Generic;

namespace vv.Application.DTOs.Governance.Corporate
{
    public class CorporateProposalWorkflowDto
    {
        public required string WorkflowId { get; set; }
        public required string ClientId { get; set; }
        public required string ProposalId { get; set; }
        public required string Status { get; set; } // Draft, PendingApproval, Approved, Rejected, Submitted, Withdrawn
        public List<ApprovalStepDto> ApprovalSteps { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public required string CreatedBy { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public required string CurrentApprover { get; set; }
        public int CurrentStep { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<string> Comments { get; set; } = new();
        public List<DocumentDto> SupportingDocuments { get; set; } = new();
        public bool IsExpedited { get; set; }
    }

    public class ApprovalStepDto
    {
        public int StepNumber { get; set; }
        public required string ApproverRole { get; set; }
        public required string ApproverUserId { get; set; }
        public required string Status { get; set; } // Pending, Approved, Rejected, Skipped
        public DateTime? ActionDate { get; set; }
        public string? Comments { get; set; }
        public bool IsRequired { get; set; }
        public int RequiredSignatureCount { get; set; } // For multi-sig approval
        public List<SignatureDto> Signatures { get; set; } = new();
    }

    public class SignatureDto
    {
        public required string UserId { get; set; }
        public required string UserName { get; set; }
        public required string Role { get; set; }
        public DateTime SignedAt { get; set; }
        public required string IpAddress { get; set; }
        public required string SignatureHash { get; set; } // Digital signature verification
    }

    public class DocumentDto
    {
        public required string DocumentId { get; set; }
        public required string FileName { get; set; }
        public required string FileType { get; set; }
        public long FileSizeBytes { get; set; }
        public DateTime UploadedAt { get; set; }
        public required string UploadedBy { get; set; }
        public required string StoragePath { get; set; }
        public required string DocumentHash { get; set; } // For integrity checking
        public bool IsConfidential { get; set; }
    }

    public class VoteAutomationRuleDto
    {
        public required string RuleId { get; set; }
        public required string ClientId { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public List<VoteTriggerDto> Triggers { get; set; } = new();
        public required VoteActionDto Action { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public required string CreatedBy { get; set; }
        public DateTime? LastTriggeredAt { get; set; }
        public int TriggerCount { get; set; }
        public required string LastResult { get; set; }
    }

    public class VoteTriggerDto
    {
        public required string TriggerId { get; set; }
        public required string Type { get; set; } // ProposalTag, ProposerAddress, TokenThreshold, etc.
        public required string Condition { get; set; } // Contains, Equals, GreaterThan, etc.
        public required string Value { get; set; }
        public bool IsRequired { get; set; }
    }

    public class VoteActionDto
    {
        public required string ActionType { get; set; } // Vote, Notify, Delegate, Abstain
        public required string VoteDecision { get; set; } // For, Against, Abstain
        public decimal VotingPowerPercentage { get; set; } // % of available power to use
        public string? NotifyUserIds { get; set; } // Comma-separated user IDs to notify
        public string? DelegateToAddress { get; set; } // Address to delegate to
        public string? CustomMessage { get; set; } // For notifications
        public bool RequiresReview { get; set; }
    }

    public class GovernanceReportDto
    {
        public required string ReportId { get; set; }
        public required string ClientId { get; set; }
        public required string ReportType { get; set; } // Monthly, Quarterly, Annual, Ad-hoc
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public DateTime GeneratedAt { get; set; }
        public int ProposalsParticipatedIn { get; set; }
        public int ProposalsCreated { get; set; }
        public decimal AverageParticipationRate { get; set; }
        public Dictionary<string, int> VoteBreakdown { get; set; } = new(); // For/Against/Abstain count
        public List<string> SignificantProposals { get; set; } = new();
        public Dictionary<string, decimal> GovernanceTokensHeld { get; set; } = new();
        public decimal GovernanceInfluenceScore { get; set; } // 0-100
        public List<GovernanceActionDto> RecommendedActions { get; set; } = new();
        public required string ReportUrl { get; set; }
    }

    public class GovernanceActionDto
    {
        public required string ActionId { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string Priority { get; set; } // High, Medium, Low
        public required string Category { get; set; }
        public required string Benefit { get; set; }
        public decimal EstimatedImpact { get; set; } // 1-10
        public List<string> PrerequisiteActions { get; set; } = new();
    }
}