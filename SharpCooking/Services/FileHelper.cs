﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SharpCooking.Services
{
    public class FileHelper : IFileHelper
    {
        public Task<bool> ExistsAsync(string filename)
        {
            string filepath = GetFilePath(filename);
            bool exists = File.Exists(filepath);
            return Task<bool>.FromResult(exists);
        }

        public async Task WriteTextAsync(string filename, string text)
        {
            string filepath = GetFilePath(filename);
            using (StreamWriter writer = File.CreateText(filepath))
            {
                await writer.WriteAsync(text);
            }
        }

        public Stream ReadStream(string filename)
        {
            if (string.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));

            string filepath = GetFilePath(filename);
            return File.OpenRead(filepath);
        }

        public byte[] ReadBytes(string filename)
        {
            if (string.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));

            string filepath = GetFilePath(filename);
            return File.ReadAllBytes(filepath);
        }

        public async Task WriteStreamAsync(string filename, Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (string.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));

            string filepath = GetFilePath(filename);
            using (var file = File.Create(filepath))
            {
                stream.Seek(0, SeekOrigin.Begin);
                await stream.CopyToAsync(file);
            }
        }

        public async Task CopyAsync(string sourceFilePath, string destinationFileName)
        {
            if (string.IsNullOrEmpty(sourceFilePath)) throw new ArgumentNullException(nameof(sourceFilePath));
            if (string.IsNullOrEmpty(destinationFileName)) throw new ArgumentNullException(nameof(destinationFileName));

            string destinationFilePath = GetFilePath(destinationFileName);
            using (var destinationFile = File.Create(destinationFilePath))
            {
                using (var sourceFile = File.OpenRead(sourceFilePath))
                {
                    await sourceFile.CopyToAsync(destinationFile);
                }
            }
        }

        public Task MoveAsync(string sourceFilePath, string destinationFileName)
        {
            string destinationFilePath = GetFilePath(destinationFileName);

            return Task.Run(() =>
            {
                File.Move(sourceFilePath, destinationFilePath);
            });
        }

        public async Task<string> ReadTextAsync(string filename)
        {
            string filepath = GetFilePath(filename);
            using (StreamReader reader = File.OpenText(filepath))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public Task<IEnumerable<string>> GetAllAsync()
        {
            return Task.Run(() =>
            {
                return (IEnumerable<string>)Directory.GetFiles(GetDocsFolder());
            });
        }

        public Task DeleteAsync(string filename)
        {
            File.Delete(GetFilePath(filename));
            return Task.FromResult(true);
        }

        public string GetDocsFolder()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        public string GetFilePath(string filename)
        {
            var actualFileName =  Path.GetFileName(filename);
            return Path.Combine(GetDocsFolder(), actualFileName);
        }
    }
}
