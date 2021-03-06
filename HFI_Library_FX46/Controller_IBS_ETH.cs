﻿// Decompiled with JetBrains decompiler
// Type: PhoenixContact.HFI.Inline.Controller_IBS_ETH
// Assembly: HFI_Library_FX46, Version=3.2.6053.23249, Culture=neutral, PublicKeyToken=bbf13850d99d956d
// MVID: 42FFD0DD-74E7-4B38-A116-483C52C5F352
// Assembly location: D:\Program Files (x86)\Phoenix Contact\HFI 3.2\HFI_Tools\Libraries\HFI_Library_FX46.dll

using PhoenixContact.Common.Ticker;
using PhoenixContact.DDI;
using PhoenixContact.PxC_Library.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

namespace PhoenixContact.HFI.Inline
{
  public class Controller_IBS_ETH : IController, IDisposable, IInterbusG4
  {
    private readonly object syncUserAccess = new object();
    private List<VarInput> _listInput = new List<VarInput>();
    private List<VarOutput> _listOutput = new List<VarOutput>();
    private string _ctrlName;
    private string _ctrlDescription;
    private string _connection;
    private string connectionDTI;
    private string connectionMXI;
    private bool _ctrlConnect;
    private bool _ctrlRun;
    private bool _ctrlError;
    private ControllerStartup _crtlStartup;
    private int _ctrlUpdateTimeDTI;
    private int _ctrlUpdateTimeMXI;
    private int _mxiSleep;
    private Diagnostic _diagnostic;
    private InterbusDiagnostic _ibsDiagnostic;
    private int _ibsDiagStatusReg;
    private int _ibsDiagParaReg;
    private DateTime readCycleTime;
    private WatchdogMonitoringTime _watchdogTimeout;
    private int _netfailTimeout;
    private bool _netfailDeactivate;
    private bool _ctrlNetfail;
    private Controller_IBS_ETH.Config _ctrlConfig;
    private byte[] in_buffer;
    private byte[] out_buffer;
    private byte[] recvData;
    private bool deleteOldServices;
    private int msgClientObjectID;
    private int minPD_AddressInput;
    private int maxPD_AddressInput;
    private int minPD_AddressOutput;
    private int maxPD_AddressOutput;
    private UpdateProcessDataHandler _hdOnUpdateIO;
    private UpdateMailboxHandler _hdOnUpdateMX;
    private ControllerEventHandler _hdOnRun;
    private ControllerEventHandler _hdOnStop;
    private ControllerEventHandler _hdOnConnect;
    private ControllerEventHandler _hdOnDisable;
    private IBS_G4_Drv _ctrlClass;
    private PhoenixContact.Common.Ticker.Ticker _mxTimer;
    private PhoenixContact.Common.Ticker.Ticker _ctrlStateTimer;
    private InterbusHandling _locBusHandling;
    private MessageClientList messageClientList;
    private Controller_IBS_ETH.CtrlState aktState;
    private Controller_IBS_ETH.CtrlState oldState;
    private bool disposed;

    public Controller_IBS_ETH(string Name)
    {
      this.in_buffer = new byte[0];
      this.out_buffer = new byte[0];
      this.recvData = new byte[0];
      if (string.IsNullOrEmpty(Name))
        Name = nameof (Controller_IBS_ETH);
      this._locBusHandling = new InterbusHandling(Name);
      this._locBusHandling.OnException += new ExceptionHandler(this._ctrlBus_OnException);
      this.messageClientList = new MessageClientList();
      this.AddObject(this._locBusHandling.MessageClient);
      this.deleteOldServices = true;
      this._diagnostic = new Diagnostic(Name);
      this._ibsDiagnostic = new InterbusDiagnostic(Name);
      this._ctrlName = Name;
      this._ctrlDescription = string.Empty;
      this._ctrlClass = new IBS_G4_Drv(this.Name);
      this._ctrlClass.OnEnable += new TickerHandler(this._ctrlClass_OnEnable);
      this._ctrlClass.OnTick += new TickerHandler(this._ctrlClass_OnTick);
      this._ctrlClass.OnDisable += new TickerHandler(this._ctrlClass_OnDisable);
      this._ctrlClass.OnDiagnostic += new TickerDiagnosticHandler(this._ctrlClass_OnDiagnostic);
      this._mxTimer = TickerFactory.Create(this.Name + "_MX", this._ctrlUpdateTimeMXI, ThreadPriority.Highest);
      this._mxTimer.OnTick += new TickerHandler(this._mxTimer_OnTick);
      this._mxTimer.OnDiagnostic += new TickerDiagnosticHandler(this._mxTimer_OnDiagnostic);
      this._ctrlConfig = new Controller_IBS_ETH.Config();
      this._ctrlStateTimer = TickerFactory.Create(this.Name + "_State", this._ctrlConfig.UpdateControllerState, ThreadPriority.Highest);
      this._ctrlStateTimer.OnTick += new TickerHandler(this._ctrlStateTimer_OnTick);
      this._ctrlStateTimer.OnDisable += new TickerHandler(this._ctrlStateTimer_OnDisable);
      this._ctrlStateTimer.OnDiagnostic += new TickerDiagnosticHandler(this._ctrlStateTimer_OnDiagnostic);
      this._crtlStartup = ControllerStartup.PhysicalConfiguration;
      this._watchdogTimeout = WatchdogMonitoringTime.Intervall_524ms;
      this.UpdateProcessDataCycleTime = 20;
      this.UpdateMailboxTime = 60;
      this.ChangeState(Controller_IBS_ETH.CtrlState.Idle);
    }

    public override string ToString()
    {
      return this._ctrlName;
    }

    private Controller_IBS_ETH.CtrlState GetState()
    {
      return this.aktState;
    }

    private Controller_IBS_ETH.CtrlState GetOldState()
    {
      return this.oldState;
    }

