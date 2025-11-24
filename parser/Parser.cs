using HandHistoryParser.exception;
using HandHistoryParser.model;
using HandHistoryParser.utils;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using static System.Net.Mime.MediaTypeNames;

namespace HandHistoryParser.parser
{
    internal class Parser(ILogger<Parser> logger)
    {
        private const string HOLE_CARDS_SECTION_HEADER = "*** HOLE CARDS ***";
        private const string FLOP_SECTION_HEADER = "*** FLOP ***";
        private const string TERN_SECTION_HEADER = "*** TURN ***";
        private const string RIVER_SECTION_HEADER = "*** RIVER ***";
        private const string SHOW_DOWN_SECTION_HEADER = "*** SHOW DOWN ***";
        private const string SUMMARY_SECTION_HEADER = "*** SUMMARY ***";
        private const string HAND_NUMBER_TEXT = "Hand #";
        private const string SEAT_TEXT = "Seat";
        private const string DEALT_TO_TEXT = "Dealt to";
        private const string ROWS_DELIMETER = "\r\n";
        private const int PLAYER_INFO_PARTS_LENGTH = 6;
        private const int PLAYER_INFO_SEAT_NUMBER_PART_INDEX = 1;
        private const int PLAYER_INFO_NICKNAME_PART_INDEX = 2;
        private const int PLAYER_INFO_STACK_PART_INDEX = 3;
        private const int DEALT_TO_HERO_NICKNAME_PART_INDEX = 2;
        private const string HAND_HOSTORIES_DELIMETER = "\r\n\r\n\r\n\r\n";

        private readonly ILogger<Parser> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public void ParseZipAndWriteOutput(string zipFilePath)
        {
            if (_logger.IsEnabled(LogLevel.Information)) _logger.LogInformation("Start prasing: {zipFilePath}", zipFilePath);

            string archiveName = Path.GetFileNameWithoutExtension(zipFilePath);
            string dateString = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string outputDirectory = Path.Combine("resources", "parsed");
            string outputFileName = $"{dateString}_{archiveName}.txt";
            string outputFilePath = Path.Combine(outputDirectory, outputFileName);

            Directory.CreateDirectory(outputDirectory);

            using StreamWriter writer = new(outputFilePath);
            using ZipArchive archive = ZipFile.OpenRead(zipFilePath);
            List<HandHistoryData> parsedData = ParseZipArchive(archive);
            writer.WriteLine(string.Join("\r\n", parsedData.Where(e => !e.Equals(new()))));
        }

