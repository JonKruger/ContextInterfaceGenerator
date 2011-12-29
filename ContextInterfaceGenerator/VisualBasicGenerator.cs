using System.Collections.Generic;
using System.IO;

namespace ContextInterfaceGenerator
{
    public class VisualBasicGenerator : IGenerator
    {
        private readonly ContextDefinition _definition;
        private readonly IEnumerable<ContextType> _types;
        private readonly IEnumerable<ContextFunction> _functions;
        private readonly bool _functionsOnly;

        public VisualBasicGenerator(ContextDefinition definition, IEnumerable<ContextType> types, IEnumerable<ContextFunction> functions,
            bool functionsOnly)
        {
            _definition = definition;
            _types = types;
            _functions = functions;
            _functionsOnly = functionsOnly;
        }

        private int _tabCount;
        private string _outputFile;

        private TextWriter _writer;
        public TextWriter Writer
        {
            get
            {
                if (_writer == null)
                    _writer = new StreamWriter(_outputFile);
                return _writer;
            }
            set { _writer = value; }
        }

        private string InterfaceName
        {
            get { return "I" + _definition.ClassName; }
        }

        private string ProxyName
        {
            get { return _definition.ClassName + "Proxy"; }
        }

        private string ClassName
        {
            get { return _definition.ClassName; }
        }

        private string Modifier
        {
            get { return _definition.AccessModifier; }
        }

        public void GenerateContext(string outputFile)
        {
            _outputFile = outputFile;

            OutputImports();

            GenerateInterface();

            if (!_functionsOnly)
                GenerateProxy();


            Writer.Close();
            Writer = null;
        }



        private void GenerateProxy()
        {
            WriteProxyHeader();
            WriteProxyConstructor();
            WriteGetTableMethod();
            WriteProxyTypes();
            WriteProxyFunctions();
            CloseClass();
        }