    private void ChangeState(Controller_IBS_ETH.CtrlState NewState)
    {
      if (this.aktState == NewState)
        return;
      this.oldState = this.aktState;
      if (this.aktState == Controller_IBS_ETH.CtrlState.Run && this._hdOnStop != null)
        this._hdOnStop((object) this);
      this.aktState = NewState;
      switch (this.aktState)
      {
        case Controller_IBS_ETH.CtrlState.Inactive:
        case Controller_IBS_ETH.CtrlState.Idle:
        case Controller_IBS_ETH.CtrlState.Disable:
        case Controller_IBS_ETH.CtrlState.GoingToDispose:
          this._ctrlRun = false;
          this._ctrlConnect = false;
          this._ctrlError = false;
          this._ctrlNetfail = false;
          break;
        case Controller_IBS_ETH.CtrlState.Connect:
          this._ctrlConnect = true;
          this._ctrlError = false;
          this._ctrlRun = false;
          this._ctrlNetfail = false;
          break;
        case Controller_IBS_ETH.CtrlState.Run:
          this._ctrlRun = true;
          this._ctrlConnect = true;
          this._ctrlError = false;
          this._ctrlNetfail = false;
          break;
        case Controller_IBS_ETH.CtrlState.Error:
          this._ctrlError = true;
          this._ctrlRun = false;
          this._ctrlConnect = false;
          break;
      }
    }

    public string VersionInfo
    {
      get
      {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine(this.GetType().Assembly.FullName);
        if (this._ctrlClass != null)
        {
          stringBuilder.AppendLine(this._ctrlClass.GetType().Assembly.FullName);
          if (this.GetState() == Controller_IBS_ETH.CtrlState.Connect || this.GetState() == Controller_IBS_ETH.CtrlState.Run)
          {
            stringBuilder.AppendLine("dn2ddi.dll, Version " + this._ctrlClass.GetVersionDn2DDI());
            PhoenixContact.DDI.VersionInfo versionDdi = this._ctrlClass.GetVersionDDI(VersionInfoType.DDI_Info);
            stringBuilder.Append(versionDdi.Name + ", Revision " + versionDdi.Revision);
          }
          else
            stringBuilder.Append("Driver version only in active controller state available.");
        }
        else
          stringBuilder.Append("Driver version only in active controller state available.");
        return stringBuilder.ToString();
      }
    }

    public string Name
    {
      get
      {
        return this._ctrlName;
      }
      set
      {
        if (string.IsNullOrEmpty(value))
        {
          this._ctrlName = nameof (Controller_IBS_ETH);
          this._diagnostic.Name = nameof (Controller_IBS_ETH);
        }
        else
        {
          this._ctrlName = value;
          this._diagnostic.Name = value;
        }
      }
    }

    public string Description
    {
      get
      {
        return this._ctrlDescription;
      }
      set
      {
        this._ctrlDescription = value;
      }
    }

    public ControllerStartup Startup
    {
      get
      {
        return this._crtlStartup;
      }
      set
      {
        this._crtlStartup = value;
      }
    }

    public int UpdateProcessDataCycleTime
    {
      get
      {
        return this._ctrlUpdateTimeDTI;
      }
      set
      {
        if (value < 5 || value > 200)
          return;
        this._ctrlUpdateTimeDTI = value;
      }
    }

    public int UpdateMailboxTime
    {
      get
      {
        return this._ctrlUpdateTimeMXI;
      }
      set
      {
        if (value < 10 || value > 1000)
          return;
        this._ctrlUpdateTimeMXI = value;
      }
    }

    public bool Connect
    {
      get
      {
        return this._ctrlConnect;
      }
    }

    public bool Run
    {
      get
      {
        return this._ctrlRun;
      }
    }

    public bool Error
    {
      get
      {
        return this._ctrlError;
      }
    }

    public string InternalState
    {
      get
      {
        return this.GetState().ToString();
      }
    }

    public ReadOnlyCollection<VarInput> InputObjectList
    {
      get
      {
        return this._listInput.AsReadOnly();
      }
    }

    public ReadOnlyCollection<VarOutput> OutputObjectList
    {
      get
      {
        return this._listOutput.AsReadOnly();
      }
    }

    public ReadOnlyCollection<MessageClient> MessageObjectList
    {
      get
      {
        return this.messageClientList.GetReadOnlyCollection();
      }
    }

    public bool WatchdogOccurred
    {
      get
      {
        return this._ctrlNetfail;
      }
    }

    public WatchdogMonitoringTime WatchdogTimeout
    {
      get
      {
        return this._watchdogTimeout;
      }
      set
      {
        this._watchdogTimeout = value;
      }
    }

    public bool WatchdogDeactivate
    {
      get
      {
        return this._netfailDeactivate;
      }
      set
      {
        if (this.GetState() != Controller_IBS_ETH.CtrlState.Idle && this.GetState() != Controller_IBS_ETH.CtrlState.Disable)
          return;
        this._netfailDeactivate = value;
      }
    }

    public bool WatchdogClear()
    {
      this._diagnostic.Throw((System.Enum) ControllerDiagnostic.NotSupported, PhoenixContact.PxC_Library.Util.Util.Int32ToByteArray(0, 2));
      this._diagnostic.Quit();
      return false;
    }

    public string Connection
    {
      get
      {
        return this._connection;
      }
      set
      {
        try
        {
          this._connection = !this._ctrlConfig.DNS_NameResolution ? IPAddress.Parse(value).ToString() : Dns.GetHostEntry(value).AddressList[0].ToString();
        }
        catch
        {
          this._connection = string.Empty;
          this.connectionDTI = (string) null;
          this.connectionMXI = (string) null;
          return;
        }
        if (this._connection.Length > 0)
        {
          this.connectionDTI = "IBETHIP[" + this._connection + "]N1_D";
          this.connectionMXI = "IBETHIP[" + this._connection + "]N1_M";
        }
        else
        {
          this._connection = string.Empty;
          this.connectionDTI = (string) null;
          this.connectionMXI = (string) null;
        }
      }
    }

