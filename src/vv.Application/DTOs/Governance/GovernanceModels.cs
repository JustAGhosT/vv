using System;
using System.Collections.Generic;

namespace vv.Application.DTOs.Governance
{
    public class GovernanceProposalDto
    {
        public required string ProposalId { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string Author { get; set; }
        public required string AuthorAddress { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime VotingStartsAt { get; set; }
        public DateTime VotingEndsAt { get; set; }
        public required string Status { get; set; } // Draft, Active, Passed, Failed, Executed, Cancelled
        public List<ProposalActionDto> Actions { get; set; } = new();
        public VotingResultsDto? CurrentResults { get; set; }
        public Dictionary<string, decimal> TokenThresholds { get; set; } = new(); // Token → threshold
        public int QuorumPercentage { get; set; }
        public int ApprovalThresholdPercentage { get; set; }
        public List<string> Tags { get; set; } = new();
        public List<ProposalDiscussionDto> Discussions { get; set; } = new();
        public List<string> RelatedProposals { get; set; } = new();
        public Dictionary<string, string> Metadata { get; set; } = new(); // Custom metadata
        public string? IpfsHash { get; set; } // For permanent storage
    }

    public class ProposalActionDto
    {
        public string ActionId { get; set; }
        public string Type { get; set; } // ParameterChange, FundTransfer, CodeExecution, etc.
        public string Target { get; set; } // Contract/system to affect
        public string Function { get; set; } // Function to call
        public Dictionary<string, object> Parameters { get; set; } = new();
        public string Description { get; set; }
        public string ExecutionStatus { get; set; } // Pending, Executed, Failed
        public string TransactionHash { get; set; }
        public DateTime? ExecutedAt { get; set; }
        public decimal EstimatedGasCost { get; set; }
        public bool RequiresTimelock { get; set; }
        public int TimelockDuration { get; set; } // In hours
    }

    public class VotingResultsDto
    {
        public decimal VotesFor { get; set; }
        public decimal VotesAgainst { get; set; }
        public decimal VotesAbstain { get; set; }
        public int ParticipantsCount { get; set; }
        public decimal ParticipationRate { get; set; } // % of total voting power
        public decimal CurrentApprovalRate { get; set; } // % of votes in favor
        public bool QuorumReached { get; set; }
        public bool ThresholdReached { get; set; }
        public Dictionary<string, decimal> VotesByToken { get; set; } = new(); // Token → voting power
        public List<VoteBreakdownDto> TopVotes { get; set; } = new(); // Top influential votes
    }

    public class VoteDto
    {
        public string VoteId { get; set; }
        public string ProposalId { get; set; }
        public string VoterAddress { get; set; }
        public string VoterName { get; set; } // If known
        public string Vote { get; set; } // For, Against, Abstain
        public decimal VotingPower { get; set; }
        public DateTime CastAt { get; set; }
        public Dictionary<string, decimal> TokenBreakdown { get; set; } = new(); // Token → amount
        public string TransactionHash { get; set; }
        public string Reason { get; set; } // Optional voting reason
        public bool IsDelegated { get; set; }
        public string DelegatedFrom { get; set; } // If vote is from delegation
    }

    public class VoteBreakdownDto
    {
        public string VoterAddress { get; set; }
        public string VoterName { get; set; }
        public string Vote { get; set; }
        public decimal VotingPower { get; set; }
        public decimal PercentageOfTotal { get; set; }
    }

    public class DelegationDto
    {
        public string DelegatorAddress { get; set; }
        public string DelegatorName { get; set; }
        public string DelegateAddress { get; set; }
        public string DelegateName { get; set; }
        public decimal Amount { get; set; }
        public string TokenSymbol { get; set; }
        public DateTime DelegatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string Status { get; set; } // Active, Expired, Revoked
        public string TransactionHash { get; set; }
    }

    public class ProposalDiscussionDto
    {
        public string CommentId { get; set; }
        public string ProposalId { get; set; }
        public string AuthorAddress { get; set; }
        public string AuthorName { get; set; }
        public string Content { get; set; }
        public DateTime PostedAt { get; set; }
        public int UpvoteCount { get; set; }
        public int DownvoteCount { get; set; }
        public List<ProposalDiscussionDto> Replies { get; set; } = new();
        public bool IsEdited { get; set; }
        public string ParentCommentId { get; set; }
    }

    public class GovernanceSettingsDto
    {
        public string GovernanceId { get; set; }
        public string Name { get; set; } // e.g., "Veritas Vault Governance"
        public Dictionary<string, decimal> ProposalThresholds { get; set; } = new(); // Token → amount needed to propose
        public int MinimumVotingPeriod { get; set; } // In hours
        public int MaximumVotingPeriod { get; set; } // In hours
        public int DefaultVotingPeriod { get; set; } // In hours
        public int QuorumPercentage { get; set; } // % of total voting power needed
        public int ApprovalThresholdPercentage { get; set; } // % of votes needed to pass
        public int ExecutionDelay { get; set; } // In hours after proposal passes
        public int ExecutionWindow { get; set; } // In hours to execute after passing
        public List<string> GovernanceTokens { get; set; } = new();
        public List<string> VotingStrategies { get; set; } = new(); // e.g., "token-weighted", "quadratic"
        public bool EmergencyProposalsEnabled { get; set; }
        public Dictionary<string, decimal> EmergencyThresholds { get; set; } = new();
        public List<string> RestrictedFunctions { get; set; } = new(); // Functions that can't be called via governance
    }

    public class VotingPowerDto
    {
        public string Address { get; set; }
        public string Name { get; set; } // If known
        public decimal TotalVotingPower { get; set; }
        public Dictionary<string, decimal> TokenBreakdown { get; set; } = new(); // Token → amount
        public List<DelegationDto> ReceivedDelegations { get; set; } = new();
        public List<DelegationDto> ActiveDelegations { get; set; } = new(); // Delegated to others
        public decimal TotalDelegatedToOthers { get; set; }
        public decimal TotalDelegatedFromOthers { get; set; }
        public List<VoteDto> RecentVotes { get; set; } = new();
        public Dictionary<string, decimal> VotingHistory { get; set; } = new(); // ProposalId → voting power
        public decimal ParticipationRate { get; set; } // % of eligible votes cast
    }

    public class GovernanceDashboardDto
    {
        public List<GovernanceProposalDto> ActiveProposals { get; set; } = new();
        public List<GovernanceProposalDto> RecentlyClosedProposals { get; set; } = new();
        public List<VotingPowerDto> TopVoters { get; set; } = new();
        public Dictionary<string, decimal> TotalVotingPower { get; set; } = new(); // Token → total voting power
        public int ProposalCount { get; set; }
        public int PassedProposalCount { get; set; }
        public int FailedProposalCount { get; set; }
        public decimal AverageParticipationRate { get; set; }
        public List<GovernanceMetricDto> ParticipationTrend { get; set; } = new(); // Historical participation
        public Dictionary<string, int> ProposalsByCategory { get; set; } = new(); // Tag → count
    }

    public class GovernanceMetricDto
    {
        public DateTime Date { get; set; }
        public decimal ParticipationRate { get; set; }
        public int ActiveProposals { get; set; }
        public int UniqueVoters { get; set; }
        public decimal TotalVotesCast { get; set; }
        public Dictionary<string, decimal> TokenDistribution { get; set; } = new(); // Top 10 holders' %
    }

    public class CorporateGovernanceRightsDto
    {
        public string ClientId { get; set; }
        public string CompanyName { get; set; }
        public decimal TotalVotingPower { get; set; }
        public decimal VotingPowerPercentage { get; set; } // % of total system voting power
        public List<string> DelegatedAddresses { get; set; } = new(); // Addresses that can vote on behalf
        public Dictionary<string, VotingPermissionDto> UserVotingRights { get; set; } = new();
        public bool AutomaticVoteEnabled { get; set; }
        public string DefaultVoteStrategy { get; set; } // e.g., "Always Abstain", "Follow Recommendation"
        public Dictionary<string, string> CategoryVotePreferences { get; set; } = new(); // Category → default vote
        public decimal MinimumProposalThreshold { get; set; } // Minimum % of assets to create proposal
        public List<string> AuthorizedProposalCategories { get; set; } = new(); // Categories client can propose for
    }

    public class VotingPermissionDto
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public List<string> AllowedVoteTypes { get; set; } = new(); // For, Against, Abstain
        public decimal VotingPowerLimit { get; set; } // Max % of company voting power
        public bool RequiresApproval { get; set; }
        public List<string> ApproverUserIds { get; set; } = new();
        public List<string> AllowedCategories { get; set; } = new(); // Categories they can vote on
        public bool CanCreateProposals { get; set; }
        public bool CanDelegate { get; set; }
    }
}