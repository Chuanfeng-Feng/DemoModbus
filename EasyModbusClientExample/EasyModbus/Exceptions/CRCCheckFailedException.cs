﻿// Decompiled with JetBrains decompiler
// Type: EasyModbus.Exceptions.CRCCheckFailedException
// Assembly: EasyModbusClientExample, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 92ADC808-D80B-41C0-B9AB-216E9E70F2AD
// Assembly location: D:\hvu\EasyModbusTCP .NET Package V5.0\EasyModbusTCP .NET Package V5.0\EasyModbusClient (.NET Version)\EasyModbusClient (.NET Version)\EasyModbusClientExample.exe

using System;
using System.Runtime.Serialization;

namespace EasyModbus.Exceptions
{
  public class CRCCheckFailedException : ModbusException
  {
    public CRCCheckFailedException()
    {
    }

    public CRCCheckFailedException(string message)
      : base(message)
    {
    }

    public CRCCheckFailedException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    protected CRCCheckFailedException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