    public bool Enable()
    {
      Ping ping = new Ping();
      lock (this.syncUserAccess)
      {
        if (this.GetState() != Controller_IBS_ETH.CtrlState.Disable)
        {
          if (this.GetState() != Controller_IBS_ETH.CtrlState.Idle)
            goto label_11;
        }
        this._ctrlConfig.WriteValue = false;
        this._diagnostic.Enable();
        this.ChangeState(Controller_IBS_ETH.CtrlState.GoingToEnable);
        if (ping.Send(this._connection, 1000).Status != IPStatus.Success)
        {
          this.StopOnError(ControllerDiagnostic.PingTimeout, 0, (Exception) null);
          return false;
        }
        if (this.connectionDTI == null || this.connectionMXI == null)
        {
          this.StopOnError(ControllerDiagnostic.NoValidConnectionString, 0, (Exception) null);
          return false;
        }
        this._ctrlClass.ConnectionDTI = this.connectionDTI;
        this._ctrlClass.ConnectionMXI = this.connectionMXI;
        this._ctrlClass.IB_Sync = false;
        this._ctrlClass.Intervall = this._ctrlUpdateTimeDTI;
        this._ctrlClass.Priority = ThreadPriority.Highest;
        Trace.WriteLine("*IDS_ETH Enable");
        this._ctrlClass.Enable();
        return true;
      }
label_11:
      return false;
    }

    public void Disable()
    {
      lock (this.syncUserAccess)
      {
        if (this.GetState() != Controller_IBS_ETH.CtrlState.Run && this.GetState() != Controller_IBS_ETH.CtrlState.Connect && this.GetState() != Controller_IBS_ETH.CtrlState.Error)
          return;
        this.ChangeState(Controller_IBS_ETH.CtrlState.GoingToDisable);
        this.DisableAll();
        Thread.Sleep(this._netfailTimeout + 100);
      }
    }

    public bool AutoStart()
    {
      switch (this._crtlStartup)
      {
        case ControllerStartup.PhysicalConfiguration:
          if (!this._ibsDiagnostic.StatusRegister.ACTIVE)
            this._locBusHandling.CreateConfiguration();
          if (!this._ibsDiagnostic.StatusRegister.RUN)
          {
            this._locBusHandling.StartDataTransfer();
            goto case ControllerStartup.WithoutConfiguration;
          }
          else
            goto case ControllerStartup.WithoutConfiguration;
        case ControllerStartup.LogicalConfiguration:
          if (!this._ibsDiagnostic.StatusRegister.ACTIVE)
            this._locBusHandling.ActivateConfiguration();
          if (!this._ibsDiagnostic.StatusRegister.RUN)
          {
            this._locBusHandling.StartDataTransfer();
            goto case ControllerStartup.WithoutConfiguration;
          }
          else
            goto case ControllerStartup.WithoutConfiguration;
        case ControllerStartup.WithoutConfiguration:
          return true;
        default:
          return false;
      }
    }

    public int AddObject(VarInput InputObject)
    {
      if (this.GetState() == Controller_IBS_ETH.CtrlState.Connect || this.GetState() == Controller_IBS_ETH.CtrlState.Run)
        return -10;
      if (InputObject == null)
        return -1;
      if (InputObject.BaseAddress < 0 || InputObject.VarType == VarType.Unknown)
        return -2;
      if (InputObject.ControllerAssigned)
        return -3;
      if (InputObject.BaseAddress < this.minPD_AddressInput || this._listInput.Count == 0)
        this.minPD_AddressInput = InputObject.BaseAddress;
      if (InputObject.BaseAddress + InputObject.ByteLength >= this.maxPD_AddressInput)
        this.maxPD_AddressInput = InputObject.BaseAddress + InputObject.ByteLength;
      this._listInput.Add(InputObject);
      InputObject.AssignController(this.Name);
      this._listInput.Sort();
      return 0;
    }

    public bool RemoveObject(VarInput InputObject)
    {
      if (this.GetState() == Controller_IBS_ETH.CtrlState.Connect || this.GetState() == Controller_IBS_ETH.CtrlState.Run || InputObject == null)
        return false;
      this._listInput.Remove(InputObject);
      if (this._listInput.Count == 0)
      {
        this.minPD_AddressInput = 0;
        this.maxPD_AddressInput = 0;
      }
      else
      {
        this.minPD_AddressInput = int.MaxValue;
        this.maxPD_AddressInput = 0;
        foreach (VarInput varInput in this._listInput)
        {
          if (varInput.BaseAddress < this.minPD_AddressInput)
            this.minPD_AddressInput = varInput.BaseAddress;
          if (varInput.BaseAddress + varInput.ByteLength >= this.maxPD_AddressInput)
            this.maxPD_AddressInput = varInput.BaseAddress + varInput.ByteLength;
        }
      }
      InputObject.AssignController(string.Empty);
      return true;
    }

    public int AddObject(VarOutput OutputObject)
    {
      if (this.GetState() == Controller_IBS_ETH.CtrlState.Connect || this.GetState() == Controller_IBS_ETH.CtrlState.Run)
        return -10;
      if (OutputObject == null)
        return -1;
      if (OutputObject.BaseAddress < 0 || OutputObject.VarType == VarType.Unknown)
        return -2;
      if (OutputObject.ControllerAssigned)
        return -3;
      if (OutputObject.BaseAddress < this.minPD_AddressOutput || this._listOutput.Count == 0)
        this.minPD_AddressOutput = OutputObject.BaseAddress;
      if (OutputObject.BaseAddress + OutputObject.ByteLength >= this.maxPD_AddressOutput)
        this.maxPD_AddressOutput = OutputObject.BaseAddress + OutputObject.ByteLength;
      this._listOutput.Add(OutputObject);
      OutputObject.AssignController(this.Name);
      this._listOutput.Sort();
      return 0;
    }

