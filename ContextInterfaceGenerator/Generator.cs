using System.Collections.Generic;
using System.IO;

namespace ContextInterfaceGenerator
{
    public interface IGenerator
    {
        void GenerateContext(string outputFile);
    }

    public class Generator : IGenerator
    {
        private readonly ContextDefinition _definition;
        private readonly IEnumerable<ContextType> _types;
        private readonly IEnumerable<ContextFunction> _functions;
        private readonly bool _functionsOnly;

        public Generator(ContextDefinition definition, IEnumerable<ContextType> types, IEnumerable<ContextFunction> functions,
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

            OutputNamespace();

            GenerateInterface();

            if (!_functionsOnly)
                GenerateProxy();

            CloseBrace();

            Writer.Close();
            Writer = null;
        }

        private void GenerateProxy()
        {
            WriteProxyHeader();
            WriteProxyConstructor();
            WriteGetTableMethod();
            WriteProxyTypes();
            //WriteProxyFunctions();
            CloseBrace();
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
            Writer.WriteLine(Tabs + string.Format("ITable<{0}> {2}.{1}", type.ClassName, type.MemberName, InterfaceName));
            OpenBrace();
            Writer.WriteLine(Tabs + string.Format("get {{ return {0}; }}", type.MemberName));
            CloseBrace();
        }

        private void WriteGetTableMethod()
        {
            Writer.WriteLine();
            Writer.WriteLine(Tabs + string.Format("ITable<TEntity> {0}.GetTable<TEntity>()", InterfaceName));
            OpenBrace();
            Writer.WriteLine(Tabs + "return GetTable<TEntity>();");
            CloseBrace();
        }

        private void WriteProxyConstructor()
        {
            Writer.WriteLine(Tabs + string.Format("public {0}(string connectionString) : base(connectionString)", ProxyName));
            OpenBrace();
            CloseBrace();
        }

        private void WriteProxyHeader()
        {
            Writer.WriteLine();
            Writer.WriteLine(Tabs + string.Format("{3} partial class {0} : {1}, {2}", ProxyName, ClassName, InterfaceName, Modifier));
            OpenBrace();
        }

        private void WriteInterfaceHeader()
        {
            Writer.WriteLine(Tabs + string.Format("{1} partial interface {0}", InterfaceName, Modifier));
            OpenBrace();
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
            CloseBrace();
        }

        private void WriteInterfaceBasics()
        {
            Writer.WriteLine(Tabs + "void CreateDatabase();");
            Writer.WriteLine(Tabs + "bool DatabaseExists();");
            Writer.WriteLine(Tabs + "void DeleteDatabase();");
            Writer.WriteLine(Tabs + "void Dispose();");
            Writer.WriteLine(Tabs + "int ExecuteCommand(string command, params object[] parameters);");
            Writer.WriteLine(Tabs + "IEnumerable<TResult> ExecuteQuery<TResult>(string query, params object[] parameters);");
            Writer.WriteLine(Tabs + "IEnumerable ExecuteQuery(Type elementType, string query, params object[] parameters);");
            Writer.WriteLine(Tabs + "ChangeSet GetChangeSet();");
            Writer.WriteLine(Tabs + "DbCommand GetCommand(IQueryable query);");
            Writer.WriteLine(Tabs + "ITable<TEntity> GetTable<TEntity>() where TEntity : class;");
            Writer.WriteLine(Tabs + "ITable GetTable(Type type);");
            Writer.WriteLine(Tabs + "void Refresh(RefreshMode mode, params object[] entities);");
            Writer.WriteLine(Tabs + "void Refresh(RefreshMode mode, IEnumerable entities);");
            Writer.WriteLine(Tabs + "void Refresh(RefreshMode mode, object entity);");
            Writer.WriteLine(Tabs + "void SubmitChanges();");
            Writer.WriteLine(Tabs + "void SubmitChanges(ConflictMode failureMode);");
            Writer.WriteLine(Tabs + "IEnumerable<TResult> Translate<TResult>(DbDataReader reader);");
            Writer.WriteLine(Tabs + "IMultipleResults Translate(DbDataReader reader);");
            Writer.WriteLine(Tabs + "IEnumerable Translate(Type elementType, DbDataReader reader);");
            Writer.WriteLine(Tabs + "ChangeConflictCollection ChangeConflicts { get; }");
            Writer.WriteLine(Tabs + "int CommandTimeout { get; set; }");
            Writer.WriteLine(Tabs + "DbConnection Connection { get; }");
            Writer.WriteLine(Tabs + "bool DeferredLoadingEnabled { get; set; }");
            Writer.WriteLine(Tabs + "DataLoadOptions LoadOptions { get; set; }");
            Writer.WriteLine(Tabs + "TextWriter Log { get; set; }");
            Writer.WriteLine(Tabs + "MetaModel Mapping { get; }");
            Writer.WriteLine(Tabs + "bool ObjectTrackingEnabled { get; set; }");
            Writer.WriteLine(Tabs + "DbTransaction Transaction { get; set; }");
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
                Writer.WriteLine(Tabs + string.Format("{0}{3} {1}({2});", function.ReturnType, function.MethodName,
                    function.GetSignature(), function.IsComposable && function.ReturnType != "System.String" ? "?" : string.Empty));
            else
                Writer.WriteLine(Tabs + string.Format("{0}<{1}> {2}({3});",
                                                      function.IsComposable ? "IQueryable" : "ISingleResult",
                                                      function.ReturnElement, function.MethodName,
                                                      function.GetSignature()));
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
            Writer.WriteLine(Tabs + string.Format("ITable<{0}> {1} {{ get; }}", type.ClassName, type.MemberName));
        }

        private void OutputNamespace()
        {
            Writer.WriteLine("using System;");
            Writer.WriteLine("using System.Data.Linq;");
            Writer.WriteLine("using System.Collections;");
            Writer.WriteLine("using System.Collections.Generic;");
            Writer.WriteLine("using System.Data.Common;");
            Writer.WriteLine("using System.Data.Linq.Mapping;");
            Writer.WriteLine("using System.IO;");
            Writer.WriteLine("using System.Linq;");
            Writer.WriteLine("");
            Writer.WriteLine("namespace " + _definition.EntityNamespace);
            OpenBrace();
        }

        private void OpenBrace()
        {
            Writer.WriteLine(Tabs + "{");
            _tabCount++;
        }

        private void CloseBrace()
        {
            _tabCount--;
            Writer.WriteLine(Tabs + "}");
        }

        private string Tabs
        {
            get { return _tabCount > 0 ? new string('\t', _tabCount) : string.Empty; }
        }

        //public partial interface I***DataContext
        //
        //

        //public partial class DataContextProxy : I***DataContext
        //
        //

        //Tables
        //Interface:
        //ITable<#ClassName#> #MemberName { get; }
        //
        //Class:
        //public ITable<#ClassName#> #MemberName#
        //{
        //  get
        //  {
        //      return new TableProxy<#ClassName#>(_context.#MemberName#);
        //  }
        //}


    }
}

