using System;
using System.Collections.Generic;

namespace vv.Application.DTOs.Portfolio.BlackLitterman
{
    public class AIGeneratedViewDto
    {
        public required string ViewId { get; set; }
        public required string ModelId { get; set; } // Which AI model generated this view
        public required string ModelVersion { get; set; }
        public DateTime GeneratedAt { get; set; }
        public List<ViewAssetWeightDto> AssetWeights { get; set; } = new();
        public decimal ExpectedReturn { get; set; }
        public decimal ModelConfidence { get; set; } // 0-1, AI's own assessment of confidence
        public required string RationaleText { get; set; }
        public Dictionary<string, decimal> FactorContributions { get; set; } = new(); // Factors affecting this view
        public List<string> DataSourcesUsed { get; set; } = new();
        public Dictionary<string, object> ModelParameters { get; set; } = new();
        public decimal BacktestAccuracy { get; set; } // Historical accuracy of similar predictions
        public List<AlternativeViewDto> AlternativeViews { get; set; } = new();
        public required string BlockchainVerificationHash { get; set; }
        public required string IpfsContentId { get; set; } // For storing detailed model output
    }

    public class AlternativeViewDto
    {
        public required string AlternativeId { get; set; }
        public List<ViewAssetWeightDto> AssetWeights { get; set; } = new();
        public decimal ExpectedReturn { get; set; }
        public decimal Probability { get; set; } // How likely this alternative is
        public required string Scenario { get; set; } // Description of market conditions for this view
    }

    public class BlockchainModelVerificationDto
    {
        public required string VerificationId { get; set; }
        public required string ModelRunId { get; set; }
        public DateTime ExecutedAt { get; set; }
        public required string InputDataHash { get; set; } // Hash of all input parameters
        public required string OutputDataHash { get; set; } // Hash of results
        public required string BlockchainNetworkUsed { get; set; }
        public required string TransactionHash { get; set; }
        public int BlockNumber { get; set; }
        public required string BlockExplorerUrl { get; set; }
        public required string SmartContractAddress { get; set; }
        public required string VerifierAddress { get; set; }
        public required string ExecutorAddress { get; set; }
        public required string VerificationStatus { get; set; } // Pending, Verified, Failed
        public DateTime VerifiedAt { get; set; }
        public List<ModelInputReferenceDto> InputReferences { get; set; } = new();
        public required string ProofOfExecutionHash { get; set; }
    }

    public class ModelInputReferenceDto
    {
        public required string ReferenceId { get; set; }
        public required string DataType { get; set; } // Market Data, Views, Parameters, etc.
        public required string StorageType { get; set; } // IPFS, Blockchain, Database
        public required string StorageReference { get; set; } // IPFS hash, tx hash, or database ID
        public required string DataDescription { get; set; }
        public DateTime StoredAt { get; set; }
        public bool IsPubliclyAccessible { get; set; }
        public required string AccessMethod { get; set; } // API endpoint, query, etc.
    }

    public class ModelExecutionDto
    {
        public required string ExecutionId { get; set; }
        public required string ModelType { get; set; } // "BlackLitterman", "RiskParity", etc.
        public required string ExecutedBy { get; set; } // User or system ID
        public DateTime StartedAt { get; set; }
        public DateTime CompletedAt { get; set; }
        public required string Status { get; set; } // Running, Completed, Failed
        public required string ErrorMessage { get; set; }
        public Dictionary<string, string> InputParameterHashes { get; set; } = new();
        public required string ResultHash { get; set; }
        public required string ExecutionEnvironment { get; set; } // Cloud, Local, etc.
        public Dictionary<string, object> PerformanceMetrics { get; set; } = new(); // Execution time, resource usage
        public required string ModelVersion { get; set; }
        public required string CodeRepositoryReference { get; set; } // Git commit hash
        public required BlockchainModelVerificationDto Verification { get; set; }
    }

    public class ModelRegistryDto
    {
        public required string ModelId { get; set; }
        public required string Name { get; set; }
        public required string Version { get; set; }
        public required string Type { get; set; } // BlackLitterman, ML Forecasting, etc.
        public required string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public required string CreatedBy { get; set; }
        public required string Status { get; set; } // Draft, Testing, Production, Deprecated
        public List<string> SupportedAssetClasses { get; set; } = new();
        public Dictionary<string, object> DefaultParameters { get; set; } = new();
        public decimal BacktestSharpeRatio { get; set; }
        public decimal BacktestAlpha { get; set; }
        public required string ValidationMethodology { get; set; }
        public List<string> ApprovedForClientTypes { get; set; } = new(); // Retail, Institutional, etc.
        public required string BlockchainRegistryAddress { get; set; }
        public required string RegistrationTransactionHash { get; set; }
        public required string SourceCodeReference { get; set; } // IPFS hash or GitHub reference
    }
}