    public bool RemoveObject(VarOutput OutputObject)
    {
      if (this.GetState() == Controller_IBS_ETH.CtrlState.Connect || this.GetState() == Controller_IBS_ETH.CtrlState.Run || OutputObject == null)
        return false;
      this._listOutput.Remove(OutputObject);
      if (this._listOutput.Count == 0)
      {
        this.minPD_AddressOutput = 0;
        this.maxPD_AddressOutput = 0;
      }
      else
      {
        this.minPD_AddressOutput = int.MaxValue;
        this.maxPD_AddressOutput = 0;
        foreach (VarOutput varOutput in this._listOutput)
        {
          if (varOutput.BaseAddress < this.minPD_AddressOutput)
            this.minPD_AddressOutput = varOutput.BaseAddress;
          if (varOutput.BaseAddress + varOutput.ByteLength >= this.maxPD_AddressOutput)
            this.maxPD_AddressOutput = varOutput.BaseAddress + varOutput.ByteLength;
        }
      }
      OutputObject.AssignController(string.Empty);
      return true;
    }

    public int InputObjectStartAddress
    {
      get
      {
        return this.minPD_AddressInput;
      }
    }

    public int InputObjectEndAddress
    {
      get
      {
        return this.maxPD_AddressInput;
      }
    }

    public int InputObjectLength
    {
      get
      {
        return this.maxPD_AddressInput - this.minPD_AddressInput;
      }
    }

    public int OutputObjectStartAddress
    {
      get
      {
        return this.minPD_AddressOutput;
      }
    }

    public int OutputObjectEndAddress
    {
      get
      {
        return this.maxPD_AddressOutput;
      }
    }

    public int OutputObjectLength
    {
      get
      {
        return this.maxPD_AddressOutput - this.minPD_AddressOutput;
      }
    }

    public event UpdateProcessDataHandler OnUpdateProcessData
    {
      add
      {
        this._hdOnUpdateIO += value;
      }
      remove
      {
        this._hdOnUpdateIO -= value;
      }
    }

    public int AddObject(MessageClient MessageObject)
    {
      lock (this.syncUserAccess)
      {
        if (MessageObject == null)
          return -1;
        if (MessageObject.ControllerId != 0)
          return -2;
        ++this.msgClientObjectID;
        if (this.msgClientObjectID > (int) ushort.MaxValue)
          return -3;
        MessageObject.AssignController(this.Name, this.msgClientObjectID);
        this.messageClientList.AddService(MessageObject);
        return 0;
      }
    }

    public bool RemoveObject(MessageClient MessageObject)
    {
      bool flag = false;
      lock (this.syncUserAccess)
      {
        if (MessageObject != null)
        {
          if (MessageObject.State != MessageClientState.SendRequestOnlyDone && MessageObject.State != MessageClientState.ConfirmationReceived && MessageObject.State != MessageClientState.Error)
          {
            if (MessageObject.State != MessageClientState.Idle)
              goto label_8;
          }
          this.messageClientList.Remove(MessageObject);
          MessageObject.ClearController();
          flag = true;
        }
      }
label_8:
      return flag;
    }

    public event UpdateMailboxHandler OnUpdateMailbox
    {
      add
      {
        this._hdOnUpdateMX += value;
      }
      remove
      {
        this._hdOnUpdateMX -= value;
      }
    }

    public event ExceptionHandler OnException
    {
      add
      {
        this._diagnostic.OnException += value;
      }
      remove
      {
        this._diagnostic.OnException -= value;
      }
    }

    public event ControllerEventHandler OnRun
    {
      add
      {
        this._hdOnRun += value;
      }
      remove
      {
        this._hdOnRun -= value;
      }
    }

    public event ControllerEventHandler OnStop
    {
      add
      {
        this._hdOnStop += value;
      }
      remove
      {
        this._hdOnStop -= value;
      }
    }

    public event ControllerEventHandler OnConnect
    {
      add
      {
        this._hdOnConnect += value;
      }
      remove
      {
        this._hdOnConnect -= value;
      }
    }

    public event ControllerEventHandler OnDisable
    {
      add
      {
        this._hdOnDisable += value;
      }
      remove
      {
        this._hdOnDisable -= value;
      }
    }

    public InterbusDiagnostic BusDiag
    {
      get
      {
        return this._ibsDiagnostic;
      }
    }

    public Controller_IBS_ETH.Config Configuration
    {
      get
      {
        return this._ctrlConfig;
      }
    }

    public InterbusHandling BusHandling
    {
      get
      {
        return this._locBusHandling;
      }
      set
      {
        this._locBusHandling = value;
      }
    }

    public void ClearAllOutputValues()
    {
      this.ClearVarOutput();
    }

    private bool WatchdogEnable()
    {
      if (this._netfailDeactivate)
        return true;
      Thread.Sleep(10);
      lock (this.syncUserAccess)
      {
        int addMessage1 = this._ctrlClass.ETH_SetNetFailMode(ETH_NetFailModes.ETH_NF_ALARMSTOP_MODE);
        if (addMessage1 != 0)
        {
          this.StopOnError(ControllerDiagnostic.EnableNetfail, addMessage1, (Exception) null);
          return false;
        }
        Thread.Sleep(10);
        switch (this._watchdogTimeout)
        {
          case WatchdogMonitoringTime.Intervall_524ms:
            this._netfailTimeout = 524;
            break;
          case WatchdogMonitoringTime.Intervall_1048ms:
            this._netfailTimeout = 1048;
            break;
        }
        int addMessage2 = this._ctrlClass.ETH_SetTimeout(ref this._netfailTimeout);
        if (addMessage2 == 0)
          return true;
        this.StopOnError(ControllerDiagnostic.EnableNetfail, addMessage2, (Exception) null);
        return false;
      }
    }

