using System;
using System.Collections.Generic;
using vv.Core.Security;

namespace vv.Application.DTOs.Wallet
{
    public abstract class BaseWalletDto
    {
        public string WalletId { get; set; }
        public string Name { get; set; }
        public Dictionary<string, decimal> Balances { get; set; } = new();
        public decimal TotalValueUsd { get; set; }
        public List<WalletAddressDto> Addresses { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } // Active, Frozen, Closed
        public List<WalletTransactionDto> RecentTransactions { get; set; } = new();
        public List<string> Tags { get; set; } = new();
    }

    public class StandardWalletDto : BaseWalletDto
    {
        public string UserId { get; set; }
        public bool HasRecoveryMethod { get; set; }
        public string RecoveryType { get; set; } // Email, Phone, Social, Hardware
        public DateTime LastBackupDate { get; set; }
        public List<string> LinkedExchanges { get; set; } = new();
        public SecuritySettingsDto SecuritySettings { get; set; }
    }

    public class CorporateWalletDto : BaseWalletDto
    {
        public string CorporateClientId { get; set; }
        public MultiSigConfigDto MultiSigConfig { get; set; }
        public List<WalletPermissionDto> UserPermissions { get; set; } = new();
        public List<WalletPolicyDto> Policies { get; set; } = new();
        public List<TransactionApprovalWorkflowDto> ApprovalWorkflows { get; set; } = new();
        public List<ExternalIntegrationDto> ExternalIntegrations { get; set; } = new();
        public bool HasTreasuryIntegration { get; set; }
        public AuditConfigDto AuditConfig { get; set; }
        public ComplianceSettingsDto ComplianceSettings { get; set; }
    }

    public class WalletAddressDto
    {
        public string AddressId { get; set; }
        public string Address { get; set; }
        public string Network { get; set; } // Ethereum, Bitcoin, etc.
        public string AddressType { get; set; } // Hot, Cold, MultiSig, etc.
        public string Label { get; set; }
        public bool IsDefault { get; set; }
        public Dictionary<string, decimal> Balances { get; set; } = new();
        public DateTime LastUsed { get; set; }
        public string Status { get; set; } // Active, Archived
        public int AddressIndex { get; set; } // For HD wallets
        public List<string> AllowedOperations { get; set; } = new(); // Deposit, Withdrawal, etc.
    }

    public class MultiSigConfigDto
    {
        public int RequiredSignatures { get; set; }
        public int TotalSigners { get; set; }
        public List<SignerDto> Signers { get; set; } = new();
        public string MultiSigType { get; set; } // Gnosis Safe, MPC, Smart Contract, etc.
        public string ContractAddress { get; set; } // If smart contract-based
        public Dictionary<string, int> ThresholdsByAmount { get; set; } = new(); // Amount → required signatures
        public List<RoleThresholdDto> RoleThresholds { get; set; } = new();
        public string RecoveryMethod { get; set; }
        public List<SignerDto> RecoverySigners { get; set; } = new();
    }

    public class SignerDto
    {
        public string SignerId { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string PublicKey { get; set; }
        public DateTime AddedAt { get; set; }
        public bool IsActive { get; set; }
        public string DeviceInfo { get; set; }
        public List<string> SigningMethods { get; set; } = new(); // Hardware, Mobile, Email, etc.
        public List<SignerLimitDto> TransactionLimits { get; set; } = new();
    }

    public class SignerLimitDto
    {
        public string AssetType { get; set; } // Specific asset or "Any"
        public decimal DailyLimit { get; set; }
        public decimal SingleTransactionLimit { get; set; }
        public bool RequiresAdditionalApproval { get; set; }
        public List<string> ApproverRoles { get; set; } = new();
    }

    public class RoleThresholdDto
    {
        public string Role { get; set; }
        public decimal AmountThreshold { get; set; }
        public int RequiredSignatures { get; set; }
    }

    public class WalletPermissionDto
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public List<string> AllowedOperations { get; set; } = new(); // View, Initiate, Approve, etc.
        public Dictionary<string, decimal> TransactionLimits { get; set; } = new(); // Asset → amount
        public bool CanManageUsers { get; set; }
        public bool CanViewBalances { get; set; }
        public bool CanInitiateTransactions { get; set; }
        public bool CanApproveTransactions { get; set; }
        public List<string> AllowedAddresses { get; set; } = new();
        public List<string> RestrictedAddresses { get; set; } = new();
    }

