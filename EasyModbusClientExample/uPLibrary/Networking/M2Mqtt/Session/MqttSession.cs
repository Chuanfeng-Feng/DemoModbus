﻿// Decompiled with JetBrains decompiler
// Type: uPLibrary.Networking.M2Mqtt.Session.MqttSession
// Assembly: EasyModbusClientExample, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 92ADC808-D80B-41C0-B9AB-216E9E70F2AD
// Assembly location: D:\hvu\EasyModbusTCP .NET Package V5.0\EasyModbusTCP .NET Package V5.0\EasyModbusClient (.NET Version)\EasyModbusClient (.NET Version)\EasyModbusClientExample.exe

using System.Collections;

namespace uPLibrary.Networking.M2Mqtt.Session
{
  public abstract class MqttSession
  {
    public string ClientId { get; set; }

    public Hashtable InflightMessages { get; set; }

    public MqttSession()
      : this((string) null)
    {
    }

    public MqttSession(string clientId)
    {
      this.ClientId = clientId;
      this.InflightMessages = new Hashtable();
    }

    public virtual void Clear()
    {
      this.ClientId = (string) null;
      this.InflightMessages.Clear();
    }
  }
}
