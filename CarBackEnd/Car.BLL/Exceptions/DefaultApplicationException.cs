﻿using System;
using System.Runtime.Serialization;
using Car.BLL.Dto;

namespace Car.BLL.Exceptions
{
    [Serializable]
    public class DefaultApplicationException : Exception, ISerializable
    {
        public int StatusCode { get; set; }

        public Severity Severity { get; set; }

        public DefaultApplicationException()
        {
        }

        public DefaultApplicationException(string message)
            : base(message)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