    private bool NetfailClear()
    {
      Thread.Sleep(10);
      lock (this.syncUserAccess)
      {
        int addMessage = this._ctrlClass.ETH_ClearNetFail();
        if (addMessage == 0)
          return true;
        this.StopOnError(ControllerDiagnostic.ClearNetfail, addMessage, (Exception) null);
        return false;
      }
    }

    private bool CheckWatchdogState()
    {
      if (!this._netfailDeactivate && !this._ctrlNetfail)
      {
        lock (this.syncUserAccess)
        {
          int State;
          int Reason;
          int netFailStatus = this._ctrlClass.ETH_GetNetFailStatus(out State, out Reason);
          if (netFailStatus == 0)
          {
            switch (State)
            {
              case 0:
                this._ctrlNetfail = false;
                break;
              case (int) ushort.MaxValue:
                this._ctrlNetfail = true;
                this.StopOnError(ControllerDiagnostic.NetfailOccurred, Reason, (Exception) null);
                break;
            }
            return true;
          }
          this.StopOnError(ControllerDiagnostic.GetWatchdogState, netFailStatus, (Exception) null);
        }
      }
      return false;
    }

    private void DisableAll()
    {
      if (this._diagnostic != null)
        this._diagnostic.Disable();
      this._locBusHandling.Disable();
      this.messageClientList.ClearSendReceiveData();
      this.StopControllerUpdate();
    }

    private void _ctrlClass_OnEnable(object sender)
    {
      if (!this.StartUpdateMX())
        return;
      this._locBusHandling.Enable();
      if (this._ctrlConfig.GetRevisionInfo)
        this._locBusHandling.FetchRevisionInfo();
      if (this._ctrlConfig.ExpertModeActivate)
        this._locBusHandling.ExpertModeActivate();
      this.NetfailClear();
      this.ChangeState(Controller_IBS_ETH.CtrlState.Connect);
      if (this._hdOnConnect != null)
        this._hdOnConnect((object) this);
      if (!this.AutoStart() || !this.InitUpdatePD() || this.WatchdogEnable())
        return;
      this.StopOnError(ControllerDiagnostic.EnableWatchdog, 0, (Exception) null);
    }

    private void StopOnError(ControllerDiagnostic message, int addMessage, Exception innerException = null)
    {
      lock (this.syncUserAccess)
      {
        if (this.GetState() == Controller_IBS_ETH.CtrlState.Error)
          return;
        this.ChangeState(Controller_IBS_ETH.CtrlState.Error);
        if (innerException == null)
          this._diagnostic.Throw((System.Enum) message, PhoenixContact.PxC_Library.Util.Util.Int32ToByteArray(addMessage, 2));
        else
          this._diagnostic.Throw((System.Enum) message, PhoenixContact.PxC_Library.Util.Util.Int32ToByteArray(addMessage, 2), innerException);
      }
    }

    private bool StartUpdateMX()
    {
      if (this._ctrlUpdateTimeMXI >= 10 && this._ctrlUpdateTimeMXI <= 1000)
      {
        this.deleteOldServices = true;
        this._mxTimer.Intervall = this._ctrlUpdateTimeMXI;
        this.recvData = new byte[1024];
        this._mxTimer.Enable();
        if (this._ctrlConfig.UpdateControllerState >= 10 && this._ctrlConfig.UpdateControllerState <= 1000)
        {
          this._ctrlStateTimer.Intervall = this._ctrlConfig.UpdateControllerState;
          this._ctrlStateTimer.Enable();
          return true;
        }
        this.StopOnError(ControllerDiagnostic.ControllerStateCycleTimeOutOfRange, this._ctrlConfig.UpdateControllerState, (Exception) null);
        return false;
      }
      this.StopOnError(ControllerDiagnostic.MailboxDataCycleTimeOutOfRange, this._ctrlUpdateTimeMXI, (Exception) null);
      return false;
    }

    private bool InitUpdatePD()
    {
      if (this._listInput.Count > 0)
      {
        if (this.minPD_AddressInput < 0 || this.minPD_AddressInput + this.InputObjectLength - 1 > this.InputObjectEndAddress)
        {
          this.StopOnError(ControllerDiagnostic.UpdateInputBlockStartAddress, this.minPD_AddressInput, (Exception) null);
          return false;
        }
        if (this.InputObjectLength < 1 || this.InputObjectLength - 1 > this.InputObjectEndAddress)
        {
          this.StopOnError(ControllerDiagnostic.UpdateInputBlockLength, this.InputObjectLength, (Exception) null);
          return false;
        }
        this.in_buffer = new byte[this.InputObjectLength];
      }
      if (this._listOutput.Count <= 0)
        return false;
      if (this.minPD_AddressOutput < 0 || this.minPD_AddressOutput + this.OutputObjectLength - 1 > this.OutputObjectEndAddress)
      {
        this.StopOnError(ControllerDiagnostic.UpdateInputBlockStartAddress, this.minPD_AddressOutput, (Exception) null);
        return false;
      }
      if (this.OutputObjectLength < 1 || this.OutputObjectLength - 1 > this.OutputObjectEndAddress)
      {
        this.StopOnError(ControllerDiagnostic.UpdateOutputBlockLength, this.OutputObjectLength, (Exception) null);
        return false;
      }
      this.out_buffer = new byte[this.OutputObjectLength];
      return true;
    }

