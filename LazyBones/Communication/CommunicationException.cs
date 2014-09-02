﻿using System;
using System.Runtime.Serialization;

namespace LazyBones.Communication
{
    [Serializable]
    public class CommunicationException : Exception
    {
        public CommunicationException()
        {
        }

        public CommunicationException(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {
        }

        public CommunicationException(string message)
            : base(message)
        {
        }

        public CommunicationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
