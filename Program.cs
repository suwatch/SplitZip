using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace SplitZip
{
    static class Program
    {
        static List<string> _directories = new List<string>();

        static FileInfo _zipFile;
        static DirectoryInfo _dir;
        static int _index = 0;
        static long _size = 0;
        static ZipArchive _curr = null;
        static long _maxSize = 0;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: SplitZip.exe zipfile sizeMB");
                Console.WriteLine("Usage: SplitZip.exe dir sizeMB");
                return;
            }

            try
            {
                if (args[0].EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    _zipFile = new FileInfo(args[0]);
                }
                else
                {
                    _dir = new DirectoryInfo(args[0]);
                    _zipFile = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), $"{_dir.Name}.zip"));
                }

                _maxSize = long.Parse(args[1]) * 1024 * 1024;

                if (_dir == null)
                {
                    using (var stream = _zipFile.OpenRead())
                    using (var zipArchive = new ZipArchive(stream, ZipArchiveMode.Read))
                    {
                        zipArchive.Split();
                    }
                }
                else
                {
                    foreach (var info in _dir.EnumerateFileSystemInfos("*", SearchOption.AllDirectories))
                    {
                        var entryName = info.FullName.Substring(_dir.FullName.Length + 1).Replace('\\', '/');
                        if (info is DirectoryInfo)
                        {
                            var dst = GetCurrentZipArchive(1);
                            var result = dst.CreateEntry(entryName.Trim('/') + '/');
                            result.LastWriteTime = info.LastWriteTime;
                            _directories.Add(entryName.Trim('/') + '/');
                        }
                        else
                        {
                            var fileInfo = (FileInfo)info;
                            var dst = GetCurrentZipArchive(fileInfo.Length / 3 + 1);
                            var result = dst.CreateEntry(entryName, CompressionLevel.Fastest);
                            result.LastWriteTime = info.LastWriteTime;

                            using (var srcStream = fileInfo.OpenRead())
                            using (var dstStream = result.Open())
                            {
                                srcStream.CopyTo(dstStream);
                            }
                        }
                    }
                }

                if (_curr != null)
                {
                    Console.WriteLine($"{_size,11:n0} bytes");
                    _curr.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static ZipArchive GetCurrentZipArchive(long size)
        {
            if (_curr == null || _size == 0 || (_size + size) > _maxSize)
            {
                _curr?.Dispose();

                if (_curr != null)
                {
                    Console.WriteLine($"{_size,11:n0} bytes");
                    _curr.Dispose();
                }

                // initial create
                _curr = new ZipArchive(NextStream(), ZipArchiveMode.Create, leaveOpen: false);
                _size = size;

                // prep all paths so far
                foreach (var path in _directories)
                {
                    _curr.CreateEntry(path);
                }
            }
            else
            {
                _size += size;
            }

            return _curr;
        }

        public static Stream NextStream()
        {
            var fileName = $"{Path.GetFileNameWithoutExtension(_zipFile.Name)}_{_index:000}{_zipFile.Extension}";
            Console.Write($"Writing {fileName} ... ");

            var fileInfo = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), fileName));
            ++_index;
            return fileInfo.Open(FileMode.Create, FileAccess.Write);
        }

        public static void Split(this ZipArchive archive)
        {
            foreach (var entry in archive.Entries)
            {
                if (entry.Length == 0 && (entry.FullName.EndsWith("/", StringComparison.Ordinal) || entry.FullName.EndsWith("\\", StringComparison.Ordinal)))
                {
                    var dst = GetCurrentZipArchive(1);
                    var result = dst.CreateEntry(entry.FullName);
                    result.LastWriteTime = entry.LastWriteTime;
                    _directories.Add(entry.FullName);
                }
                else
                {
                    var dst = GetCurrentZipArchive(entry.CompressedLength);
                    var result = dst.CreateEntry(entry.FullName, CompressionLevel.Fastest);
                    result.LastWriteTime = entry.LastWriteTime;

                    using (var srcStream = entry.Open())
                    using (var dstStream = result.Open())
                    {
                        srcStream.CopyTo(dstStream);
                    }
                }
            }
        }
    }
}