        private void WriteProxyFunctions()
        {
            WriteProxySub(
                "Public Shadows Sub CreateDatabase() Implements {0}.CreateDatabase",
                "MyBase.CreateDatabase()");

            WriteProxyFunction(
                "Public Shadows Function DatabaseExists() As Boolean Implements {0}.DatabaseExists",
                "Return MyBase.DatabaseExists()");

            WriteProxySub(
                "Public Shadows Sub DeleteDatabase() Implements {0}.DeleteDatabase",
                "MyBase.DeleteDatabase()");

            WriteProxySub(
                "Public Shadows Sub Dispose() Implements {0}.Dispose",
                "MyBase.Dispose()");

            WriteProxyFunction(
                "Public Shadows Function ExecuteCommand(ByVal command As String, ByVal ParamArray parameters As Object()) As Integer Implements {0}.ExecuteCommand", 
                "Return MyBase.ExecuteCommand(command, parameters)");

            WriteProxyFunction(
                "Public Shadows Function ExecuteQuery(Of TResult)(ByVal query As String, ByVal ParamArray parameters As Object()) As IEnumerable(Of TResult) Implements {0}.ExecuteQuery",
                "Return MyBase.ExecuteQuery(Of TResult)(query, parameters)");

            WriteProxyFunction(
                "Public Shadows Function ExecuteQuery(ByVal elementType As Type, ByVal query As String, ByVal ParamArray parameters As Object()) As IEnumerable Implements {0}.ExecuteQuery",
                "Return MyBase.ExecuteQuery(elementType, query, parameters)");

            WriteProxyFunction(
                "Public Shadows Function GetChangeSet() As ChangeSet Implements {0}.GetChangeSet",
                "Return GetChangeSet()");

            WriteProxyFunction(
                "Public Shadows Function GetCommand(ByVal query As IQueryable) As DbCommand Implements {0}.GetCommand",
                "Return MyBase.GetCommand(query)");

            WriteProxyFunction(
                "Public Shadows Function GetTable(ByVal type As Type) As ITable Implements {0}.GetTable",
                "Return MyBase.GetTable(type)");

            WriteProxySub(
                "Public Shadows Sub Refresh(ByVal mode as RefreshMode, ByVal ParamArray entities As Object()) Implements {0}.Refresh",
                "MyBase.Refresh(mode, entities)");

            WriteProxySub(
                "Public Shadows Sub Refresh(ByVal mode as RefreshMode, ByVal entities As IEnumerable) Implements {0}.Refresh",
                "MyBase.Refresh(mode, entities)");

            WriteProxySub(
                "Public Shadows Sub Refresh(ByVal mode as RefreshMode, ByVal entity as Object) Implements {0}.Refresh",
                "MyBase.Refresh(mode, entity)");

            WriteProxySub(
                "Public Shadows Sub SubmitChanges() Implements {0}.SubmitChanges",
                "MyBase.SubmitChanges()");

            WriteProxySub(
                "Public Shadows Sub SubmitChanges(ByVal failureMode As ConflictMode) Implements {0}.SubmitChanges",
                "MyBase.SubmitChanges(failureMode)");

            WriteProxyFunction(
                "Public Shadows Function Translate(Of TResult)(ByVal reader As DbDataReader) As IEnumerable(Of TResult) Implements {0}.Translate",
                "Return MyBase.Translate(Of TResult)(reader)");

            WriteProxyFunction(
                "Public Shadows Function Translate(ByVal reader As DbDataReader) As IMultipleResults Implements {0}.Translate",
                "Return MyBase.Translate(reader)");

            WriteProxyFunction(
                "Public Shadows Function Translate(ByVal elementType As Type, ByVal reader As DbDataReader) As IEnumerable Implements {0}.Translate",
                "Return MyBase.Translate(elementType, reader)");

            WriteProxyProperty(
                "Public Shadows ReadOnly Property ChangeConflicts() As ChangeConflictCollection Implements {0}.ChangeConflicts",
                "Return MyBase.ChangeConflicts()");

            WriteProxyProperty(
                "Public Shadows ReadOnly Property CommandTimeout() As Integer Implements {0}.CommandTimeout",
                "Return MyBase.CommandTimeout()");

            WriteProxyProperty(
                "Public Shadows ReadOnly Property Connection() As DbConnection Implements {0}.Connection",
                "Return MyBase.Connection()");

            WriteProxyProperty(
                "Public Shadows ReadOnly Property DeferredLoadingEnabled() As Boolean Implements {0}.DeferredLoadingEnabled",
                "Return MyBase.DeferredLoadingEnabled()");

            WriteProxyProperty(
                "Public Shadows ReadOnly Property LoadOptions() As DataLoadOptions Implements {0}.LoadOptions",
                "Return MyBase.LoadOptions()");

            WriteProxyProperty(
                "Public Shadows ReadOnly Property Log() As TextWriter Implements {0}.Log",
                "Return MyBase.Log()");

            WriteProxyProperty(
                "Public Shadows ReadOnly Property Mapping() As MetaModel Implements {0}.Mapping",
                "Return MyBase.Mapping()");

            WriteProxyProperty(
                "Public Shadows ReadOnly Property ObjectTrackingEnabled() As Boolean Implements {0}.ObjectTrackingEnabled",
                "Return MyBase.ObjectTrackingEnabled()");

            WriteProxyProperty(
                "Public Shadows ReadOnly Property Transaction() As DbTransaction Implements {0}.Transaction",
                "Return MyBase.Transaction()");

            foreach (ContextFunction function in _functions)
            {
                WriteProxyFunction(function);
            }
        }

        private void WriteProxyProperty(string signature, string call)
        {
            Writer.WriteLine();
            Writer.WriteLine(Tabs + string.Format(signature, InterfaceName));
            OpenGet();
            Writer.WriteLine(Tabs + call);
            CloseGet();
            Writer.WriteLine(Tabs + "End Property");
        }

        private void WriteProxyFunction(string signature, string call)
        {
            Writer.WriteLine();
            Writer.WriteLine(Tabs + string.Format(signature, InterfaceName));
            _tabCount++;
            Writer.WriteLine(Tabs + call);
            CloseFunction();
        }

        private void WriteProxySub(string signature, string call)
        {
            Writer.WriteLine();
            Writer.WriteLine(Tabs + string.Format(signature, InterfaceName));
            _tabCount++;
            Writer.WriteLine(Tabs + call);
            CloseSub();
        }

        private void WriteProxyTypes()
        {
            foreach (ContextType type in _types)
            {
                WriteProxyType(type);
            }
        }

        private void WriteProxyType(ContextType type)
        {
            Writer.WriteLine();
            Writer.WriteLine(Tabs + string.Format("Private Overloads ReadOnly Property {1}() As ITable(Of {0}) Implements {2}.{1}", type.ClassName, type.MemberName, InterfaceName));
            OpenGet();
            Writer.WriteLine(Tabs + string.Format("return MyBase.{0})", type.MemberName));
            CloseGet();
            Writer.WriteLine(Tabs + "End Property");
        }

