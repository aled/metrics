﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Wibblr.Metrics.Core;

namespace Wibblr.Metrics.SqlServer
{
    public class SqlServerSink : IMetricsSink
    {
        internal int BatchSize { get; set; } = 10000;
        internal int MaxQueuedRows { get; set; } = 100000;

        const int TIMEOUT_SECONDS = 60;

        private string connectionString;
        private string tableName;

        internal DataTable dataTable;
        private object dataTableLock = new object();

        public SqlServerSink(string connectionString, string tableName)
        {
            this.connectionString = connectionString;
            this.tableName = tableName;

            dataTable = new DataTable();
            dataTable.Columns.Add("EventName", typeof(string));
            dataTable.Columns.Add("StartTime", typeof(DateTime));
            dataTable.Columns.Add("EndTime", typeof(DateTime));
            dataTable.Columns.Add("Count", typeof(long));
        }

        public void Flush(IEnumerable<AggregatedCounter> counters)
        {
            lock (dataTableLock)
            {
                foreach (var c in counters)
                {
                    // Throw away new rows when we have too many queued, so that we
                    // don't consume unlimited memory if the database is down.
                    // TODO: aggregate them into coarser time periods instead.
                    if (dataTable.Rows.Count >= MaxQueuedRows)
                        break;

                    var row = dataTable.NewRow();
                    row["CounterName"] = c.name;
                    row["StartTime"] = c.window.start;
                    row["EndTime"] = c.window.start.Add(c.window.size);
                    row["Count"] = c.count;
                    dataTable.Rows.Add(row);
                }

                try
                {
                    using (var bc = new SqlBulkCopy(connectionString, SqlBulkCopyOptions.UseInternalTransaction))
                    {
                        bc.DestinationTableName = tableName;
                        bc.BatchSize = BatchSize;
                        bc.BulkCopyTimeout = TIMEOUT_SECONDS;
                        bc.EnableStreaming = false;
                        bc.ColumnMappings.Add("EventName", "EventName");
                        bc.ColumnMappings.Add("StartTime", "StartTime");
                        bc.ColumnMappings.Add("EndTime", "EndTime");
                        bc.ColumnMappings.Add("Count", "Count");
                        bc.WriteToServer(dataTable);
                    }
                    dataTable.Clear();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}