    public class WalletPolicyDto
    {
        public string PolicyId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; } // Spending, Whitelist, Time-lock, etc.
        public Dictionary<string, object> Parameters { get; set; } = new();
        public bool IsEnabled { get; set; }
        public string EnforcementType { get; set; } // Hard (reject), Soft (warn)
        public List<string> ExemptRoles { get; set; } = new();
        public DateTime EffectiveFrom { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    public class TransactionApprovalWorkflowDto
    {
        public string WorkflowId { get; set; }
        public string Name { get; set; }
        public decimal AmountThreshold { get; set; }
        public string AssetType { get; set; } // Specific asset or "Any"
        public List<ApprovalStepDto> ApprovalSteps { get; set; } = new();
        public int MaxApprovalTimeHours { get; set; }
        public bool RequiresJustification { get; set; }
        public bool CanBeExpedited { get; set; }
        public List<string> ExpeditingRoles { get; set; } = new();
    }

    public class WalletTransactionDto
    {
        public required string TransactionId { get; set; }
        public required string Type { get; set; } // Send, Receive, Swap, Stake, etc.
        public required string Status { get; set; } // Pending, Approved, Rejected, Completed, Failed
        public required string FromAddress { get; set; }
        public required string ToAddress { get; set; }
        public required string Asset { get; set; }
        public decimal Amount { get; set; }
        public decimal FeeAmount { get; set; }
        public required string FeeAsset { get; set; }
        public DateTime InitiatedAt { get; set; }
        public required string InitiatedBy { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? TransactionHash { get; set; }
        public int Confirmations { get; set; }
        public List<TransactionApprovalDto> Approvals { get; set; } = new();
        public string? Memo { get; set; }
        public string? Purpose { get; set; } // Business context
        public Dictionary<string, string> Metadata { get; set; } = new();
        public bool IsOnChainVerified { get; set; }
    }

    public class TransactionApprovalDto
    {
        public required string ApprovalId { get; set; }
        public required string UserId { get; set; }
        public required string UserName { get; set; }
        public required string Role { get; set; }
        public required string Decision { get; set; } // Approved, Rejected, Pending
        public DateTime? ActionTime { get; set; }
        public string? Comments { get; set; }
        public required string ApprovalMethod { get; set; } // App, Email, Hardware, etc.
        public string? SignatureHash { get; set; }
        public string? IpAddress { get; set; }
    }

    public class SecuritySettingsDto
    {
        public bool TwoFactorEnabled { get; set; }
        public string? TwoFactorType { get; set; } // App, SMS, Email, Hardware - required only when TwoFactorEnabled is true
        public bool BiometricEnabled { get; set; }
        public bool EmailNotificationsEnabled { get; set; }
        public bool PushNotificationsEnabled { get; set; }
        public List<string> WhitelistedIPs { get; set; } = new();
        public List<string> WhitelistedAddresses { get; set; } = new();
        public bool HasWithdrawalDelay { get; set; }
        public int WithdrawalDelayHours { get; set; }
        public decimal LargeTransactionThreshold { get; set; }
    }

    public class ExternalIntegrationDto
    {
        public required string IntegrationId { get; set; }
        public required string Type { get; set; } // ERP, Accounting, Banking, etc.
        public required string Provider { get; set; } // Name of the provider
        public required string Status { get; set; } // Active, Inactive, Error
        public DateTime LastSyncTime { get; set; }
        public Dictionary<string, string> Configuration { get; set; } = new();
        public List<string> SyncedDataTypes { get; set; } = new();
        public required string SyncFrequency { get; set; } // Real-time, Hourly, Daily, etc.
        public required MaskedString ApiKeyHash { get; set; } // Protected secret - always masked in serialization and ToString()
    }

    public class AuditConfigDto
    {
        public bool ComprehensiveLogging { get; set; }
        public int LogRetentionDays { get; set; }
        public bool ExportToComplianceSystem { get; set; }
        public required string ComplianceSystemName { get; set; }
        public List<string> AuditedActions { get; set; } = new();
        public bool CaptureIpAddress { get; set; }
        public bool CaptureDeviceInfo { get; set; }
        public bool CaptureGeoLocation { get; set; }
        public required string AuditFileFormat { get; set; } // CSV, JSON, etc.
        public required string AuditStorageLocation { get; set; }
    }

    public class ComplianceSettingsDto
    {
        public bool AutoScreenTransactions { get; set; }
        public List<string> SanctionLists { get; set; } = new();
        public bool BlockHighRiskCountries { get; set; }
        public List<string> HighRiskCountryCodes { get; set; } = new();
        public decimal LargeTransactionThresholdUsd { get; set; }
        public bool RequirePurposeForLargeTransactions { get; set; }
        public bool PerformBlockchainAnalysis { get; set; }
        public string? BlockchainAnalysisProvider { get; set; } // Required only when PerformBlockchainAnalysis is true
        public int MaxRiskScoreAllowed { get; set; } // 1-100
        public List<string> TransactionPurposeOptions { get; set; } = new();
    }
}