        private void CloseClass()
        {
            _tabCount--;
            Writer.WriteLine(Tabs + "End Class");
        }

        private void OpenGet()
        {
            _tabCount++;
            Writer.WriteLine(Tabs + "Get");
            _tabCount++;
        }

        private void CloseGet()
        {
            _tabCount--;
            Writer.WriteLine(Tabs + "End Get");
            _tabCount--;
        }

        private void CloseFunction()
        {
            _tabCount--;
            Writer.WriteLine(Tabs + "End Function");
        }

        private void CloseSub()
        {
            _tabCount--;
            Writer.WriteLine(Tabs + "End Sub");
        }

        private void CloseInterface()
        {
            _tabCount--;
            Writer.WriteLine(Tabs + "End Interface");
        }

        private void WriteGetTableMethod()
        {
            Writer.WriteLine();
            Writer.WriteLine(Tabs + string.Format("Private Shadows Function GetTable(Of TEntity As Class)() As ITable(Of TEntity) Implements {0}.GetTable", InterfaceName));
            _tabCount++;
            Writer.WriteLine(Tabs + "return MyBase.GetTable(Of TEntity)()");
            CloseFunction();
        }

        private void WriteProxyConstructor()
        {
            Writer.WriteLine(Tabs + string.Format("Public Sub New(ByVal connectionString As String)"));
            _tabCount++;
            Writer.WriteLine(Tabs + "MyBase.New(connectionString)");
            CloseSub();
            
        }

        private void WriteProxyHeader()
        {
            Writer.WriteLine();
            Writer.WriteLine(Tabs + string.Format("Partial {1} Class {0}", ProxyName, Modifier.ToLower() == "internal" ? "Friend" : Modifier));
            _tabCount++;
            Writer.WriteLine(Tabs + string.Format("Inherits {0}", ClassName));
            Writer.WriteLine(Tabs + string.Format("Implements {0}", InterfaceName));
        }

        private void WriteInterfaceHeader()
        {
            Writer.WriteLine(Tabs + string.Format("{1} Interface {0}", InterfaceName, Modifier.ToLower() == "internal" ? "Friend" : Modifier));
            _tabCount++;
            Writer.WriteLine(Tabs + "Implements IDisposable");
        }

        private void GenerateInterface()
        {
            WriteInterfaceHeader();
            if (!_functionsOnly)
            {
                WriteInterfaceBasics();
                WriteInterfaceTypes();
            }
            WriteInterfaceFunctions();
            CloseInterface();
        }

        private void WriteInterfaceBasics()
        {
            Writer.WriteLine(Tabs + "Sub CreateDatabase()");
            Writer.WriteLine(Tabs + "Function DatabaseExists() As Boolean");
            Writer.WriteLine(Tabs + "Sub DeleteDatabase()");
            Writer.WriteLine(Tabs + "Sub Dispose()");
            Writer.WriteLine(Tabs + "Function ExecuteCommand(ByVal command As String, ByVal ParamArray parameters As Object()) As Integer");
            Writer.WriteLine(Tabs + "Function ExecuteQuery(Of TResult)(ByVal query As String, ByVal ParamArray parameters As Object()) As IEnumerable(Of TResult)");
            Writer.WriteLine(Tabs + "Function ExecuteQuery(ByVal elementType As Type, ByVal query As String, ByVal ParamArray parameters As Object()) As IEnumerable");
            Writer.WriteLine(Tabs + "Function GetChangeSet() As ChangeSet");
            Writer.WriteLine(Tabs + "Function GetCommand(ByVal query As IQueryable) As DbCommand");
            Writer.WriteLine(Tabs + "Function GetTable(Of TEntity As Class)() As ITable(Of TEntity)");
            Writer.WriteLine(Tabs + "Function GetTable(ByVal type As Type) As ITable");
            Writer.WriteLine(Tabs + "Sub Refresh(ByVal mode as RefreshMode, ByVal ParamArray entities As Object())");
            Writer.WriteLine(Tabs + "Sub Refresh(ByVal mode as RefreshMode, ByVal entities As IEnumerable)");
            Writer.WriteLine(Tabs + "Sub Refresh(ByVal mode as RefreshMode, ByVal entity as Object)");
            Writer.WriteLine(Tabs + "Sub SubmitChanges()");
            Writer.WriteLine(Tabs + "Sub SubmitChanges(ByVal failureMode As ConflictMode)");
            Writer.WriteLine(Tabs + "Function Translate(Of TResult)(ByVal reader As DbDataReader) As IEnumerable(Of TResult)");
            Writer.WriteLine(Tabs + "Function Translate(ByVal reader As DbDataReader) As IMultipleResults");
            Writer.WriteLine(Tabs + "Function Translate(ByVal elementType As Type, ByVal reader As DbDataReader) As IEnumerable");
            Writer.WriteLine(Tabs + "ReadOnly Property ChangeConflicts() As ChangeConflictCollection");
            Writer.WriteLine(Tabs + "ReadOnly Property CommandTimeout() As Integer");
            Writer.WriteLine(Tabs + "ReadOnly Property Connection() As DbConnection");
            Writer.WriteLine(Tabs + "ReadOnly Property DeferredLoadingEnabled() As Boolean");
            Writer.WriteLine(Tabs + "ReadOnly Property LoadOptions() As DataLoadOptions");
            Writer.WriteLine(Tabs + "ReadOnly Property Log() As TextWriter");
            Writer.WriteLine(Tabs + "ReadOnly Property Mapping() As MetaModel");
            Writer.WriteLine(Tabs + "ReadOnly Property ObjectTrackingEnabled() As Boolean");
            Writer.WriteLine(Tabs + "ReadOnly Property Transaction() As DbTransaction");
        }

