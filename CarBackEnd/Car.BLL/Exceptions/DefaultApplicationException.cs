﻿using System;
using System.Runtime.Serialization;
using Car.BLL.Dto;

namespace Car.BLL.Exceptions
{
    [Serializable]
    public class DefaultApplicationException : Exception
    {
        public int StatusCode { get; set; }

        public Severity Severity { get; set; }

        public DefaultApplicationException(string message)
            : base(message)
        {
        }

        private DefaultApplicationException()
            : base()
        {
        }

        public new virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("Error", StatusCode);
        }
    }
}
