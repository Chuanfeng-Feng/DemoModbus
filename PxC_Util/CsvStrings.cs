﻿// Decompiled with JetBrains decompiler
// Type: PhoenixContact.PxC_Library.Util.CsvStrings
// Assembly: PxC_Util, Version=1.4.6053.23249, Culture=neutral, PublicKeyToken=bbf13850d99d956d
// MVID: 18D5BDF8-0D3D-4138-A479-03DED5E34959
// Assembly location: D:\Program Files (x86)\Phoenix Contact\HFI 3.2\HFI_Tools\Libraries\PxC_Util.dll

using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace PhoenixContact.PxC_Library.Util
{
  [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
  [DebuggerNonUserCode]
  [CompilerGenerated]
  internal class CsvStrings
  {
    private static ResourceManager resourceMan;
    private static CultureInfo resourceCulture;

    internal CsvStrings()
    {
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static ResourceManager ResourceManager
    {
      get
      {
        if (CsvStrings.resourceMan == null)
          CsvStrings.resourceMan = new ResourceManager("PhoenixContact.PxC_Library.Util.CsvStrings", typeof (CsvStrings).Assembly);
        return CsvStrings.resourceMan;
      }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static CultureInfo Culture
    {
      get
      {
        return CsvStrings.resourceCulture;
      }
      set
      {
        CsvStrings.resourceCulture = value;
      }
    }

    internal static string ErrorWhileParsingData
    {
      get
      {
        return CsvStrings.ResourceManager.GetString(nameof (ErrorWhileParsingData), CsvStrings.resourceCulture);
      }
    }

    internal static string RowCountError
    {
      get
      {
        return CsvStrings.ResourceManager.GetString(nameof (RowCountError), CsvStrings.resourceCulture);
      }
    }
  }
}
