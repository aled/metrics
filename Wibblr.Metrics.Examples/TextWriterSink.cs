﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace Wibblr.Metrics.Core
{
    public class TextWriterSink : IMetricsSink
    {
        private TextWriter writer;

        public TextWriterSink(TextWriter writer)
        {
            this.writer = writer;
        }

        private string Printable(string s) =>
            new String(s.SelectMany(c => Char.IsControl(c) ? ("0x" + ((int)c).ToString("X4")).ToCharArray() : new char[] { c }).ToArray());

        public void Flush(IEnumerable<WindowedCounter> events)
        {
            var lines = events
                .OrderBy(x => x.window.start)
                .ThenBy(x => x.name)
                .Select(x => $"{x.window.start.ToString("mm:ss.fff")} - {x.window.end.ToString("mm:ss.fff")} {Printable(x.name)} {x.count}");

            foreach (var line in lines)
                writer.WriteLine(line);

            writer.WriteLine("---");

            writer.Flush();
        }

        public void Flush(IEnumerable<WindowedBucket> buckets)
        {
            var lines = buckets
                .OrderBy(x => x.window.start)
                .ThenBy(x => x.name)
                .ThenBy(x => x.from)
                .Select(x => $"{x.window.start.ToString("mm:ss.fff")} - {x.window.end.ToString("mm:ss.fff")} {Printable(x.name)} {x.from}-{x.to} {x.count}");

            foreach (var line in lines)
                writer.WriteLine(line);

            writer.WriteLine("---");

            writer.Flush();
        }
    }
}