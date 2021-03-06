﻿//************************************************************************
//
//    This work contains valuable confidential and proprietary
//    information.  Disclosure, use or reproduction without the
//    written authorization of the company is prohibited.  This
//    unpublished work by the company is protected by the laws of the
//    United States and other countries.  If publication of the work
//    should occur the following notice shall apply:
//    Copyright Baldwin Technology Company, Inc. 1998/2018 All Rights Reserved
//
//    Creation Date: 07/5/2018
//    Description: IVariable interface for Variable to work with Easymodbus.dll

//
//************************************************************************
namespace Qti.Autotron.ModbusAutotronAPI
{
    public interface IVariable
    {
        int BaseAddress { get; }
        int BitOffset { get; }
        int ByteLength { get; }
        string ControllerName { get; }
        int Length { get; }
        ulong MaxValue { get; }
        ulong MinValue { get; }
        string Name { get; }
        VarType VarType { get; }

        event VarChangeHandler OnChange;

        int CompareTo(object obj);
        string ToString();
    }
}