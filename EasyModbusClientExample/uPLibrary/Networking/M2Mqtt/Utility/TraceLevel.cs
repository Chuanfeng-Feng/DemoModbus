﻿// Decompiled with JetBrains decompiler
// Type: uPLibrary.Networking.M2Mqtt.Utility.TraceLevel
// Assembly: EasyModbusClientExample, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 92ADC808-D80B-41C0-B9AB-216E9E70F2AD
// Assembly location: D:\hvu\EasyModbusTCP .NET Package V5.0\EasyModbusTCP .NET Package V5.0\EasyModbusClient (.NET Version)\EasyModbusClient (.NET Version)\EasyModbusClientExample.exe

namespace uPLibrary.Networking.M2Mqtt.Utility
{
  public enum TraceLevel
  {
    Error = 1,
    Warning = 2,
    Information = 4,
    Verbose = 15, // 0x0000000F
    Frame = 16, // 0x00000010
    Queuing = 32, // 0x00000020
  }
}
