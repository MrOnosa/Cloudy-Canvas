﻿namespace Cloudy_Canvas.Service
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Threading.Tasks;
    using Discord.Commands;
    using Microsoft.Extensions.Logging;

    public class LoggingHelperService
    {
        private readonly ILogger<Worker> _logger;

        public LoggingHelperService(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        public Task Log(string message, SocketCommandContext context)
        {
            AppendToFile(message, context);
            var logMessage = PrepareMessageForLogging(message, context);
            _logger.LogInformation(logMessage);
            return Task.CompletedTask;
        }

        private static string PrepareMessageForLogging(string message, SocketCommandContext context, bool fileEntry = false, bool header = false)
        {
            var logMessage = "";
            if (!header)
            {
                logMessage += $"[{DateTime.UtcNow.ToString(CultureInfo.CurrentCulture)}] ";
            }

            if (context.IsPrivate)
            {
                if (!fileEntry)
                {
                    logMessage += $"DM with @{context.User.Username}#{context.User.Discriminator} ({context.User.Id})";
                }

                if (!fileEntry && !header)
                {
                    logMessage += ", ";
                }

                if (!header)
                {
                    logMessage += message;
                }
            }
            else
            {
                if (!fileEntry)
                {
                    logMessage += $"server: {context.Guild.Name} ({context.Guild.Id}) #{context.Channel.Name} ({context.Channel.Id})";
                }

                if (!fileEntry && !header)
                {
                    logMessage += " ";
                }

                if (!header)
                {
                    logMessage += $"@{context.User.Username}#{context.User.Discriminator} ({context.User.Id}), ";
                    logMessage += message;
                }
            }

            if (fileEntry || header)
            {
                logMessage += "\n";
            }

            return logMessage;
        }

        private static void AppendToFile(string message, SocketCommandContext context)
        {
            var filepath = SetUpFilepath(context);
            if (!File.Exists(filepath))
            {
                File.WriteAllText(filepath, PrepareMessageForLogging(message, context, false, true));
            }

            var logMessage = PrepareMessageForLogging(message, context, true);
            File.AppendAllText(filepath, logMessage);
        }

        private static string SetUpFilepath(SocketCommandContext context)
        {
            var filepath = "Logs/";
            CreateDirectoryIfNotExists(filepath);
            if (context.IsPrivate)
            {
                filepath += "_UserDMs/";
                CreateDirectoryIfNotExists(filepath);
                filepath += $"@{context.User.Username}/";
                CreateDirectoryIfNotExists(filepath);
            }
            else
            {
                filepath += $"{context.Guild.Name}/";
                CreateDirectoryIfNotExists(filepath);
                filepath += $"#{context.Channel.Name}/";
                CreateDirectoryIfNotExists(filepath);
            }

            filepath += $"{DateTime.Today.ToShortDateString()}.txt";
            return filepath;
        }

        private static void CreateDirectoryIfNotExists(string path)
        {
            var directory = new DirectoryInfo(path);
            if (!directory.Exists)
            {
                directory.Create();
            }
        }
    }
}
