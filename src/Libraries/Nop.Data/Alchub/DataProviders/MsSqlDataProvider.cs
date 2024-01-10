using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Infrastructure;

namespace Nop.Data.DataProviders
{
    /// <summary>
    /// Represents the MS SQL Server data provider
    /// </summary>
    public partial class MsSqlNopDataProvider
    {
        #region SQL

        public async Task ExecuteSqlFileScriptsOnDatabase()
        {
            var _fileProvider = EngineContext.Current.Resolve<INopFileProvider>();
            var customCommands = new List<string>();
            customCommands.AddRange(ParseCommands(_fileProvider.MapPath("~/Alchub/App_Data/Install/SqlServer.StoredProcedures.sql"), false));

            if (customCommands != null && customCommands.Any())
            {
                foreach (var command in customCommands)
                    await ExecuteNonQueryAsync(command);
            }
        }

        protected virtual string[] ParseCommands(string filePath, bool throwExceptionIfNonExists)
        {
            if (!File.Exists(filePath))
            {
                if (throwExceptionIfNonExists)
                    throw new ArgumentException($"Specified file doesn't exist - {filePath}");

                return new string[0];
            }

            var statements = new List<string>();
            using (var stream = File.OpenRead(filePath))
            using (var reader = new StreamReader(stream))
            {
                string statement;
                while ((statement = ReadNextStatementFromStream(reader)) != null)
                {
                    statements.Add(statement);
                }
            }

            return statements.ToArray();
        }

        protected virtual string ReadNextStatementFromStream(StreamReader reader)
        {
            var sb = new StringBuilder();

            while (true)
            {
                var lineOfText = reader.ReadLine();
                if (lineOfText == null)
                {
                    if (sb.Length > 0)
                        return sb.ToString();

                    return null;
                }

                if (lineOfText.TrimEnd().ToUpper() == "GO")
                    break;

                sb.Append(lineOfText + Environment.NewLine);
            }

            return sb.ToString();
        }

        #endregion
    }
}
