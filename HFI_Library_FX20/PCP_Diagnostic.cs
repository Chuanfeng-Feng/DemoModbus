﻿// Decompiled with JetBrains decompiler
// Type: PhoenixContact.HFI.PCP_Diagnostic
// Assembly: HFI_Library_FX20, Version=2.1.0.0, Culture=neutral, PublicKeyToken=61dd274f0cd79c49
// MVID: BBEABD5D-3D47-474E-899D-9A7AB31C38F6
// Assembly location: D:\DotNet Framework 2.0\HFI\Libraries\HFI_Library_FX20.dll

namespace PhoenixContact.HFI
{
  public enum PCP_Diagnostic
  {
    Inactive = 0,
    NoError = 33536, // 0x00008300
    ConfirmationTimeout = 49156, // 0x0000C004
    UnexpectedService = 49157, // 0x0000C005
    NegativeInitiateConfirmation = 49409, // 0x0000C101
    NegativeReadConfirmation = 49922, // 0x0000C302
    NegativeWriteConfirmation = 49923, // 0x0000C303
    UnkonwnService = 49924, // 0x0000C304
    ReceiveDataTooLong = 49925, // 0x0000C305
  }
}
