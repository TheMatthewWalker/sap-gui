using Avalonia.Controls;
using sap_gui;
using sap_gui.Pages;
using SAPFunctionsOCX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class SapController
{
    dynamic sapFuncs = App.SapFuncs;
    
    public IFunction CreateFunction(string name)
    {
        IFunction func = (IFunction)sapFuncs.Add(name);
        return func;
    }

    public bool Login(string system, string client, string systemId, string user, string password)
    {
        try
        {
            dynamic sapFuncs = App.SapFuncs;
            dynamic conn = sapFuncs.Connection;

                    conn.System = system;
                    conn.Client = client;
                    conn.SystemID = systemId;
                    conn.User = user;
                    conn.Password = password;
                    return conn.Logon(0, true);
        }
        catch (Exception ex)
        {
            throw new Exception($"Login failed: {ex.Message}", ex);
        }
    }

    public TOCreateResult CreateTransferOrder(string plant, string sloc, string warehouse, string binType, string bin, string material, string batch, float qty, string destBin, string destBinType, string category, string special, string specialNumber)
    {
        try
        {
            dynamic func = sapFuncs.Add("L_TO_CREATE_SINGLE");

            if (func == null)
            {
                return new TOCreateResult
                {
                    TransferOrderNumber = "",
                    Message = "",
                    Error = "Function module not found"
                };
            }

            // Set the import parameters for the function module
            func.Exports("I_LGNUM").Value = warehouse;         // Warehouse number
            func.Exports("I_WERKS").Value = plant;             // Plant
            func.Exports("I_LGORT").Value = sloc;              // Storage Location
            func.Exports("I_SQUIT").Value = "X";              // Immediate Confirmation

            func.Exports("I_BWLVS").Value = "999";         // Movement Type

            func.Exports("I_MATNR").Value = material;          // Material
            func.Exports("I_ANFME").Value = qty;               // Quantity
            func.Exports("I_CHARG").Value = batch ?? "";       // Batch number
            func.Exports("I_ZEUGN").Value = batch ?? "";       // Certificate number

            func.Exports("I_VLTYP").Value = binType;         // Storage bin type
            func.Exports("I_VLPLA").Value = bin;             // Storage bin

            func.Exports("I_BESTQ").Value = category ?? "";        // Stock Category (Blocked / Quality)
            func.Exports("I_SOBKZ").Value = special ?? "";         // Special Stock Indicator
            func.Exports("I_SONUM").Value = specialNumber ?? "";   // Special Stock Number

            func.Exports("I_NLPLA").Value = destBin;         // Destination bin
            func.Exports("I_NLTYP").Value = destBinType;     // Destination bin type

            // Execute BAPI
            bool success = func.Call;
            if (!success)
                return new TOCreateResult
                {
                    TransferOrderNumber = "",
                    Message = "",
                    Error = Convert.ToString(func.Exception)
                };

            // Read return messages
            var returnTable = func.Tables.Item("RETURN");
            string messages = "";
            string errorMsg = null;

            if (returnTable != null)
            {
                int rc = 0;
                try { rc = (int)returnTable.Rows.Count; } catch { rc = 0; }

                for (int i = 1; i <= rc; i++)
                {
                    var row = returnTable.Rows.Item(i);
                    string type = Convert.ToString(row["TYPE"] ?? "");
                    string msg = Convert.ToString(row["MESSAGE"] ?? "");
                    messages += $"{type}: {msg}\n";
                    if (type == "E" || type == "A")
                        errorMsg = msg;
                }
            }

            // Transfer Order number (if created)
            string toNumber = Convert.ToString(func.Imports.Item("E_TANUM").Value);

            if (errorMsg == "RFC_COMMUNICATION_ERROR" || errorMsg == "INVALID_RFC_HANDLE")
            {
                Login("KAP", "100", "01", App.CurrentUser, App.CurrentPass);
                errorMsg = "Connection Issue // System Re-connecting // Please Try Again";
            }

            return new TOCreateResult
            {
                TransferOrderNumber = toNumber,
                Message = messages.Trim(),
                Error = errorMsg
            };

        }
        catch (System.Runtime.InteropServices.COMException comEx)
        {
            // This is the error you reported
            System.Diagnostics.Debug.WriteLine($"COMException: {comEx.Message}");
            System.Diagnostics.Debug.WriteLine($"COMException: {comEx.ErrorCode}");
            throw new Exception(
                "COM failed:\n" +
                $"Message: {comEx.Message}\n" +
                $"Error: {comEx.ErrorCode}\n",
                comEx
            );
        }
        catch (Exception ex)
        {
            string inner = ex.InnerException?.Message ?? "null";
            string trace = ex.StackTrace ?? "null";

            throw new Exception(
                "LT01 failed:\n" +
                $"Message: {ex.Message}\n" +
                $"Inner: {inner}\n" +
                $"StackTrace: {trace}\n",
                ex
            );
        }
    }

    public List<TableLQUA> ReadSapTable(string tableName, string[] fieldsToRead, string[] options = null, int rowCount = 1000, string delimiter = ";")
    {
        if (fieldsToRead == null || fieldsToRead.Length == 0)
            throw new ArgumentException("You must provide at least one field.");

        var result = new List<TableLQUA>();

        // Initialize SAP Functions
        dynamic sapFuncs = App.SapFuncs;
        dynamic conn = sapFuncs.Connection;


        dynamic func = sapFuncs.Add("RFC_READ_TABLE");
        if (func == null)
            throw new Exception("Failed to add RFC_READ_TABLE function.");

        // Set basic parameters
        func.exports("QUERY_TABLE").Value = tableName;
        func.exports("ROWCOUNT").Value = rowCount;
        func.exports("DELIMITER").Value = delimiter;

        // Get tables
        var fields = func.tables.Item("FIELDS");
        var optionsTable = func.tables.Item("OPTIONS");
        fields.Freetable();
        optionsTable.Freetable();

        // Add requested fields dynamically
        foreach (var name in fieldsToRead)
        {
            var row = fields.Rows.Add();
            row["FIELDNAME"] = name;
        }

        // Add filter options if provided
        if (options != null)
        {
            foreach (var opt in options)
            {
                var row = optionsTable.Rows.Add();
                row["TEXT"] = opt;
            }
        }

        // Call the function
        bool sapExec = func.Call;
        if (!sapExec)
            throw new Exception("SAP RFC_READ_TABLE call failed.");

        // Process returned data
        var dataTable = func.tables.Item("DATA");
        foreach (var dataRow in dataTable.Rows)
        {
            string rowStr = Convert.ToString(dataRow["WA"]) ?? string.Empty;
            var cols = rowStr.Split(delimiter);

            var record = new TableLQUA
            {
                LGORT = cols.ElementAtOrDefault(0)?.Trim() ?? "",
                MATNR = long.TryParse(cols.ElementAtOrDefault(1)?.Trim(), out _)
                    ? cols.ElementAtOrDefault(1)?.Trim().TrimStart('0') ?? ""
                    : cols.ElementAtOrDefault(1)?.Trim() ?? "",
                CHARG = cols.ElementAtOrDefault(2)?.Trim() ?? "",
                GESME = decimal.TryParse(cols.ElementAtOrDefault(3)?.Trim(), out decimal gesmeVal) ? (gesmeVal / 1000m) : 0m,
                LGTYP = cols.ElementAtOrDefault(4)?.Trim() ?? "",
                LGPLA = cols.ElementAtOrDefault(5)?.Trim() ?? "",
                BESTQ = cols.ElementAtOrDefault(6)?.Trim() ?? "",
                SOBKZ = cols.ElementAtOrDefault(7)?.Trim() ?? "",
                SONUM = cols.ElementAtOrDefault(8)?.Trim() ?? ""

            };

            result.Add(record);
        }

        return result;
    }

    public string SapN2C(string value, int columnLength)
    {
        string material = value.Trim() ?? "";

        // Check if numeric
        if (long.TryParse(material, out long numericValue))
        {
            // Pad with leading zeros to length 18
            string padded = numericValue.ToString().PadLeft(columnLength, '0');
            return padded;
        }
        else
        {
            // Not numeric – leave as-is or show a message
            return material;
        }

    }

    public static SAPFunctions RecreateSapFunctions()
    {
        try
        {
            if (App.SapFuncs != null)
            {
                Marshal.FinalReleaseComObject(App.SapFuncs);
                App.SapFuncs = null;
            }
        }
        catch { /* ignore */ }

        GC.Collect();
        GC.WaitForPendingFinalizers();

        App.SapFuncs = new SAPFunctions();
        return App.SapFuncs;
    }


    public class TOCreateResult
    {
        public string TransferOrderNumber { get; set; }
        public string Message { get; set; }
        public string Error { get; set; }
        public bool Success => string.IsNullOrEmpty(Error);
    }

}