        private List<HandHistoryData> ParseZipArchive(ZipArchive archive)
        {
            List<HandHistoryData> parsedData = [];
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (entry.FullName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        if (_logger.IsEnabled(LogLevel.Information)) _logger.LogInformation("--Prasing: {file}", entry.FullName);
                        using StreamReader reader = new(entry.Open());
                        string content = reader.ReadToEnd();
                        parsedData.AddRange(ParseFileContent(content, HAND_HOSTORIES_DELIMETER));
                    }
                    catch (Exception ex)
                    {
                        if (_logger.IsEnabled(LogLevel.Error))
                        {
                            _logger.LogError("Error processing {entry.FullName}: {ex.Message}", entry.FullName, ex.Message);
                        }
                    }
                }
            }
            return parsedData;
        }

        private List<HandHistoryData> ParseFileContent(string content, string delimeter)
        {
            return [.. content.Split(delimeter).Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => Parse(e))];
        }

        internal HandHistoryData Parse(string handHistory)
        {
            return ParseSections(handHistory);
        }

        private HandHistoryData ParseSections(string handHistory)
        {
            if (_logger.IsEnabled(LogLevel.Information)) _logger.LogInformation("--Prasing sections...");
            HandHistoryData data = new();

            var notParsedlines = handHistory.Split("\r\n");

            data = ParseHeaderSection(data, string.Join("\r\n", notParsedlines.TakeWhile(e => !e.StartsWith("***"))));
            notParsedlines = [.. notParsedlines.SkipWhile(row => !row.StartsWith("***"))];

            while (notParsedlines.Length > 0)
            {
                string header = notParsedlines[0];
                notParsedlines = notParsedlines[1..];
                string section = string.Join("\r\n", notParsedlines.TakeWhile(e => !e.StartsWith("***")));
                notParsedlines = [.. notParsedlines.SkipWhile(row => !row.StartsWith("***"))];
                data = ParseSection(data, header, section);
            }
            
            return data;
        }

        private HandHistoryData ParseSection(HandHistoryData data, string sectionName, string section)
        {
            return sectionName.Trim() switch
            {
                "HEADER" => ParseHeaderSection(data, section),
                HOLE_CARDS_SECTION_HEADER => ParseHoleCardsSection(data, section),
                _ => data,
            };
        }

        private HandHistoryData ParseHoleCardsSection(HandHistoryData data, string section)
        {
            if (_logger.IsEnabled(LogLevel.Information)) _logger.LogInformation("--Prasing hole cards section...");
            List<string> dealtToRows = [.. section.Split(ROWS_DELIMETER).Where(e => e.Trim().StartsWith(DEALT_TO_TEXT))];
            if (dealtToRows.Count != 1)
            {
                throw new ArgumentException("The number of rows with '" + DEALT_TO_TEXT + "' is " + dealtToRows.Count, nameof(section));
            }

            Player hero = ParseCardsDealtToHero(dealtToRows.First());
            return data.WithNewPlayers([.. data.Players.Select(p => p.Equals(hero) ? p.updateCards(hero.Cards) : p)]);
        }

        private Player ParseCardsDealtToHero(string dealtToRow)
        {
            if (_logger.IsEnabled(LogLevel.Information)) _logger.LogInformation("--Prasing dealt to cards part: {dealtToRow}", dealtToRow);
            string[] dealtToParts = dealtToRow.Split(" ");
            string heroNickname = dealtToParts[DEALT_TO_HERO_NICKNAME_PART_INDEX];

            int startIndex = dealtToRow.IndexOf('[') + 1;
            int endIndex = dealtToRow.IndexOf(']');

            if (startIndex == - 1 || endIndex == -1 )
            {
                throw new DataNotFoundException("Could not find hero cards");
            }

            List<Card> heroCards = [.. dealtToRow[startIndex..endIndex]
                .Split(" ")
                .Select(pair => new Card(pair[0], pair[1]))];

            return new Player(-1, heroNickname, decimal.Zero, '$', heroCards);
        }

        private HandHistoryData ParseHeaderSection(HandHistoryData data, string? headerSection)
        {
            if (_logger.IsEnabled(LogLevel.Information)) _logger.LogInformation("--Prasing header section...");
            if (String.IsNullOrEmpty(headerSection)) { return data; }

            long handHistoryNumber = ParseHandHistoryNumber(headerSection);
            List<Player> players = ParseHandHistoryPlayers(headerSection);
            
            return data
                .WithNewHandHistoryNumber(handHistoryNumber)
                .WithNewPlayers(players);
        }

        private List<Player> ParseHandHistoryPlayers(string headerSection)
        {
            if (_logger.IsEnabled(LogLevel.Information)) _logger.LogInformation("--Prasing players part...");
            List<Player> players = [];
            players.AddRange([.. headerSection.Split(ROWS_DELIMETER)
                .Where(row => row.StartsWith(SEAT_TEXT))
                .Select(ParsePlayerInfo)]);
            return players;
        }

        private Player ParsePlayerInfo(string playerInfoRow)
        {
            if (_logger.IsEnabled(LogLevel.Information)) _logger.LogInformation("--Prasing playerInfoRow: {playerInfoRow}", playerInfoRow);
            int indexSeatSepNick = playerInfoRow.IndexOf(':');
            int indexNickSepStack = playerInfoRow.IndexOf('(');

            while (playerInfoRow[indexNickSepStack + 1] != '$' && playerInfoRow[indexNickSepStack + 1] != '€')
            {
                indexNickSepStack = playerInfoRow.IndexOf('(', indexNickSepStack + 1);
                if (indexNickSepStack == -1) break;
            }
            if (indexSeatSepNick == -1 || indexNickSepStack == -1)
            {
                throw new DataNotFoundException("Player info row has wrong structure: " + playerInfoRow);
            }

            int seatNumber = Converter.ParseInt(playerInfoRow[..indexSeatSepNick].Split(' ')[1]);
            string nickname = playerInfoRow[(indexSeatSepNick + 1)..indexNickSepStack].Trim();
            decimal stack = Converter.ParseDecimal(playerInfoRow[(indexNickSepStack+2)..]);
            char currency = playerInfoRow[(indexNickSepStack+1)..][0];

            return new Player(seatNumber, nickname, stack, currency, []);
        }

        private long ParseHandHistoryNumber(string section)
        {
            if (_logger.IsEnabled(LogLevel.Information)) _logger.LogInformation("--Prasing hand history number");
            int handNumberTextIndex = section.IndexOf(HAND_NUMBER_TEXT);
            if (handNumberTextIndex == -1)
            {
                throw new DataNotFoundException("'" + HAND_NUMBER_TEXT + "' Not Found in '" + section + "'.");
            }

            return Converter.ParseLong(section, handNumberTextIndex + HAND_NUMBER_TEXT.Length);
        }
    }
}