    private void StopControllerUpdate()
    {
      if (this._mxTimer != null)
      {
        this._mxTimer.Disable();
        Thread.Sleep(this._ctrlUpdateTimeMXI);
      }
      if (this._ctrlStateTimer != null)
      {
        this._ctrlStateTimer.Disable();
        Thread.Sleep(this._ctrlConfig.UpdateControllerState);
      }
      if (this._ctrlClass == null)
        return;
      if (this._ctrlClass.InternalState == IBS_G4_Drv.DriverState.Idle || this._ctrlClass.InternalState == IBS_G4_Drv.DriverState.Disable || this._ctrlClass.InternalState == IBS_G4_Drv.DriverState.Inactive)
        this.ChangeState(Controller_IBS_ETH.CtrlState.Disable);
      else
        this._ctrlClass.Disable(this.Configuration.AlarmStop);
    }

    private void _ctrlStateTimer_OnTick(object sender)
    {
      if (this.GetState() != Controller_IBS_ETH.CtrlState.Connect && this.GetState() != Controller_IBS_ETH.CtrlState.Run || this._locBusHandling.IsServiceActive())
        return;
      int diagnostic = this._ctrlClass.GetDiagnostic(out this._ibsDiagStatusReg, out this._ibsDiagParaReg);
      if (diagnostic != 0)
      {
        this.CheckWatchdogState();
        this.StopOnError(ControllerDiagnostic.GetDiagnostic, diagnostic, (Exception) null);
      }
      else
      {
        this._ibsDiagnostic.SetRegister(this._ibsDiagStatusReg, this._ibsDiagParaReg, 0);
        if (this._ibsDiagnostic.StatusRegister.RUN)
        {
          if (this.GetState() == Controller_IBS_ETH.CtrlState.Connect)
          {
            this.ChangeState(Controller_IBS_ETH.CtrlState.Run);
            if (this._hdOnRun != null)
              this._hdOnRun((object) this);
          }
        }
        else if (this.GetState() == Controller_IBS_ETH.CtrlState.Run)
        {
          this.ClearVarInput();
          this.ChangeState(Controller_IBS_ETH.CtrlState.Connect);
        }
      }
      if (!this._ctrlConfig.Read_IBS_Cycletime || !this._ibsDiagnostic.StatusRegister.RUN || (this._ibsDiagnostic.StatusRegister.DETECT || !(this.readCycleTime < DateTime.Now)))
        return;
      this.readCycleTime = DateTime.Now.AddSeconds(1.0);
      this._locBusHandling.FetchRealCycleTime();
    }

    private void _ctrlStateTimer_OnDisable(object Sender)
    {
      this._ibsDiagnostic.SetRegister(0, 0, 0);
    }

    private void _ctrlClass_OnTick(object sender)
    {
      if (this.GetState() != Controller_IBS_ETH.CtrlState.Connect && this.GetState() != Controller_IBS_ETH.CtrlState.Run)
        return;
      this.UpdateInputs(this.minPD_AddressInput, this.InputObjectLength);
      if (this._hdOnUpdateIO != null)
        this._hdOnUpdateIO((object) this);
      this.UpdateOutputs(this.minPD_AddressOutput, this.OutputObjectLength);
    }

    private void _ctrlClass_OnDisable(object sender)
    {
      this.ClearVarInput();
      this._ctrlConfig.WriteValue = true;
      this.ChangeState(Controller_IBS_ETH.CtrlState.Disable);
      if (this._hdOnDisable == null)
        return;
      this._hdOnDisable((object) this);
    }

    private void _ctrlClass_OnDiagnostic(Exception exceptionData)
    {
      this.StopOnError(ControllerDiagnostic.OnUpdateProcessData, 0, exceptionData);
    }

    private void UpdateInputs(int startAddress, int length)
    {
      if (!this._ibsDiagnostic.StatusRegister.RUN || this._listInput.Count <= 0)
        return;
      int addMessage = this._ctrlClass.ReadData(startAddress, ref this.in_buffer);
      switch (addMessage)
      {
        case 0:
          using (List<VarInput>.Enumerator enumerator = this._listInput.GetEnumerator())
          {
            while (enumerator.MoveNext())
            {
              VarInput current = enumerator.Current;
              if (current.BaseAddress >= startAddress && current.BaseAddress < startAddress + length)
              {
                switch (current.VarType)
                {
                  case VarType.Boolean:
                    current.SetValue = this.GetDataFromBuffer(this.in_buffer, current.BaseAddress - startAddress, current.ByteLength, current.BitOffset);
                    continue;
                  case VarType.UInt64:
                    current.SetValue = this.GetDataFromBuffer(this.in_buffer, current.BaseAddress - startAddress, current.ByteLength, current.BitOffset, current.MaxValue);
                    continue;
                  case VarType.ByteArray:
                    current.SetByteArray(this.GetByteFromBuffer(this.in_buffer, current.BaseAddress - startAddress, current.ByteLength));
                    continue;
                  default:
                    continue;
                }
              }
            }
            return;
          }
        case 134:
          this.CheckWatchdogState();
          break;
      }
      this.StopOnError(ControllerDiagnostic.RetErrReadData, addMessage, (Exception) null);
    }

    private void UpdateOutputs(int StartAddress, int Length)
    {
      if (this._listOutput.Count > 0)
      {
        foreach (VarOutput varOutput in this._listOutput)
        {
          if (varOutput.BaseAddress >= StartAddress && varOutput.BaseAddress < StartAddress + Length)
          {
            switch (varOutput.VarType)
            {
              case VarType.Boolean:
                this.PutDataToBuffer(varOutput.GetValue(), ref this.out_buffer, varOutput.BaseAddress - StartAddress, varOutput.ByteLength, varOutput.BitOffset);
                continue;
              case VarType.UInt64:
                this.PutDataToBuffer(varOutput.GetValue(), ref this.out_buffer, varOutput.BaseAddress - StartAddress, varOutput.ByteLength, varOutput.BitOffset, varOutput.MaxValue);
                continue;
              case VarType.ByteArray:
                this.PutByteToBuffer(varOutput.ByteArray, ref this.out_buffer, varOutput.BaseAddress - StartAddress);
                continue;
              default:
                continue;
            }
          }
        }
        int addMessage = this._ctrlClass.WriteData(StartAddress, this.out_buffer);
        switch (addMessage)
        {
          case 0:
            return;
          case 134:
            this.CheckWatchdogState();
            break;
        }
        this.StopOnError(ControllerDiagnostic.RetErrWriteData, addMessage, (Exception) null);
      }
      else
        this._ctrlClass.WriteData(0, new byte[1]);
    }

