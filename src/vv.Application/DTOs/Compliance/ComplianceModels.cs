using System;
using System.Collections.Generic;

namespace vv.Application.DTOs.Compliance
{
    public class ComplianceReportDto
    {
        public DateTime GeneratedAt { get; set; }
        public required string PoolId { get; set; }
        public List<RegulatoryCheckDto> RegulatoryChecks { get; set; } = new();
        public List<AMLAlertDto> AmlAlerts { get; set; } = new();
        public List<TransactionMonitoringDto> FlaggedTransactions { get; set; } = new();
        public Dictionary<string, bool> JurisdictionalCompliance { get; set; } = new(); // Region → compliant?
        public List<LicenseDto> ActiveLicenses { get; set; } = new();
        public DateTime NextAuditDue { get; set; }
        public required string ComplianceOfficer { get; set; }
        public bool IsCompliant { get; set; }
    }

    public class RegulatoryCheckDto
    {
        public required string CheckId { get; set; }
        public required string Regulation { get; set; } // GDPR, AML, KYC, etc.
        public required string Jurisdiction { get; set; }
        public required string Status { get; set; } // Passed, Failed, Pending
        public DateTime CheckedAt { get; set; }
        public required string Description { get; set; }
        public List<string> EvidenceUrls { get; set; } = new();
        public required string ResponsiblePerson { get; set; }
        public DateTime NextCheckDue { get; set; }
    }

    public class AMLAlertDto
    {
        public required string AlertId { get; set; }
        public required string Type { get; set; } // Suspicious Transaction, High Risk, etc.
        public required string ClientId { get; set; }
        public required string TransactionId { get; set; }
        public DateTime DetectedAt { get; set; }
        public required string Description { get; set; }
        public decimal Amount { get; set; }
        public required string Asset { get; set; }
        public required string Status { get; set; } // Open, Investigating, Closed, Reported
        public required string AssignedTo { get; set; }
        public bool ReportedToAuthorities { get; set; }
    }

    public class TransactionMonitoringDto
    {
        public required string MonitoringId { get; set; }
        public required string TransactionId { get; set; }
        public required string Flag { get; set; } // Size, Frequency, Pattern, etc.
        public required string Description { get; set; }
        public decimal RiskScore { get; set; }
        public bool RequiresReview { get; set; }
        public required string ReviewedBy { get; set; }
        public DateTime ReviewedAt { get; set; }
        public required string Resolution { get; set; }
    }

    public class LicenseDto
    {
        public required string LicenseId { get; set; }
        public required string Type { get; set; } // MSB, Virtual Asset Provider, etc.
        public required string Jurisdiction { get; set; }
        public DateTime IssuedDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public required string Status { get; set; } // Active, Pending, Expired
        public required string IssuingAuthority { get; set; }
        public List<string> CoveredActivities { get; set; } = new();
        public required string DocumentUrl { get; set; }
    }

    public class KYCStatusDto
    {
        public required string DocumentType { get; set; }
        public required string Status { get; set; } // Verified, Pending, Rejected
        public DateTime SubmissionDate { get; set; }
        public DateTime VerificationDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public required string VerifiedBy { get; set; }
        public List<string> Notes { get; set; } = new();
    }

    public class TransactionScreeningDto
    {
        public required string ScreeningId { get; set; }
        public required string TransactionId { get; set; }
        public DateTime ScreenedAt { get; set; }
        public bool IsClean { get; set; }
        public List<SanctionHitDto> SanctionHits { get; set; } = new();
        public List<RiskIndicatorDto> RiskIndicators { get; set; } = new();
        public required string BlockchainAnalysisProvider { get; set; }
        public required string ScreeningResult { get; set; }
        public decimal RiskScore { get; set; }
        public required string Action { get; set; } // Allowed, Blocked, Pending Review
    }

    public class SanctionHitDto
    {
        public required string SanctionListName { get; set; }
        public required string EntityName { get; set; }
        public required string EntityType { get; set; } // Person, Organization, Address
        public required string MatchType { get; set; } // Exact, Partial, Fuzzy
        public decimal MatchScore { get; set; } // 0-100
        public required string SanctioningBody { get; set; } // OFAC, UN, EU, etc.
        public DateTime ListUpdatedDate { get; set; }
    }

    public class RiskIndicatorDto
    {
        public required string IndicatorType { get; set; }
        public required string Description { get; set; }
        public decimal Severity { get; set; } // 1-10
        public required string DataSource { get; set; }
        public DateTime DetectedAt { get; set; }
        public List<string> RelatedEntities { get; set; } = new();
    }
}