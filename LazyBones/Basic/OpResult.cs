using System;
using System.Runtime.Serialization;

namespace LazyBones.Basic
{
    /// <summary>
    /// 表示操作的结果
    /// </summary>
    [Serializable]
    [DataContract]
    public class OpResult
    {
        public OpResult() { }
        public OpResult(bool isSuccess, string message)
        {
            IsSuccess = isSuccess;
            Message = message;
        }
        /// <summary>
        /// 获取或设置操作是否成功
        /// </summary>
        [DataMember]
        public bool IsSuccess { get; set; }
        /// <summary>
        /// 获取或设置操作的信息
        /// </summary>
        [DataMember]
        public string Message { get; set; }
    }
}