    private ulong GetDataFromBuffer(byte[] Data, int Address, int ByteLength, int BitOffset, ulong MaxValue)
    {
      ulong num = 0;
      for (int index = 0; index < ByteLength; ++index)
      {
        num |= (ulong) Data[Address + index];
        if (index < ByteLength - 1)
          num <<= 8;
      }
      if (BitOffset > 0)
        num >>= BitOffset;
      return num & MaxValue;
    }

    private ulong GetDataFromBuffer(byte[] Data, int Address, int ByteLength, int BitOffset)
    {
      ulong num = 0;
      for (int index = 0; index < ByteLength; ++index)
      {
        num |= (ulong) Data[Address + index];
        if (index < ByteLength - 1)
          num <<= 8;
      }
      return num >> BitOffset & 1UL;
    }

    private byte[] GetByteFromBuffer(byte[] Data, int Address, int Length)
    {
      byte[] numArray = new byte[0];
      if (Data != null && Address >= 0 && Length > 0)
      {
        numArray = new byte[Length];
        for (int index = 0; index < Length; ++index)
          numArray[index] = Data[Address + index];
      }
      else
        this.StopOnError(ControllerDiagnostic.ParaErrGetByteFromBuffer, 0, (Exception) null);
      return numArray;
    }

    private bool PutDataToBuffer(ulong Variable, ref byte[] Data, int Address, int ByteLength, int BitOffset, ulong MaxValue)
    {
      if (BitOffset > 0)
      {
        Variable = (ulong) (((long) Variable & (long) MaxValue) << BitOffset);
        MaxValue <<= BitOffset;
      }
      for (int index = ByteLength; index > 0; --index)
      {
        byte num1 = Convert.ToByte(Variable & (ulong) byte.MaxValue);
        byte num2 = Convert.ToByte(~MaxValue & (ulong) byte.MaxValue);
        Data[Address + (index - 1)] = Convert.ToByte((int) Data[Address + (index - 1)] & (int) num2 | (int) num1);
        Variable >>= 8;
        MaxValue >>= 8;
      }
      return true;
    }

    private void PutDataToBuffer(ulong Variable, ref byte[] Data, int Address, int ByteLength, int BitOffset)
    {
      ulong num1 = 1;
      if (BitOffset > 0)
        num1 <<= BitOffset;
      for (int index = ByteLength; index > 0; --index)
      {
        byte num2 = Convert.ToByte(num1 & (ulong) byte.MaxValue);
        byte num3 = Convert.ToByte(~num1 & (ulong) byte.MaxValue);
        Data[Address + (index - 1)] = Variable == 0UL ? Convert.ToByte((int) Data[Address + (index - 1)] & (int) num3) : Convert.ToByte((int) Data[Address + (index - 1)] & (int) num3 | (int) num2);
        num1 >>= 8;
      }
    }

    private void PutByteToBuffer(byte[] ByteArray, ref byte[] Data, int Address)
    {
      if (ByteArray != null && Data != null && Address >= 0)
      {
        for (int index = 0; index < ByteArray.Length; ++index)
          Data[Address + index] = ByteArray[index];
      }
      else
        this.StopOnError(ControllerDiagnostic.ParaErrPutByteToBuffer, 0, (Exception) null);
    }

    private void ClearVarInput()
    {
      if (this._listInput == null || this._listInput.Capacity <= 0)
        return;
      foreach (VarInput varInput in this._listInput)
      {
        if (varInput.VarType == VarType.Boolean || varInput.VarType == VarType.UInt64)
          varInput.SetValue = varInput.MinValue;
        if (varInput.VarType == VarType.ByteArray)
          varInput.SetByteArray(new byte[varInput.ByteArray.Length]);
      }
    }

    private void ClearVarOutput()
    {
      if (this._listOutput.Capacity <= 0)
        return;
      foreach (VarOutput varOutput in this._listOutput)
      {
        if (varOutput.VarType == VarType.Boolean || varOutput.VarType == VarType.UInt64)
          varOutput.Value = varOutput.MinValue;
        if (varOutput.VarType == VarType.ByteArray)
          varOutput.ByteArray = new byte[varOutput.ByteArray.Length];
      }
    }

    private void _mxTimer_OnTick(object sender)
    {
      if (this.GetState() != Controller_IBS_ETH.CtrlState.Connect && this.GetState() != Controller_IBS_ETH.CtrlState.Run)
        return;
      this.UpdateReceiveMXI();
      this._locBusHandling.Run();
      if (this._hdOnUpdateMX != null)
        this._hdOnUpdateMX((object) this);
      this.UpdateSendMXI();
    }

    private void UpdateReceiveMXI()
    {
      int Length = 0;
      int UserID;
      int MsgType;
      int num = this._ctrlClass.ReceiveMessage(out UserID, out Length, out MsgType, ref this.recvData) & (int) ushort.MaxValue;
      switch (num)
      {
        case 0:
        case 155:
          if (this.deleteOldServices)
          {
            if (num != 155)
              break;
            this.deleteOldServices = false;
          }
          if (num == 0 && (this.recvData[0] == (byte) 67 || this.recvData[0] == (byte) 83 || this.recvData[0] == (byte) 99))
          {
            if (!this.Configuration.EnableIBS_Indications)
              break;
            this._diagnostic.Throw((System.Enum) ControllerDiagnostic.ControllerIndication, PhoenixContact.PxC_Library.Util.Util.GetByteArrayFromService(this.recvData));
            this._diagnostic.Quit();
            break;
          }
          this.messageClientList.UpdateReceiveConfirmations(num, UserID, Length, this.recvData);
          break;
        default:
          this.StopOnError(ControllerDiagnostic.RetErrReceiveMessage, num, (Exception) null);
          break;
      }
    }

