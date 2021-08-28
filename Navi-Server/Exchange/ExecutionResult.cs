using System.Diagnostics.CodeAnalysis;

namespace Navi_Server.Exchange
{
    /// <summary>
    /// Execution Result Type => For exchanging execution information each logical layer.
    /// </summary>
    public enum ExecutionResultType
    {
        /// <summary>
        /// When Execution succeeds.
        /// </summary>
        SUCCESS,
        
        /// <summary>
        /// When Duplicated ID exists on Database. Meaning saving information on db failed.
        /// </summary>
        DuplicatedID,
        
        /// <summary>
        ///  When Unknown Error occurred.
        /// </summary>
        Unknown
    }
    
    /// <summary>
    /// Execution Result Data Class. This will hold information of return value, failed reason, etc.
    /// </summary>
    /// <typeparam name="T">Result return type. If no return type, just set to object.</typeparam>
    [ExcludeFromCodeCoverage]
    public class ExecutionResult<T>
    {
        /// <summary>
        /// Determiner for whether execution result is success, or failed with duplicated id, or else.
        /// </summary>
        public ExecutionResultType ResultType { get; set; }
        
        /// <summary>
        /// Return value if succeeds.
        /// </summary>
        public T Value { get; set; }
        
        /// <summary>
        /// Failure Message if needed.
        /// </summary>
        public string Message { get; set; }
    }
}