        //Functions
        //If there is no return type --
        //If the IsComposable is true, then return IQueryable<#ElementType#> and not ISingleResult
        //public ISingleResult<#ElementType#> #Method# (#ParameterType# #ParameterName#)
        //{
        //  return _context.#Method#(#ParameterName);
        //}
        //If Direction is InOut then ref parameter
        //All parameters for functions are nullable.

        private void WriteInterfaceFunctions()
        {
            foreach (ContextFunction function in _functions)
            {
                WriteInterfaceFunction(function);
            }
        }

        private void WriteInterfaceFunction(ContextFunction function)
        {
            if (!string.IsNullOrEmpty(function.ReturnType))
                Writer.WriteLine(Tabs + string.Format("Function {1}({2}) As {0}", function.IsComposable && function.ReturnType != "System.String" ? "Nullable(Of " + function.ReturnType + ")" : function.ReturnType, function.MethodName,
                    function.GetSignatureForVB()));
            else
                Writer.WriteLine(Tabs + string.Format("Function {2}({3}) As {0}(Of {1})",
                                                      function.IsComposable ? "IQueryable" : "ISingleResult",
                                                      function.ReturnElement, function.MethodName,
                                                      function.GetSignatureForVB()));
        }

        private void WriteProxyFunction(ContextFunction function)
        {
            Writer.WriteLine();
            if (!string.IsNullOrEmpty(function.ReturnType))
                Writer.WriteLine(Tabs + string.Format("Private Function {1}_proxy({2}) As {0} Implements {3}.{1}", function.IsComposable && function.ReturnType != "System.String" ? "Nullable(Of " + function.ReturnType + ")" : function.ReturnType, function.MethodName,
                    function.GetSignatureForVB(), InterfaceName));
            else
                Writer.WriteLine(Tabs + string.Format("Private Function {2}_proxy({3}) As {0}(Of {1}) Implements {4}.{2}",
                                                      function.IsComposable ? "IQueryable" : "ISingleResult",
                                                      function.ReturnElement, function.MethodName,
                                                      function.GetSignatureForVB(), InterfaceName));
            _tabCount++;
            Writer.WriteLine(Tabs + string.Format("Return {0}({1})", function.MethodName, function.GetCallForVB()));
            CloseFunction();
        }

        private void WriteInterfaceTypes()
        {
            foreach (ContextType type in _types)
            {
                WriteInterfaceType(type);
            }
        }

        private void WriteInterfaceType(ContextType type)
        {
            Writer.WriteLine(Tabs + string.Format("ReadOnly Property {1}() As ITable(Of {0})", type.ClassName, type.MemberName));
        }

        private void OutputImports()
        {
            Writer.WriteLine("Imports System");
            Writer.WriteLine("Imports System.Data.Linq");
            Writer.WriteLine("Imports System.Collections");
            Writer.WriteLine("Imports System.Collections.Generic");
            Writer.WriteLine("Imports System.Data.Common");
            Writer.WriteLine("Imports System.Data.Linq.Mapping");
            Writer.WriteLine("Imports System.IO");
            Writer.WriteLine("Imports System.Linq");
            Writer.WriteLine("");
        }

        private string Tabs
        {
            get { return _tabCount > 0 ? new string('\t', _tabCount) : string.Empty; }
        }
    }
}