    private void UpdateSendMXI()
    {
      if (this.messageClientList.Count <= 0 || this.deleteOldServices)
        return;
      foreach (MessageClient messageClient in this.messageClientList.GetClientsWithSendData())
      {
        if (messageClient.SendDataLength > 0)
        {
          int Integer = this._ctrlClass.SendMessage(messageClient.ControllerId, messageClient.SendDataLength, 0, messageClient.GetSendData()) & (int) ushort.MaxValue;
          switch (Integer)
          {
            case 0:
              messageClient.SetSendDone();
              break;
            case 156:
            case 157:
              ++this._mxiSleep;
              break;
            default:
              messageClient.SetError();
              this._diagnostic.Throw((System.Enum) ControllerDiagnostic.RetErrSendMessage, PhoenixContact.PxC_Library.Util.Util.Int32ToByteArray(Integer, 2));
              this._diagnostic.Quit();
              break;
          }
          if (this._mxiSleep > 0)
            Thread.Sleep(this._mxiSleep);
        }
        else
        {
          this._diagnostic.Throw((System.Enum) ControllerDiagnostic.ParaErrMessageClient, new byte[0]);
          this._diagnostic.Quit();
        }
      }
    }

    private void _mxTimer_OnDiagnostic(Exception exceptionData)
    {
      this.StopOnError(ControllerDiagnostic.OnUpdateMailbox, 0, exceptionData);
    }

    private void _ctrlStateTimer_OnDiagnostic(Exception exceptionData)
    {
      this.StopOnError(ControllerDiagnostic.OnStateChangeEvents, 0, exceptionData);
      this._ibsDiagnostic.SetRegister(0, 0, 0);
    }

    private void _ctrlBus_OnException(Exception exceptionData)
    {
      this._diagnostic.Throw((System.Enum) ControllerDiagnostic.InterbusHandlingDiagnostic, new byte[0], exceptionData);
      this._diagnostic.Quit();
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (this.disposed)
        return;
      if (disposing)
      {
        this.Disable();
        this.ChangeState(Controller_IBS_ETH.CtrlState.GoingToDispose);
        this.RunDispose();
        Thread.Sleep(200);
      }
      this.disposed = true;
    }

    private void RunDispose()
    {
      if (this._ctrlStateTimer != null)
        this._ctrlStateTimer.Dispose();
      if (this._mxTimer != null)
        this._mxTimer.Dispose();
      if (this._ctrlClass != null)
      {
        this._ctrlClass.Dispose();
        this._ctrlClass = (IBS_G4_Drv) null;
      }
      if (this._diagnostic != null)
      {
        this._diagnostic.Dispose();
        this._diagnostic = (Diagnostic) null;
      }
      if (this._locBusHandling != null)
        this._locBusHandling = (InterbusHandling) null;
      this._listInput = (List<VarInput>) null;
      this._listOutput = (List<VarOutput>) null;
      this.messageClientList.DeleteAll();
    }

    public class Config
    {
      private bool _writeValues;
      private bool _enableIndications;
      private int _updateCotrollerState;
      private bool _read_IBS_Cycletime;
      private bool _getRevisionInfo;
      private bool _DNS_NameResolution;
      private bool _expertModeActivate;
      private bool _alarmStop;

      public Config()
      {
        this._writeValues = true;
        this._enableIndications = false;
        this._updateCotrollerState = 100;
        this._read_IBS_Cycletime = true;
        this._getRevisionInfo = true;
        this._DNS_NameResolution = false;
        this._expertModeActivate = true;
        this._alarmStop = true;
      }

      internal bool WriteValue
      {
        get
        {
          return this._writeValues;
        }
        set
        {
          this._writeValues = value;
        }
      }

      public bool EnableIBS_Indications
      {
        get
        {
          return this._enableIndications;
        }
        set
        {
          if (!this._writeValues)
            return;
          this._enableIndications = value;
        }
      }

      public int UpdateControllerState
      {
        get
        {
          return this._updateCotrollerState;
        }
        set
        {
          if (!this._writeValues || value < 10 || value > 1000)
            return;
          this._updateCotrollerState = value;
        }
      }

      public bool Read_IBS_Cycletime
      {
        get
        {
          return this._read_IBS_Cycletime;
        }
        set
        {
          if (!this._writeValues)
            return;
          this._read_IBS_Cycletime = value;
        }
      }

      public bool GetRevisionInfo
      {
        get
        {
          return this._getRevisionInfo;
        }
        set
        {
          if (!this._writeValues)
            return;
          this._getRevisionInfo = value;
        }
      }

      public bool DNS_NameResolution
      {
        get
        {
          return this._DNS_NameResolution;
        }
        set
        {
          if (!this._writeValues)
            return;
          this._DNS_NameResolution = value;
        }
      }

      internal bool ExpertModeActivate
      {
        get
        {
          return this._expertModeActivate;
        }
        set
        {
          if (!this._writeValues)
            return;
          this._expertModeActivate = value;
        }
      }

      public bool AlarmStop
      {
        get
        {
          return this._alarmStop;
        }
        set
        {
          if (!this._writeValues)
            return;
          this._alarmStop = value;
        }
      }
    }

    private enum CtrlState
    {
      Inactive,
      Idle,
      GoingToEnable,
      Connect,
      Run,
      Error,
      GoingToDisable,
      Disable,
      GoingToDispose,
    }
  }
}
